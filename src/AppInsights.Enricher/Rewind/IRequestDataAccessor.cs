using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace AppInsights.Enricher.Rewind
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;
    using ResultType;
    using ResultType.Results;

    public interface IRequestDataAccessor
    {
        IResult<string> GetBody(string traceInitializer);
        IResult<Unit> SetBody(HttpContext context, ActionDescriptor descriptor, IDictionary<string, object> args);

        IResult<Unit> SetBody(HttpContext context, IActionResult result);
    }
}