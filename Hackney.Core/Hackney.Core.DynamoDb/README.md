### DynamoDb

**Project reference: `Hackney.Core.DynamoDb`**

#### Setting up DynamoDb support

There is a `ConfigureDynamoDB()` extension method provided to facilitate setting up an application to use AWS DynamoDb.
By calling it in the application startup, the following interfaces will be configured in the DI container:

- `IAmazonDynamoDB`
- `IDynamoDBContext`

By default it assumes there is an appropriate AWS profile configured where the application will run.
See [here](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html) for more details.
This means that, at the very least, your application must have a region specified in its appsettings.json:

```json
  "AWS": {
    "Region": "eu-west-2"
  }
```

###### Usage

```csharp
using Hackney.Core.DynamoDb;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.ConfigureDynamoDB();
            ...
        }
    }
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

##### DynamoDbBoolConverter

This should be used on `bool` properties.
The default AWS `bool` processing converts the bool value to an integer of 0 or 1, which may or may not be a problem in itself depending on your specific data requirements.
However if you need to filter your database query on a bool property, this attribute will need to be decorating it.
This will mean that the bool value will get serialsed into and out of the database properly (as "true" or "false") and it will also mean that the property can be used as needed in any filter.

###### Usage

```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbBoolConverter))]
    public bool IsActive { get; set; }
```

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
It does this because the native AWS functionality for nested objects is very simplistic and does not honour the `LowerCamelCaseProperties` value set on the root class.
By simply converting the entire sub-object using Json we bypass these limitations.
However this will mean that any DynamoDb converters set against properties on nested objects will not be used.

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

#### Paged results

The implementation of querying for paged results within DynamoDb is limited compared to other database technologies.
In essence if you query a table with a specific page size, if there are potentially more results beyond that page size limit
then a token will be returned along with the results. This token (it is a json object) indicates where in the full results list
the current set of results ended. If this token is supplied in the next query then the list of results will start from where the
last set finished.

##### PagedResult

This is a template class used to encapsulate the results of a paged query.

| Property          | Description                                                       |
| :---------------- | :---------------------------------------------------------------- |
| Results           | The list of result objects from the query.                        |
| PaginationDetails | A `PaginationDetails` object that may or may not contain a token. |

##### PaginationDetails

This class encapsulates the results of a paged query.
It contains static methods to encode or decode a token value as needed.

| Property  | Description                                                                                                |
| :-------- | :--------------------------------------------------------------------------------------------------------- |
| HasNext   | Boolen value indicating whether or not there is a next token.                                              |
| NextToken | The token returned by the call to the AWS SDK encoded as a Base64 string.</br>`null` if there is no token. |

##### GetPagedQueryResultsAsync

This is an extension method on the `IDynamoDBContext` interface that is used to make a paged query against the database
and returns a `PagedResult` object.

If there are no more results after the query has been made (regardless of the specified page size) then the
`PaginationDetails.NextToken` will be null.

###### Usage

```csharp
public async Task<PagedResult<NoteDb>> GetNotesByTargetIdAsync(GetNotesByTargetIdQuery query)

{
    _logger.LogDebug($"Querying DynamoDB for notes for TargetId {query.TargetId}");

    int pageSize = query.PageSize.HasValue ? query.PageSize.Value : MAX_RESULTS;
    var queryConfig = new QueryOperationConfig
    {
        IndexName = GETNOTESBYTARGETIDINDEX,
        BackwardSearch = true,
        ConsistentRead = true,
        Limit = pageSize,
        PaginationToken = PaginationDetails.DecodeToken(query.PaginationToken),
        Filter = new QueryFilter(TARGETID, QueryOperator.Equal, query.TargetId)
    };

    return await _dynamoDbContext.GetPagedQueryResultsAsync<NoteDb>(queryConfig).ConfigureAwait(false);
}
```

#### DynamoDb Health Check

There is a `DynamoDbHealthCheck` class implemented that uses the
[Microsoft Health check framework](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.2).
The check verifies that the required DynamoDb table is accessible by performing a `DescribeTable` call.

##### Usage

The template argument supplied to the `AddDynamoDbHealthCheck()` call is the name of a database model class that has the `DynamoDbTable`
attribute applied to it. The method uses this attribute to determine the table name to use to query the database.

```csharp
using Hackney.Core.DynamoDb.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddDynamoDbHealthCheck<NoteDb>();
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
