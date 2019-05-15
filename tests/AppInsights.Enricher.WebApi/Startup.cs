using Microsoft.AspNetCore.Http;

namespace AppInsights.Enricher.WebApi
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.Extensions.Configuration;
    using Rewind;
    using Swashbuckle.AspNetCore.Swagger;
    using Telemetry;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using ResultType.Extensions;
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

        public Startup(IConfiguration conf)
        {
            _conf = conf;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITelemetryEnricher, DefaultTelemetryEnricher>();
            services.AddSingleton<IProcessorApplier, DefaultProcessorApplier>();
            services.AddSingleton<IRequestDataAccessor>(new RequestDataAccessor(1_000, 1_000, 100_000, x => true));
            services.AddTelemetry<TelemetryProcessor>(_conf)
                .Bind(s =>
                {
                    s.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new Info {Title = "Enrich App Insights WebApi Tests API", Version = "v1"});
                    });
                    s.AddMvc(x => x.Filters.Add<RewindFilter>()); 
                    return ResultFactory.CreateSuccess();
                },err => throw new Exception(err.Message));
            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Swagger UI - Enrich";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enrich API V1");
            });
            
            app.ConfigureTelemetry(_conf["ApplicationInsights:InstrumentationKey"]);
            
            app.UseMvc();
        }
    }
}