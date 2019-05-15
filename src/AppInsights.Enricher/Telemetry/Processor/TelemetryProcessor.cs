namespace AppInsights.Enricher.Telemetry.Processor
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    public class TelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;
        private readonly IProcessorApplier _applier;

        public TelemetryProcessor(ITelemetryProcessor next, IProcessorApplier applier)
        {
            _next = next;
            _applier = applier;
        }

        public void Process(ITelemetry item)
        {
            if (_applier.Apply(item))
                _next.Process(item);
        }
    }
}
