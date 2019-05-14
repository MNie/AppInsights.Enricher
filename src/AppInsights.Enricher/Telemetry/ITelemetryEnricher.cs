namespace AppInsights.Enricher.Telemetry
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.AspNetCore.Http;

    public interface ITelemetryEnricher
    {
        ITelemetry Enrich(ITelemetry tele);
        bool AttachRequest(HttpContext context);
        bool AttachResponse(HttpContext context);
    }
}