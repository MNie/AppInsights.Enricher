namespace AppInsights.Enricher.Telemetry.Processor
{
    using Microsoft.ApplicationInsights.Channel;

    public interface IProcessorApplier
    {
        bool Apply(ITelemetry tele);
    }
}