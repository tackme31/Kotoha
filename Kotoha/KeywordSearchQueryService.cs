using Sitecore.Configuration;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Kotoha
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
            Assert.IsNotNull(queryable, $"'{nameof(queryable)}' should not be null.");
            Assert.IsNotNull(keywords, $"'{nameof(keywords)}' should not be null.");

            var matchPred = keywords
                .Aggregate(PredicateBuilder.True<T>(),
                    (acc, keyword) => acc.And(item => item[$"{_config.ContentField.Name}_t"].Contains(keyword)));

            var boostPred = _config.BoostedFields
                .SelectMany(_ => keywords, (field, keyword) => (field, keyword))
                .Aggregate(PredicateBuilder.False<T>(),
                    (acc, pair) => acc.Or(item => item[$"{pair.field.Name}_t"].Contains(pair.keyword).Boost(pair.field.Boost)));

            return queryable.Filter(matchPred).Where(boostPred);
        }
    }
}
