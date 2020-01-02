using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using System.Linq;

namespace Kotoha
{
    public class KeywordSearchContentIndexField : AbstractComputedIndexField
    {
        public override object ComputeFieldValue(IIndexable indexable)
        {
            if (!(indexable is SitecoreIndexableItem indexableItem))
            {
                return null;
            }

            var index = ContentSearchManager.GetIndex(indexable);
            if (index == null)
            {
                return null;
            }

            var config = Factory.CreateObject("kotoha/KeywordSearchConfiguration", false) as KeywordSearchConfiguration;
            if (config == null)
            {
                return null;
            }

            var fieldValues = config.TargetFields
                .Select(field => indexable.GetFieldByName(field.Name))
                .Where(field => field != null)
                .Select(field => index.Configuration.FieldReaders.GetFieldValue(field));

            return string.Join(" ", fieldValues);
        }
    }
}
