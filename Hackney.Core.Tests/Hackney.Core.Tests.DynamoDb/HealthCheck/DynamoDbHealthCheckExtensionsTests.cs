using FluentAssertions;
using Hackney.Core.DynamoDb;
using Hackney.Core.DynamoDb.HealthCheck;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb.HealthCheck
{
    public class DynamoDbHealthCheckExtensionsTests
    {

        [Fact]
        public void RegisterDynamoDbHealthCheckTest()
        {
            var services = new ServiceCollection();
            _ = services.RegisterDynamoDbHealthCheck<TestModelDb>();

            services.IsServiceRegistered<IHealthCheck, DynamoDbHealthCheck<TestModelDb>>().Should().BeTrue();
        }

        [Fact]
        public void ServiceCollectionAddDynamoDbHealthCheckTest()
        {
            var services = new ServiceCollection();
            _ = services.AddDynamoDbHealthCheck<TestModelDb>();

            services.IsServiceRegistered<IHealthCheck, DynamoDbHealthCheck<TestModelDb>>().Should().BeTrue();

            // We can't explicitly verify the Healthcheck builder reigstration here as it is not accessible.
            // We have to reply on the test below to do that for us.
        }

        [Fact]
        public void HealthChecksBuilderAddDynamoDbHealthCheckTest()
        {
            var mockBuilder = new Mock<IHealthChecksBuilder>();
            _ = mockBuilder.Object.AddDynamoDbHealthCheck<TestModelDb>();

            mockBuilder.Verify(x => x.Add(It.Is<HealthCheckRegistration>(hcr => hcr.Name == "DynamoDb"
                                                                             && hcr.Factory != null)), Times.Once);
        }
    }
}
