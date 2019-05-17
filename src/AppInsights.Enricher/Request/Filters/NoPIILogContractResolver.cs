namespace AppInsights.Enricher.Request.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class NoPIILogContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = new List<JsonProperty>();

            var thereAreSomePropertiesToHide = type.GetCustomAttributes(true).All(t => t.GetType() != typeof(SensitiveAttribute));
            if (thereAreSomePropertiesToHide)
            {
                var props = base.CreateProperties(type, memberSerialization);
                var excludedProperties = type
                    .GetProperties()
                    .Where(p => p.GetCustomAttributes(true).Any(t => t.GetType() == typeof(SensitiveAttribute)))
                    .Select(s => s.Name)
                    .ToList();

                var excludedFields = type
                    .GetFields()
                    .Where(p => p.GetCustomAttributes(true).Any(t => t.GetType() == typeof(SensitiveAttribute)))
                    .Select(s => s.Name)
                    .ToList();

                var toExclude = excludedProperties.Concat(excludedFields).ToList();
                
                return props
                    .Select(property => 
                        toExclude.Contains(property.PropertyName)
                            ? HideValue(property)
                            : property
                    )
                    .ToList();
            }

            return properties;
        }

        private static JsonProperty HideValue(JsonProperty property)
        {
            property.PropertyType = typeof(string);
            property.ValueProvider = new PIIValueProvider("PII Data");
            return property;
        }
    }
}