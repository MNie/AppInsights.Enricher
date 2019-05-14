namespace AppInsights.Enricher.Telemetry
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    public class Processor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        public Processor(ITelemetryProcessor next) => Next = next;

        public void Process(ITelemetry item) => Next.Process(item);
    }
}
