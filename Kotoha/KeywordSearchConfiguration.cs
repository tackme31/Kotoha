using Sitecore.Diagnostics;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kotoha
{
    public class KeywordSearchConfiguration
    {
        public string ContentFieldName { get; set; }

        public ICollection<TargetField> TargetFields { get; }

        public KeywordSearchConfiguration(string contentFieldName)
        {
            ContentFieldName = contentFieldName;
            TargetFields = new List<TargetField>();
        }

        public void AddTargetField(XmlNode node)
        {
            Assert.IsNotNull(node, $"'{nameof(node)}' should not be null.");

            var name = XmlUtil.GetAttribute("name", node);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("'name' attribute should not be null or white space.");
            }

            var rawBoost = XmlUtil.GetAttribute("boost", node);
            if (string.IsNullOrEmpty(rawBoost))
            {
                TargetFields.Add(new TargetField(name, 0.0f));
                return;
            }

            if (!float.TryParse(rawBoost, out var boost))
            {
                throw new InvalidOperationException("'boost' attribute should be a positive float number.");
            }

            TargetFields.Add(new TargetField(name, boost));
        }
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
