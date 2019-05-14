namespace AppInsights.Enricher.Request.Filters
{
    using Newtonsoft.Json.Serialization;

    internal class PIIValueProvider : IValueProvider
    {
        private readonly object _defaultValue;

        public PIIValueProvider(string defaultValue) => _defaultValue = defaultValue;

        public object GetValue(object target) => _defaultValue;

        public void SetValue(object target, object value)
        {
        }
    }
}