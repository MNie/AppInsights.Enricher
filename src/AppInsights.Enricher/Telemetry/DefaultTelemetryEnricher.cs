namespace AppInsights.Enricher.Telemetry
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.AspNetCore.Http;

    public class DefaultTelemetryEnricher : ITelemetryEnricher
    {
        public ITelemetry Enrich(ITelemetry tele) => tele;

        public bool AttachRequest(HttpContext context) => true;

        public bool AttachResponse(HttpContext context) => true;
    }
}