namespace AppInsights.Enricher.Telemetry.Processor
{
    using Microsoft.ApplicationInsights.Channel;

    public class DefaultProcessorApplier : IProcessorApplier
    {
        public bool Apply(ITelemetry tele) => true;
    }
}