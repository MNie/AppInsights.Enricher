namespace AppInsights.Enricher.Rewind
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using ResultType;
    using ResultType.Extensions;
    using ResultType.Operations;
    using Telemetry;

    public class RewindFilter : ActionFilterAttribute
    {
        private readonly IRequestDataAccessor _bodyAccessor;
        private readonly ITelemetryEnricher _telemetryEnricher;
        private readonly ILogger<RewindFilter> _logger;

        public RewindFilter(IRequestDataAccessor bodyAccessor, ILogger<RewindFilter> logger, ITelemetryEnricher telemetryEnricher)
        {
            _bodyAccessor = bodyAccessor;
            _logger = logger;
            _telemetryEnricher = telemetryEnricher;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_telemetryEnricher.AttachRequest(context.HttpContext))
                _bodyAccessor.SetHttpBody(context.HttpContext, context.ActionDescriptor, context.ActionArguments)
                    .Bind(
                        x => x.ToSuccess(), 
                        err =>
                        {
                            _logger.LogWarning(err.Message, err);
                            return err.Message.ToFailure<Unit>();
                        });
            
            await next();
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (_telemetryEnricher.AttachResponse(context.HttpContext))
                _bodyAccessor.SetHttpBody(context.HttpContext, context.Result)
                    .Bind(
                        x => x.ToSuccess(), 
                        err =>
                        {
                            _logger.LogWarning(err.Message, err);
                            return err.Message.ToFailure<Unit>();
                        });
            await next();
        }
    }
}