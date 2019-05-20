namespace AppInsights.Enricher.Tests
{
    using System.IO;
    using System.Text;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using ResultType.Extensions;
    using Rewind;
    using Shouldly;
    using Telemetry;
    using Xunit;

    public class TelemetryInitializerTestsWhenRequestIsAPost : Initializer
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public TelemetryInitializerTestsWhenRequestIsAPost():
            base(CreateAccessor(), CreateBodyAccessor(), new TelemetryEnricherStub())
        {
        }

        private static IHttpContextAccessor CreateAccessor()
        {
            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()
            {
                Request =
                {
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("{hehehe: \"jsonik\"")),
                    Method = HttpMethods.Post,
                    ContentType = "application/json",
                    ContentLength = 10
                },
            });
            _httpContextAccessor = mock.Object;
            return _httpContextAccessor;
        }

        private static IRequestDataAccessor CreateBodyAccessor()
        {
            var mock = new Mock<IRequestDataAccessor>();
            mock.Setup(x => x.GetBody(It.IsAny<string>())).Returns("{hehehe: \"jsonik\"".ToSuccess());
            return mock.Object;
        }

        [Fact]
        public void AddRequestBody_WhenRequestHasBodyAndResponseCodeIsAnError_AddBodyToTelemetryAndResetBodyPosition()
        {
            var requestTelemetry = new RequestTelemetry()
            {
                ResponseCode = "500"
            };
            AddRequestBody(requestTelemetry);
            
            requestTelemetry.Properties["RequestBody"].ShouldBe("{hehehe: \"jsonik\"");
            _httpContextAccessor.HttpContext.Request.Body.Position.ShouldBe(0);
            _httpContextAccessor.HttpContext.Request.Body.CanRead.ShouldBeTrue();
        }
        
        [Fact]
        public void AddRequestBody_WhenRequestHasBodyButResponseCodeIsNotAnError_AddBodyToTelemetryAndResetBodyPosition()
        {
            var requestTelemetry = new RequestTelemetry()
            {
                ResponseCode = "200"
            };
            AddRequestBody(requestTelemetry);
            
            _httpContextAccessor.HttpContext.Request.Body.Position.ShouldBe(0);
            _httpContextAccessor.HttpContext.Request.Body.CanRead.ShouldBeTrue();
        }
    }
    
    public class TelemetryInitializerTestsWhenRequestIsAPut : Initializer
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public TelemetryInitializerTestsWhenRequestIsAPut():
            base(CreateAccessor(), CreateBodyAccessor(), new TelemetryEnricherStub())
        {
        }

        private static IHttpContextAccessor CreateAccessor()
        {
            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()
            {
                Request =
                {
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("{hehehe: \"jsonik\"")),
                    Method = HttpMethods.Put,
                    ContentType = "application/json",
                    ContentLength = 10
                },
            });
            _httpContextAccessor = mock.Object;
            return _httpContextAccessor;
        }
        
        private static IRequestDataAccessor CreateBodyAccessor()
        {
            var mock = new Mock<IRequestDataAccessor>();
            mock.Setup(x => x.GetBody(It.IsAny<string>())).Returns("{hehehe: \"jsonik\"".ToSuccess());
            return mock.Object;
        }

        [Fact]
        public void AddRequestBody_WhenRequestHasBody_AddBodyToTelemetryAndResetBodyPosition()
        {
            var requestTelemetry = new RequestTelemetry
            {
                ResponseCode = "500"
            };
            AddRequestBody(requestTelemetry);
            
            requestTelemetry.Properties["RequestBody"].ShouldBe("{hehehe: \"jsonik\"");
            _httpContextAccessor.HttpContext.Request.Body.Position.ShouldBe(0);
            _httpContextAccessor.HttpContext.Request.Body.CanRead.ShouldBeTrue();
        }
    }
    
    public class TelemetryInitializerTestsWhenRequestIsAPatch : Initializer
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public TelemetryInitializerTestsWhenRequestIsAPatch():
            base(CreateAccessor(), CreateBodyAccessor(), new TelemetryEnricherStub())
        {
        }

        private static IHttpContextAccessor CreateAccessor()
        {
            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()
            {
                Request =
                {
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("{hehehe: \"jsonik\"")),
                    Method = HttpMethods.Patch,
                    ContentType = "application/json", ContentLength = 10
                },
            });
            _httpContextAccessor = mock.Object;
            return _httpContextAccessor;
        }
        
        private static IRequestDataAccessor CreateBodyAccessor()
        {
            var mock = new Mock<IRequestDataAccessor>();
            mock.Setup(x => x.GetBody(It.IsAny<string>())).Returns("{hehehe: \"jsonik\"".ToSuccess());
            return mock.Object;
        }

        [Fact]
        public void AddRequestBody_WhenRequestHasBody_AddBodyToTelemetryAndResetBodyPosition()
        {
            var requestTelemetry = new RequestTelemetry
            {
                ResponseCode = "500"
            };
            AddRequestBody(requestTelemetry);
            
            requestTelemetry.Properties["RequestBody"].ShouldBe("{hehehe: \"jsonik\"");
            _httpContextAccessor.HttpContext.Request.Body.Position.ShouldBe(0);
            _httpContextAccessor.HttpContext.Request.Body.CanRead.ShouldBeTrue();
        }
    }

    public class TelemetryInitializerTestsWhenRequestIsAGet : Initializer
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public TelemetryInitializerTestsWhenRequestIsAGet():
            base(CreateAccessor(), CreateBodyAccessor(), new TelemetryEnricherStub())
        {
        }

        private static IHttpContextAccessor CreateAccessor()
        {
            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()
            {
                Request =
                {
                    Method = HttpMethods.Get,
                    ContentType = "application/json"
                },
            });
            _httpContextAccessor = mock.Object;
            return _httpContextAccessor;
        }
        
        private static IRequestDataAccessor CreateBodyAccessor()
        {
            var mock = new Mock<IRequestDataAccessor>();
            mock.Setup(x => x.GetBody(It.IsAny<string>())).Returns("{hehehe: \"jsonik\"".ToSuccess());
            return mock.Object;
        }

        [Fact]
        public void AddRequestBody_WhenRequestHasNotBody_DonotAddIt()
        {
            var requestTelemetry = new RequestTelemetry();
            AddRequestBody(requestTelemetry);
            
            requestTelemetry.Properties.ContainsKey("RequestBody").ShouldBeFalse();
        }
    }

    public class TelemetryInitializerTestsWhenTelemetryAlreadyContainsBody : Initializer
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public TelemetryInitializerTestsWhenTelemetryAlreadyContainsBody():
            base(CreateAccessor(), CreateBodyAccessor(), new TelemetryEnricherStub())
        {
        }

        private static IHttpContextAccessor CreateAccessor()
        {
            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()
            {
                Request =
                {
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("{hehehe: \"jsonik\"")),
                    Method = HttpMethods.Post,
                    ContentType = "application/json"
                },
            });
            _httpContextAccessor = mock.Object;
            return _httpContextAccessor;
        }
        
        private static IRequestDataAccessor CreateBodyAccessor()
        {
            var mock = new Mock<IRequestDataAccessor>();
            mock.Setup(x => x.GetBody(It.IsAny<string>())).Returns("{hehehe: \"jsonik\"".ToSuccess());
            return mock.Object;
        }

        [Fact]
        public void AddRequestBody_WhenRequestHasBodyButTelemetryAlreadyHasIt_DonotAddAnything()
        {
            var requestTelemetry = new RequestTelemetry()
            {
                Properties =
                {
                    ["RequestBody"] = "piesek leszek"
                }
            };
            AddRequestBody(requestTelemetry);
            
            requestTelemetry.Properties["RequestBody"].ShouldBe("piesek leszek");
            _httpContextAccessor.HttpContext.Request.Body.Position.ShouldBe(0);
            _httpContextAccessor.HttpContext.Request.Body.CanRead.ShouldBeTrue();
        }
    }

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
}