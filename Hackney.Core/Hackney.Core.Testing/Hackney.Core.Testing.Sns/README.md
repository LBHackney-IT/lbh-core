# Hackney.Core.Testing.Sns NuGet Package

This package is designed to assist with testing services that use Sns.
It contains a fixture that can be used to set up a local Sns instance so that it will be used by code under test.

* [Configuring the Sns fixture](#Configuring-the-Sns-fixture)
* [ISnsEventVerifier](#ISnsEventVerifier)
* [ISnsFixture](#ISnsFixture)

## Configuring the Sns fixture
As a concept, the fixture is a class that ensures that a local Sns instance is set up and accessible by both the 
code under test and the tests themselves.

The test project may well contain such a class if there is existing local code that configures a local database instance, 
however with this common implementation, the order of creation of the application factory and fixture has been inverted. 
I.e. Here it is the application factory that is set on the xunit collection, and then the factory constructs and 
configures the fixture when xunit creates it.

In addition the mock (web) applicaiton factory should implement the Dispose pattern and dispose of the SnsFixture appropriately. 
In this way all topics and queues created for a test run are removed when it is done.

### Api projects
#### 1. Create / modify the mock web application factory
`CreateSnsTopic` should be called once for each topic that needs to be created.

```csharp
using Hackney.Core.Sns;
using Hackney.Core.Testing.Sns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SomeApplication.Tests
{
    public class MockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        public ISnsFixture SnsFixture { get; private set; }
        public HttpClient Client { get; private set; }

        public MockWebApplicationFactory()
        {
            EnsureEnvVarConfigured("Sns_LocalMode", "true");
            EnsureEnvVarConfigured("Sns_LocalServiceUrl", "http://localhost:8000");

            Client = CreateClient();
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != SnsFixture)
                    SnsFixture.Dispose();

                base.Dispose(true);

                _disposed = true;
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<Startup>();
            builder.ConfigureServices(services =>
            {
                services.ConfigureSns();
                services.ConfigureSnsFixture();

                var serviceProvider = services.BuildServiceProvider();

                SnsFixture = serviceProvider.GetRequiredService<ISnsFixture>();
                SnsFixture.CreateSnsTopic<EntityEventSns>("some-topic-name", "ENV-VAR-NAME-FOR-TOPIC-ARN");
            });
        }
    }
}

```

#### 2. Create an xunit collection
The mock application factory is specified on the collection declaration. 
This only need be done once for the `MockWebApplicationFactory` class.

```csharp
using Xunit;

namespace SomeApplication.Tests
{
    [CollectionDefinition("AppTest collection", DisableParallelization = true)]
    public class AppTestCollection : ICollectionFixture<MockWebApplicationFactory<Startup>>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
```

### Non-Api projects
Non-Api projects construct the `IHostBuilder` slightly differently to that for Api projects.
#### 1. Create / modify the mock application factory
`CreateSnsTopic` should be called once for each topic that needs to be created.

```csharp
using Hackney.Core.Sns;
using Hackney.Core.Testing.Sns;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace SomeApplication.Tests
{
    public class MockApplicationFactory
    
        public ISnsFixture SnsFixture { get; private set; }

        public MockApplicationFactory()
        {
            CreateHostBuilder().Build();
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
                if (null != SnsFixture)
                    SnsFixture.Dispose();

                base.Dispose(true);

                _disposed = true;
            }
        }

        public IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder(null)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               // Configures the conneciton
               services.ConfigureSns();
               services.ConfigureSnsFixture();

               var serviceProvider = services.BuildServiceProvider();

               LogCallAspectFixture.SetupLogCallAspect();

               SnsFixture = serviceProvider.GetRequiredService<ISnsFixture>();
               SnsFixture.CreateSnsTopic<EntityEventSns>("some-topic-name", "ENV-VAR-NAME-FOR-TOPIC-ARN");
           });
    }
}
```

#### 2. Create an xunit collection
The mock application factory is specified on the collection declaration.
This only need be done once for the `MockApplicationFactory` class.

```csharp
using Xunit;

namespace SomeApplication.Tests
{
    [CollectionDefinition("AppTest collection", DisableParallelization = true)]
    public class AppTestCollection : ICollectionFixture<MockApplicationFactory>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
```

### Using the xunit collection
To use the xunit collection:
* Adorn the class declaration of the test class that will be testing Sns - this will usually be an E2E 'story' class.
* The mock application factory will be passed to the test class constructor, and the `ISnsFixture` reference is accesible from it.
* For a given test, add a method to the 'steps' class to verify the expected even was raised.

```csharp
using Hackney.Core.Testing.Sns;
using SomeApplication.Tests.V1.E2ETests.Fixtures;
using SomeApplication.Tests.V1.E2ETests.Steps;
using SomeApplication.V1.Gateways;
using System;
using TestStack.BDDfy;
using Xunit;

namespace SomeApplication.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to do a thing ",
        SoThat = "I do that thing when the need rises")]
    [Collection("AppTest collection")]
    public class SomeActionStory : IDisposable
    {
        private readonly ISnsFixture _snsFixture;
        private readonly SomeActionFixture _someActionFixture;
        private readonly SomeActionSteps _steps;

        public SomeSnsGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _snsFixture = appFactory.SnsFixture;
            _someActionFixture = new SomeActionFixture(_snsFixture.SimpleNotificationService);
            _steps = new CreateContactSteps(appFactory.Client);
        }


        [Fact]
        public void ServiceReturnsTheRequestedThing()
        {
            this.Given(g => _someActionFixture.GivenSomeRequest())
                .When(w => _steps.WhenTheEndpointIsCalled(_someActionFixture.RequestObject))
                .Then(t => _steps.ThenTheThingIsSavedAndReturned(_someActionFixture))
                .Then(t => _steps.ThenTheThingCreatedEventIsRaised(_snsFixture))
                .BDDfy();
        }
    }
}
```

#### Verifying an event was raised
Create a method that calls the appropriate `SnsEventVerifier` passing it an action that will be used to test all event messages raised
to see if it is the one expected.
```csharp
public async Task ThenTheThingCreatedEventIsRaised(ISnsFixture snsFixture)
{
    // Get the object reurned from the successful Api call
    var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);

    // Construct the verification action
    Action<EventEntitySns> verifyFunc = (actual) =>
    {
        actual.CorrelationId.Should().NotBeEmpty();
        actual.DateTime.Should().BeCloseTo(DateTime.UtcNow, 1000);
        actual.EntityId.Should().Be(apiResult.TargetId);

        // Verify that the actual.EventData OldData and NewData properties contain the right data
        ...

        actual.EventType.Should().Be(EventConstants.CREATED);
        actual.Id.Should().NotBeEmpty();
        actual.SourceDomain.Should().Be(EventConstants.SOURCEDOMAIN);
        actual.SourceSystem.Should().Be(EventConstants.SOURCESYSTEM);
        actual.User.Email.Should().Be("e2e-testing@development.com");
        actual.User.Name.Should().Be("Tester");
        actual.Version.Should().Be(EventConstants.V1VERSION);
    };

    // Retrieve the right verifier reference
    var snsVerifer = snsFixture.GetSnsEventVerifier<EventEntitySns>();

    // Verify our event was raised
    (await snsVerifer.VerifySnsEventRaised<EventEntitySns>(verifyFunc)).Should().BeTrue(snsVerifer.LastException?.Message);
}
```

## ISnsEventVerifier
This object can be used to verify that the expected event was actually raised with the expected data.
Every SnsEventVerifier instance is specific to a Sns topic, and as a result can only validate message sent to that topic and that topic only.
It works by creating a temporary Sqs queue and the subscriing that queue to the specified Sns topic. 
This means any event raised to the topic will get sent to the temporary ueue

It contains the following:
#### Properties
* `LastException` - this is the last exception raised during the process of verifiyin if a message contains the expected data.
It is exposed so that the test code can see what exactly caused the failure.

#### Methods
* `PurgeQueueMessages` - This method will purge all messages from the temporary Sqs queue created to receive events for this topic.
* `VerifySnsEventRaised` - Verifies that an event was raised for this topic. Supply an action that will be used to verify a message 
was sent containing the expected data.


## ISnsFixture
This is the object that is injected into the mock application factory when the xunit collection is created and 
allows the tests to verify that expected events get raised.

It contains the following:
#### Properties
* `SimpleNotificationService` property - An `ISimpleNotificationService` reference for accessing the the Sns service.
* `AmazonSQS` property - An `AmazonSQS` reference for accessing the Sqs service.

#### Methods
* `CreateSnsTopic` - Called within the mock application factory's startup to create the required Sns topic used by the code under test.
* `GetSnsEventVerifier` - A method that can be used by tests to retrieve the `SnsEventVerifier` for the spcified topic so that the test 
can validate that the expected event was actually raised with the expected data.
 