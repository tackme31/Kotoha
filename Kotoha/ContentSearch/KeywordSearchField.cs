using Sitecore.ContentSearch;
using Sitecore.Diagnostics;
using System;

namespace Kotoha.ContentSearch
{
    public class KeywordSearchField : IIndexableDataField
    {
        public string Name { get; }

        public string TypeKey { get; }

        public Type FieldType { get; }

        public object Value { get; }

        public object Id { get; }

        public KeywordSearchField(string name, string value)
        {
            Assert.IsNotNullOrEmpty(name, $"'{nameof(name)}' should not be null or empty.");

            Name = name;
            FieldType = null;
            TypeKey = "single-line text";
            Value = value ?? string.Empty;
            Id = name;
        }
    }
}
