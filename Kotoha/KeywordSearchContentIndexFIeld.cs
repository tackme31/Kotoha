using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Xml;
using System.Linq;
using System.Xml;

namespace Kotoha
{
    public class KeywordSearchContentIndexField : IComputedIndexField
    {
        public string FieldName { get; set; }
        public string ReturnType { get; set; }
        public string SearchTargetId { get; }

        public KeywordSearchContentIndexField(XmlNode configNode)
        {
            FieldName = XmlUtil.GetAttribute("fieldName", configNode);
            ReturnType = XmlUtil.GetAttribute("returnType", configNode);
            SearchTargetId = XmlUtil.GetAttribute("searchTargetId", configNode);
        }

        public virtual object ComputeFieldValue(IIndexable indexable)
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

            var config = Factory.CreateObject("kotoha/configuration", false) as KeywordSearchConfiguration;
            if (config == null)
            {
                return null;
            }

            var searchTarget = config.GetSearchTargetById(SearchTargetId);
            if (searchTarget == null)
            {
                return null;
            }

            var fieldValues = searchTarget.Fields
                .Select(field => indexable.GetFieldByName(field.Name))
                .Where(field => field != null)
                .Select(field => index.Configuration.FieldReaders.GetFieldValue(field)?.ToString())
                .Where(value => !string.IsNullOrWhiteSpace(value));

            return string.Join(" ", fieldValues);
        }
    }
}
