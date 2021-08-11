using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Hackney.Core.DynamoDb.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb.HealthCheck
{
    public class DynamoDbHealthCheckTests
    {
        private readonly Mock<IAmazonDynamoDB> _mockClient;

        public DynamoDbHealthCheckTests()
        {
            _mockClient = new Mock<IAmazonDynamoDB>();
        }

        [Fact]
        public void DynamoDbHealthCheckConstructorTestSuccess()
        {
            (new DynamoDbHealthCheck<TestModelDb>(_mockClient.Object)).Should().NotBeNull();
        }

        [Fact]
        public void DynamoDbHealthCheckConstructorTestFails()
        {
            Action act = () => _ = new DynamoDbHealthCheck<TestModel>(_mockClient.Object);
            act.Should().Throw<ArgumentException>().WithMessage($"Type {typeof(TestModel).Name} does not have the DynamoDBTable attribute applied to it.");
        }

        [Fact]
        public async Task CheckHealthAsyncTestSucceeds()
        {
            _mockClient.Setup(x => x.DescribeTableAsync("Models", default)).ReturnsAsync(new DescribeTableResponse());

            var sut = new DynamoDbHealthCheck<TestModelDb>(_mockClient.Object);
            var result = await sut.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
            result.Status.Should().Be(HealthStatus.Healthy);
        }

        [Fact]
        public async Task CheckHealthAsyncTestFails()
        {
            var ex = new Exception("Something bad happened");
            _mockClient.Setup(x => x.DescribeTableAsync("Models", default)).ThrowsAsync(ex);

            var sut = new DynamoDbHealthCheck<TestModelDb>(_mockClient.Object);
            var result = await sut.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Exception.Should().Be(ex);
        }
    }
}
