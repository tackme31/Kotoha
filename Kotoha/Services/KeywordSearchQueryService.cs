using Kotoha.Configs;
using Sitecore.Configuration;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Kotoha.Services
{
    public interface IKeywordSearchQueryService
    {
        IQueryable<T> ApplyKeywordSearchQuery<T>(IQueryable<T> queryable, ICollection<string> keywords) where T : SearchResultItem;
    }

    public class KeywordSearchQueryService : IKeywordSearchQueryService
    {
        private readonly KeywordSearchConfiguration _config = Factory.CreateObject("kotoha/KeywordSearchConfiguration", true) as KeywordSearchConfiguration;

        public IQueryable<T> ApplyKeywordSearchQuery<T>(IQueryable<T> queryable, ICollection<string> keywords) where T : SearchResultItem
        {
            Assert.IsNotNull(queryable, $"'{nameof(queryable)}' should not be null or empty.");
            Assert.IsNotNull(keywords, $"'{nameof(keywords)}' should not be null or empty.");

            var matchPred = keywords
                .Aggregate(PredicateBuilder.True<T>(),
                    (acc, keyword) => acc.And(item => item[_config.ContentField.Name].Contains(keyword)));

            var boostPred = _config.BoostedFields
                .SelectMany(_ => keywords, (field, keyword) => (field: field.Name, keyword, boost: field.Boost))
                .Aggregate(PredicateBuilder.False<T>(),
                    (acc, pair) => acc.Or(item => item[pair.field].Contains(pair.keyword).Boost(pair.boost)));

            return queryable.Filter(matchPred).Where(boostPred);
        }
    }
}
