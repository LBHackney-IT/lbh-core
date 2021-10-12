using Elasticsearch.Net;
using FluentAssertions;
using Hackney.Core.ElasticSearch.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Nest;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.ElasticSearch.HealthCheck
{
    public class ElasticSearchHealthCheckTests
    {
        private readonly Mock<IElasticClient> _mockClient;

        public ElasticSearchHealthCheckTests()
        {
            _mockClient = new Mock<IElasticClient>();

            var connSettings = new ConnectionSettings();
            _mockClient.SetupGet(x => x.ConnectionSettings).Returns(connSettings);
        }

        private static PingResponse ConstructPingResponse(int statusCode)
        {
            var mockApiCall = new Mock<IApiCallDetails>();
            mockApiCall.SetupGet(x => x.HttpStatusCode).Returns(statusCode);
            var mockResponse = new Mock<PingResponse>();
            mockResponse.SetupGet(x => x.ApiCall).Returns(mockApiCall.Object);
            return mockResponse.Object;
        }

        [Fact]
        public void DynamoDbHealthCheckConstructorTestSuccess()
        {
            (new ElasticSearchHealthCheck(_mockClient.Object)).Should().NotBeNull();
        }

        [Fact]
        public async Task CheckHealthAsyncTestSucceeds()
        {
            _mockClient.Setup(x => x.PingAsync((Func<PingDescriptor, IPingRequest>)null, default))
                       .ReturnsAsync(ConstructPingResponse(200));

            var sut = new ElasticSearchHealthCheck(_mockClient.Object);
            var result = await sut.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
            result.Status.Should().Be(HealthStatus.Healthy);
        }

        [Fact]
        public async Task CheckHealthAsyncTestFails()
        {
            _mockClient.Setup(x => x.PingAsync((Func<PingDescriptor, IPingRequest>)null, default))
                       .ReturnsAsync(ConstructPingResponse(500));

            var sut = new ElasticSearchHealthCheck(_mockClient.Object);
            var result = await sut.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Exception.Should().BeNull();
        }

        [Fact]
        public async Task CheckHealthAsyncTestFailsWithException()
        {
            var ex = new Exception("Something bad happened");
            _mockClient.Setup(x => x.PingAsync((Func<PingDescriptor, IPingRequest>)null, default)).ThrowsAsync(ex);

            var sut = new ElasticSearchHealthCheck(_mockClient.Object);
            var result = await sut.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Exception.Should().Be(ex);
        }
    }
}
