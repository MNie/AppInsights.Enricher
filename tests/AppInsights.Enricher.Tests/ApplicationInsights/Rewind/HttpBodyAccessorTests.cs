namespace AppInsights.Enricher.Tests.ApplicationInsights.Rewind
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;
    using Enricher.Rewind;
    using Microsoft.AspNetCore.Http;
    using Shouldly;
    using Xunit;

    public class HttpBodyAccessorTests
    {
        [Fact]
        public void SetHttpBody_WhenBodyIsEmpty_ReturnFailureAndDoNotPersistData()
        {
            var sut = new HttpBodyAccessor(10, 1000, 20_000, _ => true);

            var result =
                sut.SetHttpBody(
                    new DefaultHttpContext()
                    {
                        Request = {Body = new MemoryStream(new byte[0]), ContentLength = 0}, TraceIdentifier = "dd"
                    }, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
                    {
                        MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
                    }, new Dictionary<string, object>()
                    {
                        ["arg"] = null
                    });
            var entry = sut.GetHttpBody("dd");

            result.IsFailure.ShouldBeTrue();
            entry.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void SetHttpBody_WhenBodyUnderGivenKeyAlreadyExists_ReturnTheNewestOne()
        {
            var sut = new HttpBodyAccessor(10, 1000, 20_000, _ => true);

            sut.SetHttpBody(
                new DefaultHttpContext()
                {
                    Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd")), ContentLength = 10},
                    TraceIdentifier = "dd"
                }, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
                {
                    MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
                }, new Dictionary<string, object>()
                {
                    ["arg"] = "ddd"
                });
            var context = new DefaultHttpContext()
            {
                Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd2312")), ContentLength = 10},
                TraceIdentifier = "dd"
            };
            var result = sut.SetHttpBody(context, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            {
                MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
            }, new Dictionary<string, object>()
            {
                ["arg"] = "ddd2312"
            });
            var entry = sut.GetHttpBody("dd");

            result.IsSuccess.ShouldBeTrue();
            entry.IsSuccess.ShouldBeTrue();
            entry.Payload.ShouldBe("\"ddd2312\"");

            context.Request.Body.Position.ShouldBe(0);
            context.Request.Body.CanRead.ShouldBeTrue();
        }

        [Fact]
        public void SetHttpBody_WhenAnotherBodyExceedsCacheSizeLimit_ReturnFailure()
        {
            var sut = new HttpBodyAccessor(1, 1000, 20_000, _ => true);

            var context = new DefaultHttpContext()
            {
                Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd")), ContentLength = 10},
                TraceIdentifier = "dd"
            };
            sut.SetHttpBody(context, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            {
                MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
            }, new Dictionary<string, object>()
            {
                ["arg"] = "ddd"
            });
            var result =
                sut.SetHttpBody(
                    new DefaultHttpContext()
                    {
                        Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd2312")), ContentLength = 10},
                        TraceIdentifier = "dd2"
                    }, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
                    {
                        MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
                    }, new Dictionary<string, object>()
                    {
                        ["arg"] = "ddd2312"
                    });
            var entry = sut.GetHttpBody("dd");

            result.IsFailure.ShouldBeTrue();
            entry.IsSuccess.ShouldBeTrue();
            entry.Payload.ShouldBe("\"ddd\"");

            context.Request.Body.Position.ShouldBe(0);
            context.Request.Body.CanRead.ShouldBeTrue();
        }

        [Fact]
        public async Task
            SetHttpBody_WhenBodyIsNotEmptyAndPredicateMatches_ReturnSuccessAndPersistDataInCacheFor5SecondsAndRewindDataAndResetPosition()
        {
            var sut = new HttpBodyAccessor(1, 1000, 1_000, _ => true);

            var context = new DefaultHttpContext()
            {
                Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd")), ContentLength = 10},
                TraceIdentifier = "dd"
            };
            var result = sut.SetHttpBody(context, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            {
                MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
            }, new Dictionary<string, object>()
            {
                ["arg"] = "ddd"
            });
            var entry = sut.GetHttpBody("dd");

            result.IsSuccess.ShouldBeTrue();
            entry.IsSuccess.ShouldBeTrue();
            entry.Payload.ShouldBe("\"ddd\"");

            context.Request.Body.Position.ShouldBe(0);
            context.Request.Body.CanRead.ShouldBeTrue();

            await Task.Delay(2000);

            var possibleEntry = sut.GetHttpBody("dd");

            possibleEntry.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void SetHttpBody_WhenBodyIsNotEmptyAndPredicateNotMatches_ReturnFailureAndNotPersistDataInCache()
        {
            var sut = new HttpBodyAccessor(1, 1000, 1_000, x => x.Request.ContentLength > 1000);

            var result =
                sut.SetHttpBody(
                    new DefaultHttpContext()
                    {
                        Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd")), ContentLength = 10},
                        TraceIdentifier = "dd"
                    }, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
                    {
                        MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
                    }, new Dictionary<string, object>()
                    {
                        ["arg"] = "ddd"
                    });
            var entry = sut.GetHttpBody("dd");

            result.IsFailure.ShouldBeTrue();
            entry.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void SetHttpBody_WhenReadingBodyErrorOccurred_ReturnFailureAndLogInformation()
        {
            var sut = new HttpBodyAccessor(1, 1000, 1_000, x => true);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes("ddd"));
            stream.Dispose();
            var result =
                sut.SetHttpBody(
                    new DefaultHttpContext() {Request = {Body = stream, ContentLength = 10}, TraceIdentifier = "ddd"},
                    new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
                    {
                        MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
                    }, new Dictionary<string, object>()
                    {
                        ["arg"] = "ddd"
                    });
            var entry = sut.GetHttpBody("dd");

            result.IsSuccess.ShouldBeTrue();
            entry.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void GetHttpBody_WhenBodyExistInCache_ReturnDataAndSuccess()
        {
            var sut = new HttpBodyAccessor(1, 1000, 1_000, _ => true);

            var context = new DefaultHttpContext()
            {
                Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd")), ContentLength = 10},
                TraceIdentifier = "dd"
            };
            sut.SetHttpBody(context, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            {
                MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
            }, new Dictionary<string, object>()
            {
                ["arg"] = "ddd"
            });
            var result = sut.GetHttpBody("dd");

            result.IsSuccess.ShouldBeTrue();
            result.Payload.ShouldBe("\"ddd\"");
        }

        [Fact]
        public void GetHttpBody_WhenBodyNotExistInCache_ReturnFailure()
        {
            var sut = new HttpBodyAccessor(1, 1000, 1_000, _ => true);

            var context = new DefaultHttpContext()
            {
                Request = {Body = new MemoryStream(Encoding.UTF8.GetBytes("ddd")), ContentLength = 10},
                TraceIdentifier = "dd"
            };
            sut.SetHttpBody(context, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            {
                MethodInfo = new DynamicMethod("", typeof(string), new Type[0])
            }, new Dictionary<string, object>()
            {
                ["arg"] = "ddd"
            });
            var result = sut.GetHttpBody("dd321");

            result.IsFailure.ShouldBeTrue();
        }
    }
}