// ReSharper disable ConvertClosureToMethodGroup
namespace AppInsights.Enricher.Telemetry
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ResultType.Extensions;
    using ResultType.Results;
    using Rewind;

    public static class Extensions
    {
        public static Result<IServiceCollection> AddTelemetry<TProcessor>(
            this IServiceCollection services, 
            IConfiguration configuration 
            ) where 
                TProcessor : ITelemetryProcessor
        {
            try
            {
                services.AddSingleton<ITelemetryInitializer, Initializer>();
                services.AddApplicationInsightsTelemetry(configuration);
                services.AddApplicationInsightsTelemetryProcessor<TProcessor>();

                return services.ToSuccess();
            }
            catch (Exception e)
            {
                return e.Message.ToFailure<IServiceCollection>();
            }
        }

        public static void ConfigureTelemetry(this IApplicationBuilder app, string instrumentationKey)
        {
            var isEnabled = !string.IsNullOrWhiteSpace(instrumentationKey);
            if (isEnabled)
            {
                var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
                loggerFactory.AddApplicationInsights(app.ApplicationServices, FilterLogLevel);
            }
            else
                TelemetryConfiguration.Active.DisableTelemetry = true;
        }

        private static bool FilterLogLevel(string s, LogLevel logLevel)
        {
            return logLevel != LogLevel.Trace && logLevel != LogLevel.None && logLevel != LogLevel.Information;
        }
    }
}