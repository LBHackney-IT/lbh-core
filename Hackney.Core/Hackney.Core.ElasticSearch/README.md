# Hackney.Core.ElasticSearch

This package contains helpers and common funcitonality that can be employed when using ElasticSearch.

* [DI Registration](#DI Registration)
* [Health check](#ElasticSearch-Health-check)


## DI Registration

There is a `ConfigureElasticSearch()` extension method provided to facilitate setting up an application to use AWS DynamoDb.
THis will register an `ElasticClient` instance configured with a `SingleNodeConnectionPool`. 
The domain url is retrieved from the configuration using the key supplied. 
If no value is found in the configuration then a default of http://localhost:9200 is used.
Alternatively a custom default can be supplied.

By calling it in the application startup, the following interfaces will be configured in the DI container:
* `IElasticClient`
* `IWildCardAppenderAndPrepender`

#### Usage
```csharp
using Hackney.Core.ElasticSearch;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.ConfigureElasticSearch(Configuration, "ELASTICSEARCH_DOMAIN_URL");
            ...
        }
    }
}

```

## ElasticSearch Health Check
There is an `ElasticSearchHealthCheck` class implemented that uses the 
[Microsoft Health check framework](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.2).
The check verifies that the ElasticSearch instance configured is accessible by attempting to ping it .

##### Usage
```csharp
using Hackney.Core.ElasticSearch.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddElasticSearchHealthCheck();
            ...
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
            ...
        }
    }
}

```
