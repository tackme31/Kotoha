using Sitecore.Diagnostics;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kotoha
{
    public class KeywordSearchConfiguration
    {
        private readonly string _fieldNamePrefix;

        public ConcatenatedField ContentField { get; }

        public ICollection<ConcatenatedField> BoostedFields { get; }

        public KeywordSearchConfiguration(string fieldNamePrefix)
        {
            Assert.IsNotNullOrEmpty(fieldNamePrefix, $"'{nameof(fieldNamePrefix)}' should not be null or empty.");

            _fieldNamePrefix = fieldNamePrefix;
            ContentField = new ConcatenatedField($"{fieldNamePrefix}_c", 0);
            BoostedFields = new List<ConcatenatedField>();
        }

        public void AddTargetField(XmlNode node)
        {
            Assert.IsNotNull(node, $"'{nameof(node)}' should not be null.");

            var name = XmlUtil.GetAttribute("name", node);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("'name' attribute should not be null or white space.");
            }

            ContentField.Fields.Add(name);

            if (!int.TryParse(XmlUtil.GetAttribute("boost", node), out var boost))
            {
                throw new ArgumentException("'boost' attribute should be an integer.");
            }

            if (boost <= 0)
            {
                return;
            }

            var fieldName = $"{_fieldNamePrefix}_{boost}";
            var field = BoostedFields.FirstOrDefault(f => f.Name == fieldName);
            if (field != null)
            {
                field.Fields.Add(name);
                return;
            }

            var newField = new ConcatenatedField(fieldName, boost);
            newField.Fields.Add(name);
            BoostedFields.Add(newField);
        }
    }

    public class ConcatenatedField
    {
        public string Name { get; set; }

        public float Boost { get; set; }

        public ICollection<string> Fields { get; set; }

        public ConcatenatedField(string fieldName, float boost)
        {
            Name = fieldName;
            Boost = boost;
            Fields = new HashSet<string>();
        }
    }
}
