namespace AppInsights.Enricher.Rewind
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using ResultType;
    using ResultType.Results;

    public interface IRequestDataAccessor 
    {
        Result<string> GetBody(string traceInitializer);
        Result<Unit> SetBody(HttpContext context, ActionDescriptor descriptor, IDictionary<string, object> args);

        Result<Unit> SetBody(HttpContext context, IActionResult result);
    }
}