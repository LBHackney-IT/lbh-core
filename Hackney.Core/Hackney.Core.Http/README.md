# Hackney.Core.Http

This package contains helpers and common functionality that can be employed when using Http requests and responses.

* [ApiGateway](#ApiGateway)
* [IHttpContextWrapper](#IHttpContextWrapper)
* [JsonOptions](#JsonOptions)

## ApiGateway

The ApiGateway and interface provide the basic functionality of calling an external Api GET endpoint to retrieve a single entity - a simple Get by Id request.
It is packaged up here so that developers do not have to write the same boiler-plate code each time this common functionality is required.

### Usage

### DI Registration
Use the extension method to add the IApiGateway interface into the DI container on startup.
```csharp
using Hackney.Core.Http;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddApiGateway();

            // Also register the actual Gateway(s) that use IApiGateway            
            services.AddScoped<ISomeEntityApi, SomeEntityApi>();
            ...
        }
    }
}

```

### Implementation
Create a "Gateway" class as normal to encapsulate calling the required external Api. 
The class should inject an instance of the IApiGatewayinto and should initialise it with the required name, and configuration keys for the uri and token.

This class should implement a single `async` Get method, but internally delegate the actual call to the instance of the IApiGateway inected into the class.
This method will return either the requested entity, null, or throw a `GetFromApiException` exception for any other response from the Api call.

It is assumed that the Api's base uri and token are set in the application configuration or as environment variables. 
This would be standard practice so that the appropriate uri/token can be injected into the application container depending on the environment.
The container tests should ensure that these environment variables are set appropriately.  

```csharp
using Hackney.Core.Http;
using Hackney.Core.Logging;
using System;
using System.Threading.Tasks;
using SomeApi.Domain.SomeEntity;
using SomeApi.Gateway.Interfaces;

namespace SomeApi.Gateway
{
    public class SomeEntitytApi : ISomeEntityApi
    {
        private const string ApiName = "SomeEntity";
        private const string AccountApiUrl = "SomeEntityApiUrl";
        private const string AccountApiToken = "SomeEntityApiToken";

        private readonly IApiGateway _apiGateway;

        public SomeEntityApi(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, AccountApiUrl, AccountApiToken);
        }

        [LogCall]
        public async Task<SomeEntityResponseObject> GetSomeEntityByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/someentity/{id}";
            return await _apiGateway.GetByIdAsync<SomeEntityResponseObject>(route, id, correlationId);
        }
    }
}
```

### Testing
To test gateway classes that use `IApiGateway`, unit tests can be written as normal.

E2E tests that expect the external Api to be available should make use of the [BaseApiFixture](../Hackney.Core.Testing/Hackney.Core.Testing.Shared/README.md#BaseApiFixture) 
class in the `Hackney.Core.Testing.Shared` package.

## IHttpContextWrapper

The `IHttpContextWrapper` interface provdes a helper method to easily get at the headres dictionary of the current Http request.
It can be registered within the DI container inthe applicaiton startup using the `IServiceCollection` extension method `AddHttpContextWrapper()`.

## JsonOptions
The static `JsonOptions` class contains a single `Create()` method to create a standard `JsonSerializerOptions` object to be used in entity serialisation 
and deserialisation.
The options object created has the following pre-configured:
* Camel case naming 
* Indents each line
* Uses the JsonStringEnumConverter for enum values