using Kotoha.Models;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.Data.Items;
using System.Collections.Generic;
using System.Linq;

namespace Kotoha.DocumentBuilders
{
    public class KotohaSolrDocumentBuilder : SolrDocumentBuilder
    {
        private readonly KotohaConfiguration _configuration = Factory.CreateObject("kotoha/KotohaConfiguration", true) as KotohaConfiguration;

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

            var contentField = CreateContentField(indexableItem.Item);
            var boostedFields = _configuration.TargetFieldGroups
                .Select(fieldGroup => CreateBoostedField(indexableItem.Item, fieldGroup))
                .ToList();

            return fields.Concat(boostedFields).Append(contentField);
        }

        private IIndexableDataField CreateBoostedField(Item item, FieldGroup fieldGroup)
        {
            var value = string.Join(" ", fieldGroup.Fields.Select(field => item[field]));

            return new KotohaDataField(fieldGroup.Name, value);
        }

        private IIndexableDataField CreateContentField(Item item)
        {
            var fields = _configuration.TargetFieldGroups.SelectMany(fieldGroup => fieldGroup.Fields);
            var value = string.Join(" ", fields.Select(field => item[field]));

            return new KotohaDataField(_configuration.ContentFieldName, value);
        }
    }
}
