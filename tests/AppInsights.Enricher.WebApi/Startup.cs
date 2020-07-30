using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace AppInsights.Enricher.WebApi
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.Extensions.Configuration;
    using Rewind;
    using Telemetry;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Builder;
    using ResultType.Factories;
    using ResultType.Operations;
    using Telemetry.Processor;

    public class TelemetryEnricherStub : ITelemetryEnricher
    {
        public ITelemetry Enrich(ITelemetry tele)
        {
            tele.Context.GlobalProperties["test"] = "test";
            return tele;
        }

        public bool AttachRequest(HttpContext context)
        {
            return true;
        }

        public bool AttachResponse(HttpContext context)
        {
            return true;
        }
    }

    public class Startup
    {
        private readonly IConfiguration _conf;

        public Startup(IConfiguration conf) => _conf = conf;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITelemetryEnricher, DefaultTelemetryEnricher>();
            services.AddSingleton<IProcessorApplier, DefaultProcessorApplier>();
            services.AddSingleton<IRequestDataAccessor>(new RequestDataAccessor(1_000, 1_000, 100_000));
            services.AddTelemetry<TelemetryProcessor>(new ApplicationInsightsServiceOptions
                {
                    InstrumentationKey = _conf["ApplicationInsights:InstrumentationKey"]
                })
                .Bind(s =>
                {
                    s.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Enrich App Insights WebApi Tests API", Version = "v1" });
                    });
                    s.AddMvc(x => x.Filters.Add<RewindFilter>());
                    return ResultFactory.CreateSuccess();
                },err => throw new Exception(err.Message));

            services
                .AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Swagger UI - Enrich";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enrich API V1");
            });
            app.UseRouting();
            app.UseEndpoints(x => x.MapControllers());
        }
    }
}