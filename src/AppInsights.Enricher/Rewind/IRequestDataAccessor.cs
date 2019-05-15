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
        Result<string> GetHttpBody(string traceInitializer);
        Result<Unit> SetHttpBody(HttpContext context, ActionDescriptor descriptor, IDictionary<string, object> args);

        Result<Unit> SetHttpBody(HttpContext context, IActionResult result);
    }
}