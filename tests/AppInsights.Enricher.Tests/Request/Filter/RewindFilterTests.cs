namespace AppInsights.Enricher.Tests.Request.Filter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Internal;
    using Moq;
    using ResultType.Factories;
    using Rewind;
    using Xunit;

    public class EnableRewindMiddlewareTests
    {
        [Fact]
        public async Task Invoke_WhenSetBodyReturnSuccess_DoNotLogAnything()
        {
            var httpAccessor = new Mock<IHttpBodyAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<ActionDescriptor>(),
                    It.IsAny<IDictionary<string, object>>()))
                .Returns(ResultFactory.CreateSuccess());
            var logger = new Mock<ILogger<RewindFilter>>();
            var filter = new RewindFilter(httpAccessor.Object, logger.Object);

            await filter.OnActionExecutionAsync(new ActionExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new Dictionary<string, object>(), new object()), () => Task.FromResult(new ActionExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new object())));
            
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Never);
        }
        
        [Fact]
        public async Task Invoke_WhenSetBodyReturnFailure_LogError()
        {
            var httpAccessor = new Mock<IHttpBodyAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<ActionDescriptor>(),
                    It.IsAny<IDictionary<string, object>>()))
                .Returns(ResultFactory.CreateFailure("dd"));
            var logger = new Mock<ILogger<RewindFilter>>();
            var filter = new RewindFilter(httpAccessor.Object, logger.Object);

            await filter.OnActionExecutionAsync(new ActionExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new Dictionary<string, object>(), new object()), () => Task.FromResult(new ActionExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new object())));
            
            logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}