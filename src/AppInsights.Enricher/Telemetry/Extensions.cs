// ReSharper disable ConvertClosureToMethodGroup

using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace AppInsights.Enricher.Telemetry
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.DependencyInjection;
    using ResultType.Extensions;
    using ResultType.Results;

    public static class Extensions
    {
        public static IResult<IServiceCollection> AddTelemetry<TProcessor>(
            this IServiceCollection services,
            ApplicationInsightsServiceOptions options
            ) where
                TProcessor : ITelemetryProcessor
        {
            try
            {
                services.AddSingleton<ITelemetryInitializer, Initializer>();
                services.AddApplicationInsightsTelemetry(options);
                services.AddApplicationInsightsTelemetryProcessor<TProcessor>();

                return services.ToSuccess();
            }
            catch (Exception e)
            {
                return e.Message.ToFailure<IServiceCollection>();
            }
        }
    }
}