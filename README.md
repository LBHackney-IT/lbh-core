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
* [MVC Middleware](#MVC%20Middleware)
  * [Correlation middleware](#Correlation%20middleware])
  * [Exception middleware](#Exception%20middleware)
* [DynamoDb](#DynamoDb)
  * [Converters](#Converters)


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
            app.UseCorrelationId();
            ...
        }
    }
}

```

#### Exception middleware

The exception middleware can be used to set up a custom exception hanlder that will be used in the event of any unhandled exception.
It will log the exception and then return a standard error response that looks like the following.
```json
{
    "message": "String '2020-09-0 17:33:44' was not recognized as a valid DateTime.",
    "traceId": "0HM8BQ8L7EAEO:00000001",
    "correlationId": "3fbf8755-eb41-4c03-be9f-d0ccae470e39",
    "statusCode": 500
}
```

| Property       | Description |
| :---           | :---        |
| message        | The message property from the unhandled exception. |
| traceId        | The traceId from the original HttpRequest. |
| correlationId  | The correlationId for the request. |
| statusCode     | The HttpResponse status code. |

##### Usage

If required, the [Correlation Middleware] call should go before the exception handler to ensure that any error logged will also include the correlation id

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
            app.UseCustomExceptionHandler();
            ...
        }
    }
}

```

### DynamoDb

#### Setting up DynamoDb support

There is a `ConfigureDynamoDB()` extension method provided to facilitate setting up an application to use AWS DynamoDb.
By calling it in the application startup, the following interfaces will be configured in the DI container:
* `IAmazonDynamoDB`
* `IDynamoDBContext`

By default it assumes there is an appropriate AWS profile configured where the application will run. 
See [here](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html) for more details.
This means that, at the very least, your application must have a region specified in its appsettings.json:
```json
  "AWS": {
    "Region": "eu-west-2"
  }
```

##### Local mode
If there is a local DynamoDb instance available then this can be used by amending the application settings as shown below:
```json
  "AWS": {
    "Region": "DefaultRegion"
  },
  "DynamoDb": {
    "LocalMode": true,
    "LocalServiceUrl": "http://localhost:8000"
  }
```
The `LocalMode` flag must be set to `true` and the `LocalServiceUrl` should point to the local DynamoDb instance. 
These 2 values can also be set as environment variables.


#### Converters

There are a number of different converters provided to make using the `IDynamoDBContext` easier.
They are used against a property on a model class to tell the underlying AWS libraries how to marshal the property into and out of the database.

##### DynamoDbDateTimeConverter

This should be used on `DateTime` properties.
The default AWS `DateTime` processing is very restrictive, expects that value to be in a very specific format and will throw an exception if it is not in that format.
This converter will always store in the format `yyyy-MM-ddTHH:mm:ss.fffffffZ`, but is much more forgiving when reading data out of the database.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
    public DateTime Created { get; set; }
```

##### DynamoDbEnumConverter

This should be used on `enum` properties.
It ensures that an enum value is stored in the database as the string representation of the value rather than the numeric value.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbEnumConverter<TargetType>))]
    public TargetType TargetType { get; set; }
```

##### DynamoDbEnumListConverter

This should be used on properties that are a `List<>` of `enum` values.
Internally it operates in the same way as the `DynamoDbEnumConverter`.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbEnumListConverter<PersonType>))]
    public List<PersonType> PersonTypes { get; set; } = new List<PersonType>();
```

##### DynamoDbObjectConverter

This should be used on properties that are a custom object.
The coverter works by simply serialising the object to and from the database using Json.
It does this because the native ASWS for nested objects is very simplistic and does not honour the `LowerCamelCaseProperties` value set on the root class.
By simply converting the entire sub-object using Json we bypass these limitations.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbObjectConverter<AuthorDetails>))]
    public AuthorDetails Author { get; set; }
```

##### DynamoDbObjectListConverter

This should be used on properties that are a lists of custom objects.
Internally it operates in the same way as the `DynamoDbObjectConverter`.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbEnumListConverter<PersonType>))]
    public List<PersonType> PersonTypes { get; set; } = new List<PersonType>();
```