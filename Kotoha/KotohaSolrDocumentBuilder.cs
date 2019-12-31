using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.Data.Items;
using System.Collections.Generic;
using System.Linq;

namespace Kotoha
{
    public class KotohaSolrDocumentBuilder : SolrDocumentBuilder
    {
        private readonly KeywordSearchConfiguration _config = Factory.CreateObject("kotoha/KeywordSearchConfiguration", true) as KeywordSearchConfiguration;

        public KotohaSolrDocumentBuilder(IIndexable indexable, IProviderUpdateContext context) : base(indexable, context)
        {
        }

        protected override IEnumerable<IIndexableDataField> GetFieldsByItemList(IIndexable indexable)
        {
            var fields = base.GetFieldsByItemList(indexable);

            if (!(indexable is SitecoreIndexableItem indexableItem))
            {
                return fields;
            }

            var contentField = CreateKeywordSearchField(indexableItem.Item, _config.ContentField);
            var boostedFields = _config.BoostedFields
                .Select(concatedField => CreateKeywordSearchField(indexableItem.Item, concatedField))
                .ToList();

            return fields.Concat(boostedFields).Append(contentField);
        }

        private IIndexableDataField CreateKeywordSearchField(Item item, ConcatenatedField concatedField)
        {
            var values = concatedField.Fields.Select(field => item[field]).Where(value => !string.IsNullOrEmpty(value));
            var concatedValue = string.Join(" ", values);

            return new KeywordSearchField(concatedField.Name, concatedValue);
        }
    }
}
