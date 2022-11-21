# Hackney.Core.Authorization NuGet Package
This library provides functionality to validate that a request's JWT token contains the correct Google Group(s) to allow access to an API or endpoint.

## Usage
### Managing access to the entire API

To use this middleware you need to add the next line in your `Configure(...)` methon in `Startup.cs` file:
    
	    app.UseGoogleGroupAuthorization();
	
This middleware will use `REQUIRED_GOOGL_GROUPS` environment valiable to get required Google groups list

### Managing access to specific endpoints

1) Make sure your API has registered the `ITokenFactory` service from `Hackney.Core.JWT`.

This is normally configured in `Startup.cs`, with the line: 
```csharp 
services.AddTokenFactory();
```

2) Set up an environment variable to contain the list of groups that have access to an endpoint. **This can be called anything you want**. The list of groups should be comma separated, with no whitespaces (unless the group name has whitespaces in it).

For example:
```shell
$ $PATCH_ENDPOINT_GOOGLE_GROUPS="group1,group2,group3,group with whitespace,group4"
```
You can have as many environment variables as you want. For example, if you want a different list of groups to have access to the PATCH endpoint than the POST endpoint, you can create separate environment variables for each list of groups.

_Note: Make sure you configure this environment variable to be available locally & in your pipeline, through your `serverless.yml` and `appsettings.json`._

3) Add the authorization filter to your endpoint(s)

Add the following line to your controller methods:
```csharp
[AuthorizeEndpointByGroups("<Environment Variable Name>")]
```

For example:
```csharp
/// <summary>
/// Create a new user
/// </summary>
/// <response code="201">NoContent</response>
[ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
[HttpPost]
[AuthorizeByGroups("ALLOWED_GOOGLE_GROUPS")]
[LogCall(LogLevel.Information)]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
	var user = await _createUserUsecase.Execute(request).ConfigureAwait(false);
    return Created(new Uri($"api/v1/users/{user.Id}", UriKind.Relative), technology.Id);
}
```

Write your E2E tests and you're done! The API will return an appropriate 400 error if the user cannot access the specified endpoint.