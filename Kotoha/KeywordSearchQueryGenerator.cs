using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kotoha
{
    public static class KeywordSearchQueryGenerator
    {
        public static IQueryable<T> ApplyKeywordSearchQuery<T>(this ISearchIndex index, IQueryable<T> queryable, string searchTargetId, ICollection<string> keywords) where T : SearchResultItem
        {
            Assert.ArgumentNotNull(index, nameof(index));
            Assert.ArgumentNotNull(queryable, nameof(queryable));
            Assert.ArgumentNotNullOrEmpty(searchTargetId, nameof(searchTargetId));
            Assert.ArgumentNotNull(keywords, nameof(keywords));

            var config = Factory.CreateObject("kotoha/configuration", true) as KeywordSearchConfiguration;
            var searchTarget = config.GetSearchTargetById(searchTargetId);
            if (searchTarget == null)
            {
                throw new ArgumentException($"Keyword search target was not found. (ID: {searchTargetId})");
            }

            var contentField = index.Configuration.DocumentOptions.ComputedIndexFields
                .OfType<KeywordSearchContentIndexField>()
                .FirstOrDefault(field => field.SearchTargetId == searchTargetId);
            if (contentField == null)
            {
                throw new ArgumentException($"Keyword search content field was not found. (ID: {searchTargetId})");
            }

            var matchPred = keywords
                .Aggregate(
                    PredicateBuilder.True<T>(),
                    (acc, keyword) => acc.Or(item => item[contentField.FieldName].Contains(keyword)));

            var boostPred = searchTarget.Fields
                .Where(field => field.Boost > 0.0f)
                .SelectMany(_ => keywords, (field, keyword) => (field, keyword))
                .Aggregate(
                    PredicateBuilder.False<T>(),
                    (acc, pair) => acc.Or(item => item[pair.field.Name].Contains(pair.keyword).Boost(pair.field.Boost)));

            return queryable.Filter(matchPred).Where(boostPred);
        }
    }
}
