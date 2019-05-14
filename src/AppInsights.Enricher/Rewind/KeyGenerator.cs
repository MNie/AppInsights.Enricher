namespace AppInsights.Enricher.Rewind
{
    internal static class KeyGenerator
    {
        internal static string Request(string id) => $"Request_{id}";
        internal static string Response(string id) => $"Response_{id}";
    }
}