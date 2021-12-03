# Hackney.Core.Testing.PactBroker NuGet Package

This package is designed to assist with implementing a test to verify an AspNetCore v.3.1 API has not broken a pact 
configured witin [Pact Broker](https://github.com/pact-foundation/pact_broker).

##### Note
**This package targets `.NETCoreApp v.3.1` and not `.NETStandard`. 
This means it can only be used with AspNetCore v.3.1 (or later) API applications.**

The package contains a number of different classes to help setup the target Api application for a Pact Broker verification test.
The instructions below use xUnit as the test framework, but they could easily be adapted to use a different framework.

## Assumptions
* The actual pact definition and broker configuration has already been done.
* Pact broker verification would *only* be executed when pushing code to the *development* environment.
* The current implementation assumes that the pact broker website specified only needs a simple user and and password in order to connect to it.
* The API endpoints under test do not require anything more than a very basic token in the requests made by the pact broker to 
it whilst the verification tests are being processed.

Both of these things can be easily extended to add more complex functionlity if required.

## Required environment variables
The following 4 environment variables are required when running a pact broker verification test.

| Name | Description |
| --- | --- |
| PactBrokerUser | The usename required to authenticate with the pact broker. |
| PactBrokerUserPassword | The password required to authenticate with the pact broker. See [below](#The-Pact-Broker-password). |
| PactBrokerProviderName | The uri of the route of the pact broker instance to use. For example: https://contract-testing-development.hackney.gov.uk/ |
| PactBrokerPath | The provider name used for this API application within the specified pact broker instance. |

### The Pact Broker password
#### Local development
For local development, to avoid hard coding the password it should be set as global environment variable within the local machine.
#### CircleCi pipeline execution
When running within a CircleCi pipeline, the pipeline config should implement some way to retrieve the broker password from 
a secure store and inject it into the test process.
##### 1. Set the AWS context in the workflow
Ensure that the AWS context is set on the pipeline before development `build-and-test` job is run 
(it may well currently be being set after the `build-and-test` job):
```yml
  check-and-deploy-development:
    jobs:
      - check-code-formatting:
          context: api-nuget-token-context
      - assume-role-development:
          context: api-assume-role-housing-development-context
      - build-and-test:
          context: api-nuget-token-context
          requires:
            - assume-role-development
      - terraform-init-and-plan-development:
          requires:
            - build-and-test
          filters:
            branches:
              only: master
```
##### 2. Modify the `build-and-test` job definition
The `build-and-test` job definition needs to be updated so that it retrieves the broker password from the AWS parameter store 
for the development environment and passes it to the the test container. In order to do this 2 initial steps are also needed 
(`attach_workspace` and installing the AWS Cli) that probably weren't there before:
```yml
  build-and-test:
    executor: docker-python
    steps:
      - *attach_workspace
      - aws-cli/install
      - checkout
      - setup_remote_docker
      - run:
          name: build
          command: docker-compose build person-api-test
      - run:
          name: Run tests
          command: |
            export PactBrokerUserPassword=$(aws ssm get-parameter --name /contract-testing/development/pact-broker-password --query Parameter.Value --output text) ;
            docker-compose run -e PactBrokerUserPassword=$PactBrokerUserPassword person-api-test
```


## Using the package (using xUnit)

In order to use the package to create an xUnit test that will verify the current API application agains a published pact, 
follow the steps below:

###  1. Add the package to the API's test project
The package to add is found in the Hackey Github package feed and is called: `Hackney.Core.Testing.PactBroker`.

Also create a subfolder in the test project called "PactBroker", which is where the files below should be createdd.


### 2. Create a pact broker handler
Create a class that implements the `IPactBrokerHandler` interface.

When pact broker verification is executed, the broker will call to the API on a specific route (`/provider-states`) so that the API
can perform any necessary setup for the API call the broker will make next. 
This handler class will handle these calls made by the broker.

To implement this class you will need to use the pact defined within the broker and add a state for every interaction 
declared there.

For every interaction add an entry to a ProviderStates dictionary, 
the key will be the exact text from the "Given" portion of the interaction and the value will be a 
function implemented to perform the necessary setup required to satisfy that interaction.

For example, if an interaction is calling a GET endpoint with a specific id, then the handler must implement a function
that will be called to add an entity to the API's data store with that id, so that when the broker calls the GET endpoint, 
that entity can be returned. 
The handler method should also ensure the all test data added gets removed when all verification checks are done

#### Example:
```csharp
using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using Hackney.Core.Testing.PactBroker;
using SomeEntityApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SomeEntityApi.Tests.PactBroker
{
    public class SomeEntityPactBrokerHandler : IPactBrokerHandler
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDynamoDBContext _dynamoDbContext;
        private static readonly List<Action> _cleanup = new List<Action>();

        public IDictionary<string, PactStateHandler> ProviderStates { get; private set; }

        public SomeEntityPactBrokerHandler(IDynamoDBContext dynamoDBContext)
        {
            _dynamoDbContext = dynamoDBContext;

            // The ProviderStates property contains the map of all broker interactions
            // together with the function that should run when that state is initiated.
            // The text key must match exactly that specified within the pact configured in the broker.
            ProviderStates = new Dictionary<string, PactStateHandler>
            {
                {
                    "the SomeEntity API has a entity with an id:6fbe024f-2316-4265-a6e8-d65a837e308a",
                    ARecordExists
                }
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var act in _cleanup)
                    act();
                _cleanup.Clear();

                _disposed = true;
            }
        }

        // This is the method executed for the specific state configured in the ProviderStates dictionary
        public void ARecordExists(string name, IDictionary<string, string> parameters)
        {
            // The record id will either be in the supplied parameters or in the state description
            string id = null;
            if (parameters.ContainsKey("id"))
                id = parameters["id"];
            else
                id = name.Split(':')?.Last();

            if (!string.IsNullOrEmpty(id))
            {
                var testEntity = _fixture.Build<SomeEntityDb>()
                                         .With(x => x.Id, Guid.Parse(id))
                                         .With(x => x.VersionNumber, (int?) null)
                                         .Create();
                _dynamoDbContext.SaveAsync<SomeEntityDb>(testEntity).GetAwaiter().GetResult();

                _cleanup.Add(async () => await _dynamoDbContext.DeleteAsync<SomeEntityDb>(testEntity).ConfigureAwait(false));
            }
        }
    }
}
```


### 3. Create a custom Startup class
This is needed because we need to inject custom middleware into our API applications MVC pipeline in order to deal with the calls made to it by the pact broker.
It also needs to register the handler class ([created above](#Create-a-pact-broker-handler)) that will be used by the additional middleware.

The custom Startup class should essentially wrap the API's existing Startup class, and should make sure it calls on to the existing code where necessary.

For example:
```csharp
using Hackney.Core.Testing.PactBroker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SomeEntityApi.Tests.PactBroker
{
    public class PactBrokerTestStartup
    {
        private readonly Startup _inner;

        public PactBrokerTestStartup(IConfiguration configuration)
        {
            // Create an instance of the API's "normal" Startup class
            _inner = new Startup(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // REQUIRED
            // Register our IPactBrokerHandler implementation
            services.AddScoped<IPactBrokerHandler, SomeEntityPactBrokerHandler>();

            // Run the normal ConfigureServices method.
            _inner.ConfigureServices(services);
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // REQUIRED
            // Use the extension method to add the required custom middleware
            // The string parameter is the Audience value set on the token generated by the middleware
            // and added to Http calls made by to the API by the pact broker
            app.UsePactBroker("some-entity-api");

            // Run the normal Configure method
            Startup.Configure(app, env, logger);
        }
    }
}
```


### 4. Create a pact broker fixture
The fixture class must inherit from the `PactBrokerFixture` abstract base class and supply the custom test Startup class [implemented above](#Create-a-custom-Startup-class).
`PactBrokerFixture` conatins a number of virtual methods that should be overiden as required.
##### SetEnvironmentVariables
This is executed first and should be used to ensure any necessary environment variables exist. 
This should also be used to set the required variables used by the pact broker test base classes to connect with the broker website ([see above](#Required-environment-variables)).
This is mostly relevant when running locally as when executed within a pipeline any necessary variables should have already been injected.
##### ConfigureServices
This should be used to add any necessary DI container registrations to the supplied `IServiceCollection`. 
This section would be the same as what is already done in the existing `MockWebApplicationFactory` class used by Gateway and E2E tests.
##### ConfigureFixture
This is executed after the DI container is built and should be used to resolve any references that might be required to set up any 
external services like a database or Sns.
#### Example
In this example the API application uses both DynamoDb and Sns.

```csharp
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Hackney.Core.DynamoDb;
using Hackney.Core.Sns;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.PactBroker;
using Hackney.Core.Testing.Sns;
using Microsoft.Extensions.DependencyInjection;

namespace SomeEntityApi.Tests.PactBroker
{
    public class SomeEntityPactBrokerFixture : PactBrokerFixture<PactBrokerTestStartup>
    {
        protected override void SetEnvironmentVariables()
        {
            // These variable are required by the API application
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");
            EnsureEnvVarConfigured("Sns_LocalMode", "true");
            EnsureEnvVarConfigured("Localstack_SnsServiceUrl", "http://localhost:4566");

            // These variables are required by the pact broker test
            // The broker password should be set in the local machine environment variables for local development
            EnsureEnvVarConfigured(Constants.ENV_VAR_PACT_BROKER_USER, "<SOME-USER-NAME>");
            EnsureEnvVarConfigured(Constants.ENV_VAR_PACT_BROKER_PATH, "<PACT-BROKER-URI>");
            EnsureEnvVarConfigured(Constants.ENV_VAR_PACT_BROKER_PROVIDER_NAME, "<PACT-PROVIDER-NAME>");

            base.SetEnvironmentVariables();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureDynamoDB();
            services.ConfigureDynamoDbFixture();

            services.ConfigureSns();
            services.ConfigureSnsFixture();

            // Required to make sure the AWSXRayRecorder does not bomb out when using "local" resources
            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;

            base.ConfigureServices(services);
        }

        protected override void ConfigureFixture(IServiceProvider provider)
        {
            var dynamoDbFixture = provider.GetRequiredService<IDynamoDbFixture>();
            dynamoDbFixture.EnsureTablesExist(DynamoDbTables.Tables);

            var snsFixture = provider.GetRequiredService<ISnsFixture>();
            snsFixture.CreateSnsTopic<EntityEventSns>("someentity", "SOME_SNS_ARN");

            base.ConfigureFixture(provider);
        }
    }
}

```


### 5. Create a pact broker xUnit collection
This collection will inject the fixture class when executed.

For example:
```csharp
using Xunit;

namespace SomeEntityApi.Tests.PactBroker
{
    [CollectionDefinition("Pact Broker collection", DisableParallelization = true)]
    public class PactBrokerCollection : ICollectionFixture<SomeEntityPactBrokerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
```


### 6. Create an xUnit test
This is a single test that will call the pact broker to run the verification checks against the API.

For example:
```csharp
using Hackney.Core.Testing.PactBroker;
using PactNet.Infrastructure.Outputters;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace SomEntityApi.Tests.PactBroker
{
    [Collection("Pact Broker collection")]
    public class PactBrokerTests
    {
        private readonly SomEntityPactBrokerFixture _testFixture;
        private readonly ITestOutputHelper _outputHelper;

        public PactBrokerTests(SomEntityPactBrokerFixture testFixture, ITestOutputHelper output)
        {
            _testFixture = testFixture;
            _outputHelper = output;
        }

        [Fact]
        public void EnsureTheApiHonoursPactWithConsumer()
        {
            // There is a public method on the fixture base class that calls to the pact broker
            // to run the verification.
            // The supplied XUnitOutput instance allows the output to be streamed through xUnit 
            // to the VS output window.
            _testFixture.RunPactBrokerTest(new List<IOutput> { new XUnitOutput(_outputHelper) });
        }
    }
}
```
