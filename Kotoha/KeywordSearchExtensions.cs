using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Kotoha
{
    public static class KeywordSearchExtensions
    {
        public static IQueryable<T> CreateKeywordSearchQuery<T>(this IProviderSearchContext context, string searchTargetId, ICollection<string> keywords, KeywordSearchOptions options = null) where T : SearchResultItem
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNullOrEmpty(searchTargetId, nameof(searchTargetId));
            Assert.ArgumentNotNull(keywords, nameof(keywords));

            var query = context.GetQueryable<T>();
            return context.AddKeywordSearchQuery(query, searchTargetId, keywords, options);
        }

        public static IQueryable<T> AddKeywordSearchQuery<T>(this IProviderSearchContext context, IQueryable<T> query, string searchTargetId, ICollection<string> keywords, KeywordSearchOptions options = null) where T : SearchResultItem
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNullOrEmpty(searchTargetId, nameof(searchTargetId));
            Assert.ArgumentNotNull(keywords, nameof(keywords));

            if (!keywords.Any())
            {
                return query;
            }

            var config = Factory.CreateObject("kotoha/configuration", true) as KeywordSearchConfiguration;
            var searchTarget = config.GetSearchTargetById(searchTargetId);
            if (searchTarget == null)
            {
                throw new InvalidOperationException($"Keyword search target was not found. (ID: {searchTargetId})");
            }

            var contentField = context.Index.Configuration.DocumentOptions.ComputedIndexFields
                .OfType<KeywordSearchContentIndexField>()
                .FirstOrDefault(field => field.SearchTargetId == searchTargetId);
            if (contentField == null)
            {
                throw new InvalidOperationException($"Keyword search content field was not found. (ID: {searchTargetId})");
            }

            var generateMatchPred = CreatePredicateGenerator<T>(options?.Condition ?? default, options?.SearchType ?? default);
            var matchPred = keywords
                .Aggregate(
                    PredicateBuilder.True<T>(),
                    (acc, keyword) => generateMatchPred(acc, contentField.FieldName, keyword, 0));

            // Boost predicate must use OR operator
            var generateBoostPred = CreatePredicateGenerator<T>(options?.Condition ?? default, SearchType.Or);
            var boostPred = searchTarget.Fields
                .Where(field => field.Boost > 0.0f)
                .SelectMany(_ => keywords, (field, keyword) => (field, keyword))
                .Aggregate(
                    PredicateBuilder.Create<T>(item => item.Name.MatchWildcard("*").Boost(0)), // Make the predicate return always true
                    (acc, pair) => generateBoostPred(acc, pair.field.Name, pair.keyword, pair.field.Boost));

            return query.Filter(matchPred).Where(boostPred);
        }

        private static Func<Expression<Func<T, bool>>, string, string, float, Expression<Func<T, bool>>> CreatePredicateGenerator<T>(Condition condition, SearchType searchType) where T : SearchResultItem
        {
            switch (condition)
            {
                case Condition.Contains when searchType == SearchType.And:
                    return (acc, fieldName, value, boost) => acc.And(item => item[fieldName].Contains(value).Boost(boost));
                case Condition.Contains when searchType == SearchType.Or:
                    return (acc, fieldName, value, boost) => acc.Or(item => item[fieldName].Contains(value).Boost(boost));
                case Condition.Equals when searchType == SearchType.And:
                    return (acc, fieldName, value, boost) => acc.And(item => item[fieldName].Equals(value).Boost(boost));
                case Condition.Equals when searchType == SearchType.Or:
                    return (acc, fieldName, value, boost) => acc.Or(item => item[fieldName].Equals(value).Boost(boost));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
