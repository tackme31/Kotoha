using Sitecore.Diagnostics;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kotoha
{
    public class KeywordSearchConfiguration
    {
        private readonly IDictionary<string, KeywordSearchTarget> _searchTarget;
        public ICollection<KeywordSearchTarget> SearchTargets => _searchTarget.Values;
        public KeywordSearchOptions DefaultKeywordSearchOptions { get; set; }

        public KeywordSearchConfiguration()
        {
            _searchTarget = new Dictionary<string, KeywordSearchTarget>();
        }

        public void AddSearchTarget(KeywordSearchTarget searchTarget)
        {
            Assert.ArgumentNotNull(searchTarget, nameof(searchTarget));

            _searchTarget[searchTarget.Id] = searchTarget;
        }

        public KeywordSearchTarget GetSearchTargetById(string id)
        {
            Assert.ArgumentNotNullOrEmpty(id, nameof(id));

            return _searchTarget.TryGetValue(id, out var value) ? value : null;
        }
    }

    public class KeywordSearchTarget
    {
        public string Id { get; }
        public ICollection<TargetField> Fields { get; }

        public KeywordSearchTarget(string id)
        {
            Assert.ArgumentNotNullOrEmpty(id, nameof(id));

            Id = id;
            Fields = new List<TargetField>();
        }

        public void AddField(XmlNode node)
        {
            Assert.ArgumentNotNull(node, nameof(node));

            var name = XmlUtil.GetAttribute("name", node);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("'name' attribute should not be null or white space.");
            }

            var rawBoost = XmlUtil.GetAttribute("boost", node);
            if (string.IsNullOrEmpty(rawBoost))
            {
                Fields.Add(new TargetField(name, 0.0f));
                return;
            }

            if (!float.TryParse(rawBoost, out var boost))
            {
                throw new InvalidOperationException("'boost' attribute should be a positive float number.");
            }

            Fields.Add(new TargetField(name, boost));
        }

        public class TargetField
        {
            public string Name { get; }
            public float Boost { get; }

            public TargetField(string fieldName, float boost)
            {
                Name = fieldName;
                Boost = boost;
            }
        }
    }
}
