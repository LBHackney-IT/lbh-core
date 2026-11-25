# Hackney.Core NuGet Packages

At Hackney, we have created NuGet Packages to prevent the duplication of common code when implementing our APIs.
Hence this NuGet package will store common code that can then be used in relevant projects.

## Contributing

### Semi-automated Versioning & Branching Strategy

The CI/CD pipeline semi-automatically calculates and bumps the version number for all packages in Hackney.Core based on your branch name.
Semi-automatically because in the end you as developer need to specify what kind of version change it will be.

We follow Semantic Versioning (`Major.Minor.Patch[-Suffix]`):

- **Major**: Breaking changes _(renamed constructor, changed method signatures, modified enums, etc.)_
- **Minor**: New features, but backward compatible _(new methods or classes added that don't change inputs/outputs/behaviours of old ones)_
- **Patch**: Backwards compatible bug fixes only

**⚠️ IMPORTANT: Branch Naming Update**
The old `feature/<name>` convention has been **retired**!

Pushes modifying a package's source code or tests will trigger checks like formatting, tests, and a SonarCloud scan of that package from **any branch**.

However, package **will only be published if** your feature branch is correctly prefixed with the type of version bump your changes represent:

- `major/<your-branch-name>`
- `minor/<your-branch-name>`
- `patch/<your-branch-name>`

Only `major/`, `minor/`, and `patch/` branches calculate and publish preview versions. Merges to `release` calculate and publish stable versions. Stable version release cleans up preview versions.

For larger features this allows the breaking down of changes into multiple smaller PRs under non-prefixed feature branch merges before finally releasing them under semantic version prefixed branch.
Caveat to keep in mind, however, is that non-prefixed branches will not publish package's preview versions.

### Preview Versions

When working on a correctly prefixed branch (e.g., `minor/add-new-logging`), every push will automatically publish a preview version of your package.

Preview packages are now formatted using the calculated base version, your commit's short SHA, and the commit distance from the release branch:

- **Format:** `{BaseVersion}-{ShortSha}-p{CommitDistance}`
- **Example:** `1.5.0-f04c5a4-p3`

_(Note: The old 12-character branch name limit no longer applies! It was replaced due to often getting excessively truncated. Name your branch whatever makes the most sense)._

### Production Releases

To publish a stable production package, open a Pull Request and merge your branch into the `release` branch.

Upon merging:

1. The pipeline will publish the clean, stable version _(e.g., `1.5.0`)_.
2. It will automatically **delete all preview versions** tied to that specific base version _(e.g., `1.5.0-f04c5a4-p3`)_ from the registry to ensure no APIs stay permanently bound to preview versions that were forgotten to be changed in the API's `.csproj` _(this actually did used to happen)_.

---

## Building the package

After cloning the repo, you may find many errors relating to the `Hackney.Core.Testing.PactBroker` project similar to the one below when attempting to build the solution **_on a Windows machine_**:

```cmd
C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(4965,5): warning MSB3026: Could not copy "C:\Users\<USER-NAME>\.nuget\packages\pactnet.windows\3.0.2\tools\pact-win32\lib\ruby\lib\ruby\gems\2.2.0\gems\bundler-1.9.9\lib\bundler\vendor\thor\lib\thor\actions\empty_directory.rb" to "bin\Debug\netcoreapp3.1\pact-win32\lib\ruby\lib\ruby\gems\2.2.0\gems\bundler-1.9.9\lib\bundler\vendor\thor\lib\thor\actions\empty_directory.rb". Beginning retry 1 in 1000ms. Could not find a part of the path 'bin\Debug\netcoreapp3.1\pact-win32\lib\ruby\lib\ruby\gems\2.2.0\gems\bundler-1.9.9\lib\bundler\vendor\thor\lib\thor\actions\empty_directory.rb'.
```

This is because packages required by the PactBroker client libraries contain deep folder trees and when copied to the build output folder may
then have filenames longer than that traditionally supported by Windows.

This can be fixed by enabling long paths in the Windows registry using [these instructions](https://docs.microsoft.com/en-gb/windows/win32/fileio/maximum-file-path-limitation?tabs=cmd#enable-long-paths-in-windows-10-version-1607-and-later).
Once enabled it will take effect _without_ needing to restart Visual Studio.

## Using the packages

For full details on how to use the package(s) within this repository please read
[this wiki page](<https://github.com/LBHackney-IT/lbh-core/wiki/Using-the-package(s)-from-the-Hackney.Core-repository>).

_Note: The Hackney.Core project has been split into individual packages and is now deprecated._
_In order to use our packages, import each Hackney.Core dependency required individually._

## Adding a new package

Please refer to [our documentation](https://docs.google.com/document/d/1aJzhNxmSq_D4porwSIa528xYwTghblJmky4decvQBlA/edit?usp=sharing) on creating NuGet packages. For this repository, create your project folder in the `Hackney.Core` folder and test folder in `Hackney.Core.Tests`. Use the [workflow template](.github/workflows-template/publish.yml) to create your own workflow file in the `.github/workflows` folder.

## Features

The following features are implemented within this package (Alphabetical Order):

- [Authorisation](/Hackney.Core/Hackney.Core.Authorization/README.md)
- [DynamoDb](#DynamoDb)
  - [Converters](#Converters)
  - [Health check](#DynamoDb-Health-check)
  - [Paged results](#Paged-results)
- [ElasticSearch](/Hackney.Core/Hackney.Core.ElasticSearch/README.md)
- [Enums](/Hackney.Core/Hackney.Core.Enums/README.md)
- [Health check helpers](#Health-check-helpers)
- [Http](/Hackney.Core/Hackney.Core.Http/README.md)
- [JWT](Hackney.Core/Hackney.Core.JWT/README.md)
  - [Token Factory](Hackney.Core.Hackney.Core.JWT/README.md#api-summary)
- [Logging](#Logging)
  - [Lambda logging](#Lambda-logging)
- [MVC Middleware](#MVC-Middleware)
  - [Correlation middleware](#correlation-middleware])
  - [Exception middleware](#Exception-middleware)
- [Sns](/Hackney.Core/Hackney.Core.Sns/README.md)
- Testing
  - [Hackney.Core.Testing.DynamoDb](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.DynamoDb/README.md)
  - [Hackney.Core.Testing.PactBroker](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.PactBroker/README.md)
  - [Hackney.Core.Testing.Shared](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/README.md)
  - [Hackney.Core.Testing.Sns](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Sns/README.md)
- [Validation](/Hackney.Core/Hackney.Core.Validation/README.md)
- [Validation.AspNet](/Hackney.Core/Hackney.Core.Validation.AspNet/ReadMe.md)

### MVC Middleware

**Project reference: `Hackney.Core.Middleware`**

There are a number of different middleware classes implemented here.

Please bear in mind that the order in which they are used within an application's `Startup.Configure()` method dictates the order in which they are executed when the application receives an HTTP request.

#### Correlation middleware

The correlation middleware can be used to ensure that all incoming HTTP requests contain a correlation id value.
If a request does not contain a guid HTTP header value with the key `x-correlation-id` then one is generated and added to the request headers.
The correlation id is also automatically added to the response headers.

_Correlation Id_ - This is a (guid) value used to uniquely identify each request.

##### Usage

```csharp
using Hackney.Core.Middleware.Correlation;

namespace SomeApi
{
    public class Startup
    {
        ...
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseCorrelationId();
            ...
        }
    }
}

```

#### Exception middleware

The exception middleware can be used to set up a custom exception handler that will be used in the event of any unhandled exception.
It will log the exception and then return a standard error response that looks like the following.

```json
{
  "message": "String '2020-09-0 17:33:44' was not recognized as a valid DateTime.",
  "traceId": "0HM8BQ8L7EAEO:00000001",
  "correlationId": "3fbf8755-eb41-4c03-be9f-d0ccae470e39",
  "statusCode": 500
}
```

| Property      | Description                                        |
| :------------ | :------------------------------------------------- |
| message       | The message property from the unhandled exception. |
| traceId       | The traceId from the original HttpRequest.         |
| correlationId | The correlationId for the request.                 |
| statusCode    | The HttpResponse status code.                      |

##### Usage

If required, the [correlation middleware](#correlation-middleware]) call should go before the exception handler to ensure that any error logged will also include the correlation id

```csharp
using Hackney.Core.Middleware.Exception;

namespace SomeApi
{
    public class Startup
    {
        ...
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseCustomExceptionHandler();
            ...
        }
    }
}

```

#### Logging scope middleware

The logging scope middleware sets up a logging scope for every incoming HTTP request.
This means that every log statement made within that scope (i.e. during the HTTP request processing)
will include an addition string that contains both the correlation id (and user email, if available
in the headers) of the caller.
This means that all other logging need not concern itself without having to add this data as it is already included.

###### Usage

When used in conjunction with the [correlation middleware](#correlation-middleware]), the call to
`UseLoggingScope()` should come _after_ the call to `UseCorrelationId()`.

```csharp
using Hackney.Core.Middleware.Logging;

namespace SomeApi
{
    public class Startup
    {
        ...
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseLoggingScope();
            ...
        }
    }
}

```


### Health Check Helpers

**Project reference: `Hackney.Core.HealthCheck`**

The default HTTP response from the Microsoft Health check framework is simply a headline `HealthStatus` value with the appropriate Http status code.

In order to provide more meaningful response information a custom response writer, the `HealthCheckResponseWriter.WriteResponse` static method,  
has been implemented to serialise the entire `HealthReport` as json.

The only differences between the framework `HealthReport` class and the serialised response are:

- Durations are given in milliseconds only
- Any exception object has been replaced with just the exception message.

#### Usage

```csharp
using Hackney.Core.DynamoDb.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SomeApi
{
    public class Startup
    {
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponse
                });
            });
            ...
        }
    }
}

```

### JWT

**Project reference: `Hackney.Core.JWT`**

#### Token Factory

See more details on:
[JWT](Hackney.Core/Hackney.Core.JWT/README.md)
[Token Factory](Hackney.Core/Hackney.Core.JWT/README.md#api-summary)

### Logging

**Project reference: `Hackney.Core.Logging`**

#### Lambda logging

There is a helper method that can be used durung application startup to configure the generic Microsoft `ILogger`
framework to log to the AWS Lambda logger. By making use of the standard Microsoft implementation, it will also
make use of the any [standard logging configuration](#https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#configure-logging)
in the appsettings.json configuration file.

The `ConfigureLambdaLogging()` extension method will set up logging so to use the Lambda logger
(as well as logging to debug output, and the console if the application is running in the development environment).

###### Usage

```csharp
using Hackney.Core.Logging;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.ConfigureLambdaLogging(Configuration);
            ...
        }
    }
}

```

#### Log call aspect

By making use of the [Aspect Injector](#https://github.com/pamidur/aspect-injector) library it is possible
to easily add method logging with a single line of code.
This method logging generates simple log statements for the start and end of the decorated method.
It does this by generating (at compile time) a proxy around the class and then calling a custom aspect
before and after a decorated method. It is this custom aspect that will perform the logging.

In this way it is possible to easily add method logging without polluting methods with code that would
have to be continually replicated.

**Note:**
Because the aspect proxy is generated at compile time, this **will** affect how unit tests are written.
Any unit tests that are on a class that uses the `[LogCall]` attribute (regardless of whether or not they are testing
a decorated method) must also ensure that the DI container used by the aspect is configured appropriately.

##### Usage

###### Setup

The first call adds the necessary DI container registrations and the second call ensures that the DI
container used to inject the `ILogger` into the custom aspect is the same one that is created in
the application startup.

```csharp

using Hackney.Core.Middleware.Logging;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddLogCallAspect();
            ...
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseLogCall();
            ...
        }
    }
}

```

###### Decorate a method

To add method logging to a method simply decorate the method with the `[LogCall]` attribute.

```csharp
using Hackney.Core.Logging;

namespace SomeApi
{
    public class SomeClass
    {
        // The default log level is Trace.
        [LogCall]
        public void SomeMethod()
        {
            ...
        }

        // It is possible specify the log level required on the attribute.
        [LogCall(LogLevel.Information)]
        public void SomeOtherMethod()
        {
            ...
        }
    }
}

```
