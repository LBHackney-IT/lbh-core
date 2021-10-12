using FluentAssertions;
using Hackney.Core.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nest;
using System;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.ElasticSearch
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private const string ConfigKey = "ELASTICSEARCH_DOMAIN_URL";
        private const string EsNodeUrl = "http://somedomain:9200";
        private const string DefaultESDomainUrl = "http://localhost:9200";

        public ServiceCollectionExtensionsTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            ConfigureConfig(_mockConfiguration, EsNodeUrl);
        }

        private static void ConfigureConfig(Mock<IConfiguration> mockConfig, string url)
        {
            var section = new Mock<IConfigurationSection>();
            section.Setup(x => x.Key).Returns(ConfigKey);
            section.Setup(x => x.Value).Returns(url);
            mockConfig.Setup(x => x.GetSection(ConfigKey))
                              .Returns(section.Object);
        }

        [Fact]
        public void ConfigureElasticSearchTestNullServicesThrows()
        {
            Action act = () => ServiceCollectionExtensions.ConfigureElasticSearch((IServiceCollection)null, _mockConfiguration.Object, ConfigKey);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConfigureElasticSearchTestNullConfigurationThrows()
        {
            Action act = () => ServiceCollectionExtensions.ConfigureElasticSearch(new ServiceCollection(), null, ConfigKey);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        public void ConfigureElasticSearchTestInvalidConfigKeyThrows(string key)
        {
            Action act = () => ServiceCollectionExtensions.ConfigureElasticSearch(new ServiceCollection(), _mockConfiguration.Object, key);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConfigureElasticSearchTestNullDefaultUriThrows()
        {
            Action act = () => ServiceCollectionExtensions.ConfigureElasticSearch(new ServiceCollection(), _mockConfiguration.Object, ConfigKey, null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ConfigureElasticSearchTestRegistersServicesWithDefaultDomain(string configUrl)
        {
            ConfigureConfig(_mockConfiguration, configUrl);

            var services = new ServiceCollection();
            services.ConfigureElasticSearch(_mockConfiguration.Object, ConfigKey);

            _mockConfiguration.Verify(x => x.GetSection(ConfigKey), Times.Once);

            var serviceProvider = services.BuildServiceProvider();
            var esClient = serviceProvider.GetService<IElasticClient>();
            esClient.Should().NotBeNull();
            esClient.ConnectionSettings.ConnectionPool.Nodes.Count.Should().Be(1);
            esClient.ConnectionSettings.ConnectionPool.Nodes.First().Uri.Should().BeEquivalentTo(new Uri(DefaultESDomainUrl));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ConfigureElasticSearchTestRegistersServicesWithCustomDefaultDomain(string configUrl)
        {
            ConfigureConfig(_mockConfiguration, configUrl);

            var customDefaultDomain = new Uri(EsNodeUrl);
            var services = new ServiceCollection();
            services.ConfigureElasticSearch(_mockConfiguration.Object, ConfigKey, customDefaultDomain);

            _mockConfiguration.Verify(x => x.GetSection(ConfigKey), Times.Once);

            var serviceProvider = services.BuildServiceProvider();
            var esClient = serviceProvider.GetService<IElasticClient>();
            esClient.Should().NotBeNull();
            esClient.ConnectionSettings.ConnectionPool.Nodes.Count.Should().Be(1);
            esClient.ConnectionSettings.ConnectionPool.Nodes.First().Uri.Should().BeEquivalentTo(customDefaultDomain);
        }

        [Fact]
        public void ConfigureElasticSearchTestRegistersServicesWithConfiguredDomain()
        {
            var services = new ServiceCollection();
            services.ConfigureElasticSearch(_mockConfiguration.Object, ConfigKey);

            _mockConfiguration.Verify(x => x.GetSection(ConfigKey), Times.Once);

            var serviceProvider = services.BuildServiceProvider();
            var esClient = serviceProvider.GetService<IElasticClient>();
            esClient.Should().NotBeNull();
            esClient.ConnectionSettings.ConnectionPool.Nodes.Count.Should().Be(1);
            esClient.ConnectionSettings.ConnectionPool.Nodes.First().Uri.Should().BeEquivalentTo(new Uri(EsNodeUrl));
        }
    }
}
