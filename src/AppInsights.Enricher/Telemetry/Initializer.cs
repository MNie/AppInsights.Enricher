namespace AppInsights.Enricher.Telemetry
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using ResultType.Extensions;
    using ResultType.Operations;
    using ResultType.Results;
    using Rewind;

    public class Initializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestDataAccessor _requestDataAccessor;
        private readonly ITelemetryEnricher _telemetryEnricher;
        public Initializer(IHttpContextAccessor httpContextAccessor, IRequestDataAccessor requestDataAccessor, ITelemetryEnricher telemetryEnricher)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestDataAccessor = requestDataAccessor;
            _telemetryEnricher = telemetryEnricher;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry = _telemetryEnricher.Enrich(telemetry);

            if(_telemetryEnricher.AttachRequest(_httpContextAccessor.HttpContext))
                AddRequestBody(telemetry);
            
            if(_telemetryEnricher.AttachResponse(_httpContextAccessor.HttpContext))
                AddResponseBody(telemetry);
        }

        protected void AddRequestBody(ITelemetry telemetry)
        {
            switch (telemetry)
            {
                case RequestTelemetry requestTelemetry when HasBody() && int.TryParse(requestTelemetry.ResponseCode, out var code):
                {
                    if(!requestTelemetry.Properties.ContainsKey("RequestBody") && IsAnError(code))
                        GetBody(KeyGenerator.Request)
                            .Bind(data =>
                            {
                                requestTelemetry.Properties.Add("RequestBody", data);
                                return data.ToSuccess();
                            });
                    break;
                }
            }
        }
        
        protected void AddResponseBody(ITelemetry telemetry)
        {
            switch (telemetry)
            {
                case RequestTelemetry requestTelemetry:
                {
                    if(!requestTelemetry.Properties.ContainsKey("ResponseBody"))
                        GetBody(KeyGenerator.Response)
                            .Bind(data =>
                            {
                                requestTelemetry.Properties.Add("ResponseBody", data);
                                return data.ToSuccess();
                            });
                    break;
                }
            }
        }

        private static bool IsAnError(int code) =>
            code >= 400
            && code < 600;

        private Result<string> GetBody(Func<string, string> decorateItentifier)
        {
            var context = _httpContextAccessor.HttpContext;
            return _requestDataAccessor.GetHttpBody(decorateItentifier(context.TraceIdentifier));
        }

        private bool HasBody()
        {
            var verbWhichSupportsBody = (_httpContextAccessor.HttpContext.Request.Method == HttpMethods.Post
                     || _httpContextAccessor.HttpContext.Request.Method == HttpMethods.Put
                     || _httpContextAccessor.HttpContext.Request.Method == HttpMethods.Patch);
            return verbWhichSupportsBody
                   && _httpContextAccessor.HttpContext.Request.ContentLength > 0;
        }
    }
}
