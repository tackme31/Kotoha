using Sitecore.Diagnostics;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kotoha.Models
{
    public class KotohaConfiguration
    {
        private readonly string _fieldNamePrefix;

        public string ContentFieldName { get; }

        public ICollection<FieldGroup> TargetFieldGroups { get; }

        public KotohaConfiguration(string fieldNamePrefix)
        {
            Assert.IsNotNullOrEmpty(fieldNamePrefix, $"'{nameof(fieldNamePrefix)}' should not be null or empty.");

            _fieldNamePrefix = fieldNamePrefix;
            ContentFieldName = $"{fieldNamePrefix}_content";
            TargetFieldGroups = new List<FieldGroup>();
        }

        public void AddTargetField(XmlNode node)
        {
            Assert.IsNotNull(node, $"'{nameof(node)}' should not be null.");

            var rawName = XmlUtil.GetAttribute("name", node, ignoreCase: true);
            if (string.IsNullOrWhiteSpace(rawName))
            {
                throw new ArgumentException("'name' attribute should not be null or white space.");
            }

            var rawBoost = XmlUtil.GetAttribute("boost", node, ignoreCase: true);
            if (!int.TryParse(rawBoost, out var boost) || boost < 0)
            {
                throw new ArgumentException("'boost' attribute should be a positive integer.");
            }

            var group = TargetFieldGroups.FirstOrDefault(g => g.Boost == boost);
            if (group == null)
            {
                group = new FieldGroup($"{_fieldNamePrefix}_{boost}", boost);
                TargetFieldGroups.Add(group);
            }

            group.Fields.Add(rawName);
        }
    }

    public class FieldGroup
    {
        public string Name { get; set; }

        public ICollection<string> Fields { get; set; }

        public int Boost { get; set; }

        public FieldGroup(string fieldName, int boost)
        {
            Name = fieldName;
            Boost = boost;
            Fields = new HashSet<string>();
        }
    }
}
