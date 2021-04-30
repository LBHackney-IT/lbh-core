# Hackney.Core NuGet Package
At Hackney, we have created the NuGet Package to prevent the duplication of common code when implementing our APIs.
Hence this NuGet package will store the common code that can then be used in the relevant projects. 

### CircleCI Pipeline - Versioning
At present the pipeline does not automatically update the package version number.

**This means that for the NuGet Push command to work when code is merged to the release branch 
you must change the version number in the .csproj file according to the type of change you are making.**

The new version number should use the following format.

    A specific version number is in the form Major.Minor.Patch[-Suffix], where the components have the following meanings:

    Major: Breaking changes
    Minor: New features, but backward compatible
    Patch: Backwards compatible bug fixes only
    Suffix (optional): a hyphen followed by a string denoting a pre-release version


## Features

The following features are implemented within this package.
* [MVC Middleware]
  * [Correlation middleware]


### MVC Middleware

There are a number of different middleware classes implemented here. 

Please bear in mind that the order in which they are used within an application's `Startup.Configure()` method dictates the order in which they are executed when the application receives an HTTP request.

#### Correlation middleware

The correlation middleware can be used to ensure that all incoming HTTP requests contain a correlation id value.
If a request does not contain a guid HTTP header value with the key `x-correlation-id` then one is generated and added to the request headers.
The correlation id is also automatically added to the response headers.

*Correlation Id* - This is a (guid) value used to uniquely identify each request.

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
            app.UseCorrelation();
            ...
        }
    }
}

```
