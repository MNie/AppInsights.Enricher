namespace AppInsights.Enricher.Rewind
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;
    using Request.Filters;
    using ResultType;
    using ResultType.Extensions;
    using ResultType.Factories;
    using ResultType.Operations;
    using ResultType.Results;

    public class HttpBodyAccessor : IHttpBodyAccessor
    {
        private readonly MemoryCache _cache;
        private readonly TimeSpan _expirationInMs;
        private readonly Predicate<HttpContext> _pred;

        public HttpBodyAccessor(long cacheSize, int scanFrequencyMs, int expirationInMs, Predicate<HttpContext> pred)
        {
            var cacheOptions = new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(scanFrequencyMs),
                SizeLimit = cacheSize
            };
            _cache = new MemoryCache(cacheOptions);
            _expirationInMs = TimeSpan.FromMilliseconds(expirationInMs);
            _pred = pred;
        }

        public Result<string> GetHttpBody(string traceInitializer)
        {
            try
            {
                var success = _cache.TryGetValue(traceInitializer, out var result);
                if (!success)
                    return $"record under key {traceInitializer} was not found in a cache".ToFailure<string>();

                switch (result)
                {
                    case string s:
                        return s.ToSuccess();
                    default:
                        return
                            $"record under key: {traceInitializer} was found but was of a different type: {result.GetType()} than string"
                                .ToFailure<string>();
                }
            }
            catch (Exception e)
            {
                return e.Message.ToFailure<string>();
            }
        }

        public Result<Unit> SetHttpBody(HttpContext context, ActionDescriptor descriptor,
            IDictionary<string, object> args) =>
            ReadRequest(descriptor, args)
                .Bind(body => SetEntryInCache(KeyGenerator.Request(context.TraceIdentifier), body));

        public Result<Unit> SetHttpBody(HttpContext context, ActionDescriptor descriptor, IActionResult result) =>
            ReadResponse(descriptor, result)
                .Bind(body => SetEntryInCache(KeyGenerator.Response(context.TraceIdentifier), body));

        private static Result<string> ReadResponse(ActionDescriptor descriptor, IActionResult result)
        {
            try
            {
                return JsonConvert.SerializeObject(
                    (result as ObjectResult).Value,
                    new JsonSerializerSettings() { ContractResolver = new NoPIILogContractResolver() }
                ).ToSuccess();
            }
            catch (Exception e)
            {
                return e.Message.ToFailure<string>();
            }
        }

        private static Result<string> ReadRequest(ActionDescriptor actionDescriptor,
            IDictionary<string, object> args)
        {
            try
            {
                var methodInfo = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)actionDescriptor).MethodInfo;
                var noLogParameters = methodInfo.GetParameters().Where(p => p.GetCustomAttributes(true).Any(t => t.GetType() == typeof(SensitiveAttribute))).Select(p => p.Name);

                var hiddenSensitiveData = args
                    .Where(a => !noLogParameters.Contains(a.Key) && a.Value != null)
                    .Select(argument => JsonConvert.SerializeObject(argument.Value,
                        new JsonSerializerSettings() {ContractResolver = new NoPIILogContractResolver()})
                    )
                    .ToList();

                return hiddenSensitiveData.Any()
                    ? string.Join(Environment.NewLine, hiddenSensitiveData).ToSuccess()
                    : "data is empty".ToFailure<string>();
            }
            catch (Exception e)
            {
                return e.Message.ToFailure<string>();
            }
        }

        private Result<Unit> SetEntryInCache(string traceInitializer, string body)
        {
            try
            {
                _cache.Set(traceInitializer, body, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = _expirationInMs,
                    Size = 1
                });
                var successSave = _cache.TryGetValue(traceInitializer, out var _);
                return successSave
                    ? ResultFactory.CreateSuccess()
                    : $"entry under key {traceInitializer} was not saved in a cache".ToFailure<Unit>();
            }
            catch (Exception e)
            {
                return e.Message.ToFailure<Unit>();
            }
        }
    }
}