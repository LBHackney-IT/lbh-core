using FluentAssertions;
using Hackney.Core.Authorization;
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Authorization
{
    public class GoogleGroupsAuthorizationMiddlewareTests
    {
        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestNullUrlsEnvironmentVariable_ThrowsEnvironmentVariableIsNullException()
        {
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", null);
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Throws(new ArgumentNullException("MyException"));

            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));

            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            Func<Task> act = () => sut.Invoke(httpContext, mockTokenFactory.Object);

            await act.Should().ThrowAsync<EnvironmentVariableIsNullException>().WithMessage("URLS_TO_SKIP_AUTH environment variable is null. Please, set up URLS_TO_SKIP_AUTH variable").ConfigureAwait(false);
        }

        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestNullRequesUrlPath_ThrowsArgumentNullException()
        {
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", "/development/swagger/v1.0/swagger.json");
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            //httpContext.Request.Path = null;

            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Throws(new ArgumentNullException("MyException"));

            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));

            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            Func<Task> act = () => sut.Invoke(httpContext, mockTokenFactory.Object);

            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage($"Value cannot be null.").ConfigureAwait(false);
        }

        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestNullHeaders_ThrowsArgumentNullException()
        {
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", "/development/swagger/v1.0/swagger.json");
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Path = "/development/swagger/v1.0/swagger.json  ";

            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Throws(new ArgumentNullException("MyException"));

            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));

            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            Func<Task> act =  () =>  sut.Invoke(httpContext, mockTokenFactory.Object);

            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'MyException')").ConfigureAwait(false);
            mockRequestDelegate.Verify(x => x.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }
        
        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestNullToken_HasUnauthorizedResponse()
        {
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", "/development/swagger/v1.0/swagger.json");
            Token expectedToken = null;
            var expectedResponseText = "JWT token cannot be parsed!";
            var expectedStatusCode = (int)HttpStatusCode.Unauthorized;

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Path = "/development/swagger/v1.0/swagger.json  ";

            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Returns(expectedToken);
            
            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));
            
            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            await sut.Invoke(httpContext, mockTokenFactory.Object).ConfigureAwait(false);

            httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
            httpContext.Response.Body.Position = 0;
            using (StreamReader streamReader = new StreamReader(httpContext.Response.Body))
            {
                string actualResponseText = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                var errorResponse = JsonConvert.DeserializeObject<BaseErrorResponse>(actualResponseText);

                errorResponse.StatusCode.Should().Be(expectedStatusCode);
                errorResponse.Message.Should().Be(expectedResponseText);
            }
            mockRequestDelegate.Verify(x => x.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestTokenGroupsAreNull_HasForbiddenResponse()
        {
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", "/development/swagger/v1.0/swagger.json");
            Token expectedToken = new Token 
            {
                Groups = null
            };
            var expectedResponseText = "JWT token should contain [groups] claim!";
            var expectedStatusCode = (int)HttpStatusCode.Forbidden;

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Path = "/development/swagger/v1.0/swagger.json  ";

            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Returns(expectedToken);

            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));

            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            await sut.Invoke(httpContext, mockTokenFactory.Object).ConfigureAwait(false);

            httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
            httpContext.Response.Body.Position = 0;
            using (StreamReader streamReader = new StreamReader(httpContext.Response.Body))
            {
                string actualResponseText = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                var errorResponse = JsonConvert.DeserializeObject<BaseErrorResponse>(actualResponseText);

                errorResponse.StatusCode.Should().Be(expectedStatusCode);
                errorResponse.Message.Should().Be(expectedResponseText);
            }
            mockRequestDelegate.Verify(x => x.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestRequiredGoogleGroupsVariableIsNull_HasInternalServerErrorResponse()
        {
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", "/development/swagger/v1.0/swagger.json");
            Environment.SetEnvironmentVariable("REQUIRED_GOOGLE_GROUPS", null);
            Token expectedToken = new Token
            {
                Groups = new string[] { "HackneyAll"}
            };
            var expectedResponseText = "Cannot resolve REQUIRED_GOOGLE_GROUPS environment variable!";
            var expectedStatusCode = (int)HttpStatusCode.InternalServerError;

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Path = "/development/swagger/v1.0/swagger.json  ";
            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Returns(expectedToken);

            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));

            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            await sut.Invoke(httpContext, mockTokenFactory.Object).ConfigureAwait(false);

            httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
            httpContext.Response.Body.Position = 0;
            using (StreamReader streamReader = new StreamReader(httpContext.Response.Body))
            {
                string actualResponseText = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                var errorResponse = JsonConvert.DeserializeObject<BaseErrorResponse>(actualResponseText);

                errorResponse.StatusCode.Should().Be(expectedStatusCode);
                errorResponse.Message.Should().Be(expectedResponseText);
            }
            mockRequestDelegate.Verify(x => x.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestNoRequiredGoogleGroupsInToken_HasForbiddenResponse()
        {
            Environment.SetEnvironmentVariable("REQUIRED_GOOGLE_GROUPS", "GoodGroup; HackneyAll;");
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", "/development/swagger/v1.0/swagger.json");
            Token expectedToken = new Token 
            {
                Groups = new string[] { "HackneyAll", "BadGroup" }
            };
            var expectedResponseText = "Forbidden";
            var expectedStatusCode = (int)HttpStatusCode.Forbidden;

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Path = "/development/swagger/v1.0/swagger.json  ";

            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Returns(expectedToken);

            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));

            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            await sut.Invoke(httpContext, mockTokenFactory.Object).ConfigureAwait(false);

            httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
            httpContext.Response.Body.Position = 0;
            using (StreamReader streamReader = new StreamReader(httpContext.Response.Body))
            {
                string actualResponseText = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                var errorResponse = JsonConvert.DeserializeObject<BaseErrorResponse>(actualResponseText);

                errorResponse.StatusCode.Should().Be(expectedStatusCode);
                errorResponse.Message.Should().Be(expectedResponseText);
            }
            mockRequestDelegate.Verify(x => x.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }
        
        [Fact]
        public async Task GoogleGroupsAuthorizationMiddlewareInvoke_TestValidToken_CallsNextDelegate()
        {
            Environment.SetEnvironmentVariable("URLS_TO_SKIP_AUTH", "/swagger/v1.0/swagger.json");
            Environment.SetEnvironmentVariable("REQUIRED_GOOGLE_GROUPS", "GoodGroup; HackneyAll;");
            Token expectedToken = new Token 
            {
                Groups = new string[] { "HackneyAll", "GoodGroup", "SomeMoreGroup" }
            };

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Path = "/swagger/v1.0/swagger.json  ";

            var mockTokenFactory = new Mock<ITokenFactory>();
            mockTokenFactory.Setup(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .Returns(expectedToken);

            var mockRequestDelegate = new Mock<RequestDelegate>();
            mockRequestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));
            
            var sut = new GoogleGroupsAuthorizationMiddleware(mockRequestDelegate.Object);
            await sut.Invoke(httpContext, mockTokenFactory.Object).ConfigureAwait(false);

            mockRequestDelegate.Verify(x => x.Invoke(httpContext), Times.Once);
        }
    }
}
