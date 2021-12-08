using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PactNet;
using PactNet.Infrastructure.Outputters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.Testing.PactBroker
{
    /// <summary>
    /// Abstract base class for implementing a pact broker fixture
    /// </summary>
    /// <typeparam name="TStartup">The API Startup type</typeparam>
    public abstract class PactBrokerFixture<TStartup> : IDisposable where TStartup : class
    {
        protected readonly IHost _server;

        /// <summary>
        /// The server Uri used
        /// </summary>
        public string ServerUri { get; private set; }

        /// <summary>
        /// The Configuration interface
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        protected PactBrokerFixture()
        {
            ServerUri = Constants.DEFAULT_SERVER_URI;

            SetEnvironmentVariables();

            _server = Host.CreateDefaultBuilder()
                          .ConfigureWebHostDefaults(webBuilder =>
                          {
                              webBuilder.UseUrls(ServerUri);
                              webBuilder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                                        .UseStartup<TStartup>()
                                        .ConfigureServices((ctx, services) =>
                                        {
                                            ConfigureServices(services);

                                            var serviceProvider = services.BuildServiceProvider();

                                            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
                                            ConfigureFixture(serviceProvider);
                                        });
                          })
                          .Build();

            _server.Start();
        }

        /// <summary>
        /// Function to be used to set any required environment variables
        /// before the application DI container is set up and built.
        /// Override this as necessary in the derived implementation.
        /// </summary>
        protected virtual void SetEnvironmentVariables() { }

        /// <summary>
        /// Function to be used to configure the API DI container.
        /// Override this as necessary in the derived implementation.
        /// </summary>
        /// <param name="services">Services collection</param>
        protected virtual void ConfigureServices(IServiceCollection services) { }

        /// <summary>
        /// Function to be used to setup any required objects used by the tests after 
        /// the API DI container has been configured and built.
        /// Override this as necessary in the derived implementation.
        /// </summary>
        /// <param name="services">Services collection</param>
        protected virtual void ConfigureFixture(IServiceProvider provider) { }

        /// <summary>
        /// Function to be used within a test to execute a call to the pact broker to verify the API
        /// </summary>
        /// <param name="outputters"></param>
        public void RunPactBrokerTest(IList<IOutput> outputters = null)
        {
            if (outputters is null) outputters = new List<IOutput>();
            if (!outputters.Any(x => x.GetType() == typeof(ConsoleOutput)))
                outputters.Add(new ConsoleOutput());

            var pactVerifierConfig = new PactVerifierConfig
            {
                Outputters = outputters
            };

            var user = Configuration.GetValue<string>(Constants.ENV_VAR_PACT_BROKER_USER);
            var pwd = Configuration.GetValue<string>(Constants.ENV_VAR_PACT_BROKER_USER_PASSWORD);
            var pactUriOptions = new PactUriOptions().SetBasicAuthentication(user, pwd);

            var name = Configuration.GetValue<string>(Constants.ENV_VAR_PACT_BROKER_PROVIDER_NAME);
            var path = Configuration.GetValue<string>(Constants.ENV_VAR_PACT_BROKER_PATH);

            IPactVerifier pactVerifier = new PactVerifier(pactVerifierConfig);
            pactVerifier
                .ServiceProvider(name, ServerUri)
                .PactBroker(path, pactUriOptions)
                .ProviderState(ServerUri + Constants.PROVIDER_STATES_ROUTE)
                .Verify();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_server != null)
                    _server.Dispose();

                _disposed = true;
            }
        }

        protected void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }
    }
}
