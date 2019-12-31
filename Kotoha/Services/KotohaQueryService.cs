using Kotoha.Models;
using Sitecore.Configuration;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Kotoha.Services
{
    public interface IKotohaQueryService<T>
    {
        IQueryable<T> ApplyKeywordSearchQuery(IQueryable<T> queryable, ICollection<string> keywords);
    }

    public class KotohaQueryService<TResult> : IKotohaQueryService<TResult> where TResult : SearchResultItem
    {
        private readonly KotohaConfiguration _configuration = Factory.CreateObject("kotoha/KotohaConfiguration", true) as KotohaConfiguration;

        public IQueryable<TResult> ApplyKeywordSearchQuery(IQueryable<TResult> queryable, ICollection<string> keywords)
        {
            Assert.IsNotNull(queryable, $"'{nameof(queryable)}' should not be null or empty.");
            Assert.IsNotNull(keywords, $"'{nameof(keywords)}' should not be null or empty.");

            var matchPred = keywords
                .Aggregate(
                    PredicateBuilder.True<TResult>(),
                    (acc, keyword) => acc.And(item => item[_configuration.ContentFieldName].Contains(keyword)));

            var boostPred = _configuration.TargetFieldGroups
                .SelectMany(_ => keywords, (field, keyword) => (field: field.Name, keyword, boost: field.Boost))
                .Aggregate(
                    PredicateBuilder.False<TResult>(),
                    (acc, pair) => acc.Or(item => item[pair.field].Contains(pair.keyword).Boost(pair.boost)));

            return queryable.Filter(matchPred).Where(boostPred);
        }
    }
}
