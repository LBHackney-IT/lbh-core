using AutoFixture;
using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Core.Http.Exceptions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Http
{
    public class ApiGatewayTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ApiGateway _sut;

        private readonly static JsonSerializerOptions _jsonOptions = CreateJsonOptions();
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string ApiRoute = "https://some-domain.com/api/";
        private const string ApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public ApiGatewayTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                                  .Returns(_httpClient);

            var inMemorySettings = new Dictionary<string, string> {
                { "ApiUrl", ApiRoute },
                { "ApiToken", ApiToken }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new ApiGateway(_mockHttpClientFactory.Object, _configuration);
            _sut.Initialise("ApiName", "ApiUrl", "ApiToken");
        }

        private static string Route(Guid id) => $"{ApiRoute}accounts/{id}";

        private static bool ValidateRequest(string expectedRoute, Dictionary<string, string> headers, HttpRequestMessage request)
        {
            if ((headers != null) && headers.Any())
            {
                foreach (var h in headers)
                {
                    if (!request.Headers.Contains(h.Key)
                            || request.Headers.GetValues(h.Key)?.FirstOrDefault() != h.Value)
                        return false;
                }
            }

            var correlationIdHeader = request.Headers.GetValues("x-correlation-id")?.FirstOrDefault();
            return (request.RequestUri.ToString() == expectedRoute)
                && (request.Headers.Authorization.ToString() == ApiToken)
                && (correlationIdHeader == _correlationId.ToString());
        }

        private void SetupHttpClientResponse(string route, SomeResponseObject response, Dictionary<string, string> headers = null)
        {
            HttpStatusCode statusCode = (response is null) ?
                HttpStatusCode.NotFound : HttpStatusCode.OK;
            HttpContent content = (response is null) ?
                null : new StringContent(JsonSerializer.Serialize(response, _jsonOptions));
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => ValidateRequest(route, headers, y)),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = statusCode,
                       Content = content,
                   });
        }

        private void SetupHttpClientErrorResponse(string route, string response)
        {
            HttpContent content = (response is null) ? null : new StringContent(response);
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.InternalServerError,
                       Content = content,
                   });
        }

        private void SetupHttpClientException(string route, Exception ex)
        {
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ThrowsAsync(ex);
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        [Fact]
        public void ApiGatewayConstructorTest()
        {
            var sut = new ApiGateway(_mockHttpClientFactory.Object, _configuration);
            sut.ApiName.Should().BeNull();
            sut.ApiRoute.Should().BeNull();
            sut.ApiToken.Should().BeNull();
            sut.RequestHeaders.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void InitialiseTestInvalidApiNameConfigThrows(string invalidValue)
        {
            Action act = () => _sut.Initialise(invalidValue, "ApiUrl", "ApiToken");
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("sdrtgdfstg")]
        public void InitialiseTestInvalidRouteConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "ApiUrl", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var sut = new ApiGateway(_mockHttpClientFactory.Object, _configuration);
            Action act = () => sut.Initialise("ApiName", "ApiUrl", "ApiToken");
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void InitialiseTestInvalidTokenConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "ApiUrl", ApiRoute },
                { "ApiToken", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var sut = new ApiGateway(_mockHttpClientFactory.Object, _configuration);
            Action act = () => sut.Initialise("ApiName", "ApiUrl", "ApiToken");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void InitialiseTestSucceeds()
        {
            var sut = new ApiGateway(_mockHttpClientFactory.Object, _configuration);
            sut.Initialise("ApiName", "ApiUrl", "ApiToken");

            sut.ApiName.Should().Be("ApiName");
            sut.ApiRoute.Should().Be(ApiRoute.Trim('/'));
            sut.ApiToken.Should().Be(ApiToken);
            sut.RequestHeaders.Should().BeEmpty();
        }

        [Fact]
        public void InitialiseTestWithHeadersSucceeds()
        {
            var headers = new Dictionary<string, string>
            {
                { "key", "some-value" }
            };
            var sut = new ApiGateway(_mockHttpClientFactory.Object, _configuration);
            sut.Initialise("ApiName", "ApiUrl", "ApiToken", headers);

            sut.ApiName.Should().Be("ApiName");
            sut.ApiRoute.Should().Be(ApiRoute.Trim('/'));
            sut.ApiToken.Should().Be(ApiToken);
            sut.RequestHeaders.Should().BeEquivalentTo(headers);
        }

        [Fact]
        public void GetByIdAsyncTestNotInitialisedThrows()
        {
            var id = Guid.NewGuid();
            var route = $"{ApiRoute}accounts/{id}";

            var sut = new ApiGateway(_mockHttpClientFactory.Object, _configuration);
            Func<Task<SomeResponseObject>> act =
                async () => await sut.GetByIdAsync<SomeResponseObject>(route, id, _correlationId).ConfigureAwait(false);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetByIdAsyncGetExceptionThrown()
        {
            var id = Guid.NewGuid();
            var exMessage = "This is an exception";
            var route = Route(id);
            SetupHttpClientException(route, new Exception(exMessage));

            Func<Task<SomeResponseObject>> func =
                async () => await _sut.GetByIdAsync<SomeResponseObject>(route, id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public void GetByIdAsyncCallFailedExceptionThrown()
        {
            var id = Guid.NewGuid();
            var error = "This is an error message";
            var route = Route(id);
            SetupHttpClientErrorResponse(route, error);

            Func<Task<SomeResponseObject>> func =
                async () => await _sut.GetByIdAsync<SomeResponseObject>(route, id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<GetFromApiException>();
        }

        [Fact]
        public async Task GetByIdAsyncNotFoundReturnsNull()
        {
            var id = Guid.NewGuid();
            var route = Route(id);
            SetupHttpClientResponse(route, null);

            var result = await _sut.GetByIdAsync<SomeResponseObject>(route, id, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsyncCallReturnsResult()
        {
            var id = Guid.NewGuid();
            var route = Route(id);
            var expectedResult = new Fixture().Create<SomeResponseObject>();
            SetupHttpClientResponse(route, expectedResult);

            var result = await _sut.GetByIdAsync<SomeResponseObject>(route, id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task GetByIdAsyncCallReturnsResultWithCustomHeaders()
        {
            var id = Guid.NewGuid();
            var route = Route(id);
            var expectedResult = new Fixture().Create<SomeResponseObject>();

            var customHeaders = new Dictionary<string, string>
            {
                { "Test", "headervalue" }
            };
            _sut.Initialise("ApiName", "ApiUrl", "ApiToken", customHeaders);
            SetupHttpClientResponse(route, expectedResult, customHeaders);

            var result = await _sut.GetByIdAsync<SomeResponseObject>(route, id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(expectedResult);
        }
    }

    public class SomeResponseObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
