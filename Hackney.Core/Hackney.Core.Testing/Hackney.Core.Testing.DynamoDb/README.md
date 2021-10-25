# Hackney.Core.Testing.DynamoDb NuGet Package

This package is designed to assist with testing services that use DynamoDb.
It contains a fixture that can be used to set up a local DynamoDb instance so that it will be used by code under test.

* [Configuring the DynamoDb fixture](#Configuring-the-DynamoDb-fixture)
* [IDynamoDbFixture](#IDynamoDbFixture)

## Configuring the DynamoDb fixture
As a concept, the fixture is a class that ensures that a local DynamoDb instance is set up and accessibly by both the 
code under test and the tests themselves.

The test project may well contain such a class if there is existing local code that configures a local database instance, 
however with this common implementation, the order of creation of the application factory and fixture has been inverted. 
I.e. Here it is the application factory that is set on the xunit collection, and then the factory constructs and 
configures the fixture when xunit creates it.

### Api projects
#### 1. Create / modify the mock web application factory
This should contain definitions of all the DynamoDb tables(s) that need to be created. 
The definition of each should be exactly how it is created by the project's terreaform configuration. 
The table difinitionis are then passed to the the fixture's `EnsureTablesExist()` method.

```csharp
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Hackney.Core.DynamoDb;
using Hackney.Core.Testing.DynamoDb;
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
        private readonly List<TableDef> _tables = new List<TableDef>
        {
            new TableDef
            {
                Name = "Things",
                KeyName = "id",
                KeyType = ScalarAttributeType.S
            }
        };

        public IDynamoDbFixture DynamoDbFixture { get; private set; }
        public HttpClient Client { get; private set; }

        public MockWebApplicationFactory()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");

            Client = CreateClient();
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<Startup>();
            builder.ConfigureServices(services =>
            {
                services.ConfigureDynamoDB();
                services.ConfigureDynamoDbFixture();

                var serviceProvider = services.BuildServiceProvider();

                DynamoDbFixture = serviceProvider.GetRequiredService<IDynamoDbFixture>();
                DynamoDbFixture.EnsureTablesExist(_tables);
            });
        }
    }
}

```

#### 2. Create an xunit collection
The mock application factory is specified on the collection declaration.

```csharp
using Xunit;

namespace SomeApplication.Tests
{
    [CollectionDefinition("DynamoDb collection", DisableParallelization = true)]
    public class DynamoDbCollection : ICollectionFixture<MockWebApplicationFactory<Startup>>
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
This should contain definitions of all the DynamoDb tables(s) that need to be created. 
The definition of each should be exactly how it is created by the project's terreaform configuration. 
The table difinitionis are then passed to the the fixture's `EnsureTablesExist()` method.

```csharp
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Hackney.Core.DynamoDb;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace SomeApplication.Tests
{
    public class MockApplicationFactory
    {
        private readonly List<TableDef> _tables = new List<TableDef>
        {
            new TableDef
            {
                Name = "Things",
                KeyName = "id",
                KeyType = ScalarAttributeType.S
            }
        };

        public IDynamoDbFixture DynamoDbFixture { get; private set; }

        public MockApplicationFactory()
        {
            CreateHostBuilder().Build();
        }

        public IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder(null)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               // Configures the conneciton
               services.ConfigureDynamoDB();
               services.ConfigureDynamoDbFixture();

               var serviceProvider = services.BuildServiceProvider();

               LogCallAspectFixture.SetupLogCallAspect();

               DynamoDbFixture = serviceProvider.GetRequiredService<IDynamoDbFixture>();
               DynamoDbFixture.EnsureTablesExist(_tables);
           });
    }
}
```


#### 2. Create an xunit collection
The mock application factory is specified on the collection declaration.

```csharp
using Xunit;

namespace SomeApplication.Tests
{
    [CollectionDefinition("DynamoDb collection", DisableParallelization = true)]
    public class DynamoDbCollection : ICollectionFixture<MockApplicationFactory>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
```

### Using the xunit collection
To use the xunit collection:
* Adorn the class declaration of the test class that will be testing DynamoDb access.
* The mock application factory will be passed to the test class constructor, and the `IDynamoDbFixture` reference is accesible from it.

```csharp
using FluentAssertions;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using SomeApplication.V1.Gateways;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SomeApplication.Tests.V1.Gateways
{
    [Collection("DynamoDb collection")]
    public class SomeDynamoDbGatewayTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly SomeDynamoDbGateway _classUnderTest;

        public SomeDynamoDbGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _classUnderTest = new SomeDynamoDbGateway(_dbFixture.DynamoDbContext);
        }

        ...
    }
}
```

## IDynamoDbFixture
This is the object that is injected into the mock application factory when the xunit collection is created and 
allows the tests to access the database to set up any required data and retrieve records to perform verifications.

It contains the following:
#### Properties
* `DynamoDbContext` property - A high-level `IDynamoDbContext` reference for accessing the database.
* `DynamoDb` property - A lower-level `IDynamoDb` reference for accessing the database.

#### Methods
* `EnsureTablesExist` - Called within the mock application factory's startup to ensure all the required tables exist 
before any tests are run.
* `SaveEntityAsync` - A method that can be used by tests to set up any data in the database that will be required to 
actually execute a test. This data will automatically be removed from the database when the test is over.