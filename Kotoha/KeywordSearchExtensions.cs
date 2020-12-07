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
            return query.AddKeywordSearchQuery(searchTargetId, keywords, context, options);
        }

        public static IQueryable<T> AddKeywordSearchQuery<T>(this IQueryable<T> query, string searchTargetId, ICollection<string> keywords, IProviderSearchContext context, KeywordSearchOptions options = null) where T : SearchResultItem
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

            var condition = options?.Condition ?? config.DefaultKeywordSearchOptions?.Condition ?? default;
            var searchType = options?.SearchType ?? config.DefaultKeywordSearchOptions?.SearchType ?? default;
            var filterPred = keywords.Aggregate(
                PredicateBuilder.True<T>(),
                (acc, keyword) =>
                {
                    switch (condition)
                    {
                        case Condition.Contains when searchType == SearchType.And:
                            return acc.And(item => item[contentField.FieldName].Contains(keyword));
                        case Condition.Contains when searchType == SearchType.Or:
                            return acc.Or(item => item[contentField.FieldName].Contains(keyword));
                        case Condition.Equals when searchType == SearchType.And:
                            return acc.And(item => item[contentField.FieldName].Equals(keyword));
                        case Condition.Equals when searchType == SearchType.Or:
                            return acc.Or(item => item[contentField.FieldName].Equals(keyword));
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });

            return query.Filter(filterPred).AddKeywordBoostQuery(searchTargetId, keywords, context, options);
        }

        public static IQueryable<T> AddKeywordBoostQuery<T>(this IQueryable<T> query, string searchTargetId, ICollection<string> keywords, IProviderSearchContext context, KeywordSearchOptions options = null) where T : SearchResultItem
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

            var condition = options?.Condition ?? config.DefaultKeywordSearchOptions?.Condition ?? default;
            var boostPred = searchTarget.Fields
                .Where(field => field.Boost > 0.0f)
                .SelectMany(_ => keywords, (field, keyword) => (field, keyword))
                .Aggregate(
                    PredicateBuilder.Create<T>(item => item.Name.MatchWildcard("*").Boost(0)), // always true
                    (acc, pair) =>
                    {
                        switch (condition)
                        {
                            case Condition.Contains:
                                return acc.Or(item => item[pair.field.Name].Contains(pair.keyword).Boost(pair.field.Boost));
                            case Condition.Equals:
                                return acc.Or(item => item[pair.field.Name].Equals(pair.keyword).Boost(pair.field.Boost));
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });

            return query.Where(boostPred);
        }
    }
}
