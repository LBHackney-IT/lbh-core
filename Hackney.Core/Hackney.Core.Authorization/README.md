# Hackney.Core.Authorization NuGet Package

The `Hackney.Core.Authorization` library restricts API endpoint access to users in specific Google groups.

## AuthorizeEndpointByGroups

This feature enables developers to restrict endpoint access to users within specific Google groups.

### Instructions

1. **Configure `ITokenFactory`:** Ensure that your API has registered the `ITokenFactory` service from `Hackney.Core.JWT`. Typically, this is done in the `Startup.cs` file by adding the following code:
	```csharp
	services.AddTokenFactory();
	```

2. **Set Environment Variable:** Define an environment variable containing the list of groups with access to an endpoint. The groups should be comma-separated and have no whitespaces (unless the group name has them)
	```shell
	$PATCH_ENDPOINT_GOOGLE_GROUPS="group1,group2,group3,group with whitespace,group4"
	```

	You can have multiple environment variables if you want different groups to have access to different endpoints.
	_Note: Make sure you configure this environment variable to be available locally and in your pipeline, through your `serverless.yml` and `appsettings.json` files._
3. **Apply Authorization Filter:** In your controller methods, add the `[AuthorizeEndpointByGroups("<Environment Variable Name>")]` attribute.
	```csharp
	[HttpPost]
	[AuthorizeEndpointByGroups("ALLOWED_GOOGLE_GROUPS_POST")]
	public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
	{
		 // Your code here
	}
	```

	This means that the `CreateUser` endpoint will be limited to the Google groups listed in the `$ALLOWED_GOOGLE_GROUPS_POST` environment variable.
### Testing

To test authorization filters:

1. Create a mock JWT token for a fake user with fake Google groups.
2. Set the environment variable to include the same Google groups as those in the JWT token.
3. Modify either the environment variable or the JWT token to include a Google group that doesn't match the other.
