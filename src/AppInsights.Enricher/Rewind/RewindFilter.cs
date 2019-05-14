namespace AppInsights.Enricher.Rewind
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using ResultType;
    using ResultType.Extensions;
    using ResultType.Operations;

    public class RewindFilter : ActionFilterAttribute
    {
        private readonly IHttpBodyAccessor _bodyAccessor;
        private readonly ILogger<RewindFilter> _logger;

        public RewindFilter(IHttpBodyAccessor bodyAccessor, ILogger<RewindFilter> logger)
        {
            _bodyAccessor = bodyAccessor;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
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
            _bodyAccessor.SetHttpBody(context.HttpContext, context.ActionDescriptor, context.Result)
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