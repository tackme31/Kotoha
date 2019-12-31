using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Diagnostics;
using System;

namespace Kotoha.Models
{
    public class KotohaDataField : IIndexableDataField
    {
        public string Name { get; }

        public string TypeKey { get; }

        public Type FieldType { get; }

        public object Value { get; }

        public object Id { get; }

        public KotohaDataField(string name, string value)
        {
            Assert.IsNotNullOrEmpty(name, $"'{nameof(name)}' should not be null.");
            Assert.IsNotNullOrEmpty(value, $"'{nameof(value)}' should not be null.");

            Name = name;
            TypeKey = "single-line text";
            FieldType = null;
            Value = value;
            Id = ID.NewID;
        }
    }
}
