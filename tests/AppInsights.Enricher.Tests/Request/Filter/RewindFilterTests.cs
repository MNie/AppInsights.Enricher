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
    using Telemetry;
    using Xunit;

    public class EnableRewindMiddlewareTests
    {
        [Fact]
        public async Task OnActionExecutionAsync_WhenSetBodyReturnSuccess_DoNotLogAnything()
        {
            var httpAccessor = new Mock<IRequestDataAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<ActionDescriptor>(),
                    It.IsAny<IDictionary<string, object>>()))
                .Returns(ResultFactory.CreateSuccess());
            var logger = new Mock<ILogger<RewindFilter>>();
            var telemetryEnricher = new Mock<ITelemetryEnricher>();
            telemetryEnricher.Setup(x => x.AttachRequest(It.IsAny<HttpContext>())).Returns(true);
            var filter = new RewindFilter(httpAccessor.Object, logger.Object, telemetryEnricher.Object);

            await filter.OnActionExecutionAsync(new ActionExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new Dictionary<string, object>(), new object()), () => Task.FromResult(new ActionExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new object())));
            
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Never);
        }
        
        [Fact]
        public async Task OnActionExecutionAsync_WhenSetBodyReturnFailure_LogError()
        {
            var httpAccessor = new Mock<IRequestDataAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<ActionDescriptor>(),
                    It.IsAny<IDictionary<string, object>>()))
                .Returns(ResultFactory.CreateFailure("dd"));
            var logger = new Mock<ILogger<RewindFilter>>();
            var telemetryEnricher = new Mock<ITelemetryEnricher>();
            telemetryEnricher.Setup(x => x.AttachRequest(It.IsAny<HttpContext>())).Returns(true);
            var filter = new RewindFilter(httpAccessor.Object, logger.Object, telemetryEnricher.Object);

            await filter.OnActionExecutionAsync(new ActionExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new Dictionary<string, object>(), new object()), () => Task.FromResult(new ActionExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new object())));
            
            logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public async Task OnActionExecutionAsync_WhenBodyShouldNotBeIncluded_NotCallSetBody()
        {
            var httpAccessor = new Mock<IRequestDataAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<ActionDescriptor>(),
                    It.IsAny<IDictionary<string, object>>()))
                .Returns(ResultFactory.CreateFailure("dd"));
            var logger = new Mock<ILogger<RewindFilter>>();
            var telemetryEnricher = new Mock<ITelemetryEnricher>();
            telemetryEnricher.Setup(x => x.AttachRequest(It.IsAny<HttpContext>())).Returns(false);
            var filter = new RewindFilter(httpAccessor.Object, logger.Object, telemetryEnricher.Object);

            await filter.OnActionExecutionAsync(new ActionExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new Dictionary<string, object>(), new object()), () => Task.FromResult(new ActionExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new object())));
            
            httpAccessor.Verify(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<ActionDescriptor>(),
                It.IsAny<IDictionary<string, object>>()), Times.Never);
        }
        
        [Fact]
        public async Task OnResultExecutionAsync_WhenSetBodyReturnSuccess_DoNotLogAnything()
        {
            var httpAccessor = new Mock<IRequestDataAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<IActionResult>()))
                .Returns(ResultFactory.CreateSuccess());
            var logger = new Mock<ILogger<RewindFilter>>();
            var telemetryEnricher = new Mock<ITelemetryEnricher>();
            telemetryEnricher.Setup(x => x.AttachResponse(It.IsAny<HttpContext>())).Returns(true);
            var filter = new RewindFilter(httpAccessor.Object, logger.Object, telemetryEnricher.Object);

            await filter.OnResultExecutionAsync(new ResultExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new OkObjectResult("dd"), new object()), () => Task.FromResult(new ResultExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new OkObjectResult("dd"), new object())));
            
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Never);
        }
        
        [Fact]
        public async Task OnResultExecutionAsync_WhenSetBodyReturnFailure_LogError()
        {
            var httpAccessor = new Mock<IRequestDataAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<IActionResult>()))
                .Returns(ResultFactory.CreateFailure("dd"));
            var logger = new Mock<ILogger<RewindFilter>>();
            var telemetryEnricher = new Mock<ITelemetryEnricher>();
            telemetryEnricher.Setup(x => x.AttachResponse(It.IsAny<HttpContext>())).Returns(true);
            var filter = new RewindFilter(httpAccessor.Object, logger.Object, telemetryEnricher.Object);

            await filter.OnResultExecutionAsync(new ResultExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new OkObjectResult("dd"), new object()), () => Task.FromResult(new ResultExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new OkObjectResult("dd"), new object())));
            
            logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public async Task OnResultExecutionAsync_WhenBodyShouldNotBeIncluded_NotCallSetBody()
        {
            var httpAccessor = new Mock<IRequestDataAccessor>();
            httpAccessor.Setup(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<IActionResult>()))
                .Returns(ResultFactory.CreateFailure("dd"));
            var logger = new Mock<ILogger<RewindFilter>>();
            var telemetryEnricher = new Mock<ITelemetryEnricher>();
            telemetryEnricher.Setup(x => x.AttachResponse(It.IsAny<HttpContext>())).Returns(false);
            var filter = new RewindFilter(httpAccessor.Object, logger.Object, telemetryEnricher.Object);

            await filter.OnResultExecutionAsync(new ResultExecutingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new OkObjectResult("dd"), new object()), () => Task.FromResult(new ResultExecutedContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>(), new OkObjectResult("dd"), new object())));
            
            httpAccessor.Verify(x => x.SetHttpBody(It.IsAny<HttpContext>(), It.IsAny<ActionDescriptor>(),
                It.IsAny<IDictionary<string, object>>()), Times.Never);
        }
    }
}