# AuthorizeEndpointByGroups

This is the previous authorization filter. It restricts an action to users in specific Google groups. New endpoints should use `AllowAccessTo` from the [package README](README.md).

The filter reads one environment variable, named by the attribute, and compares it with the groups on a user token. A legacy machine token is allowed through, because the Lambda authorizer has already checked that caller against the endpoint.

## Instructions

1. **Configure `ITokenFactory`.** Register the token factory from `Hackney.Core.JWT`. `AddHackneyAuthorizationAsync` does this itself. An application that uses only this filter registers it directly:

```csharp
services.AddTokenFactory();
```

2. **Set the environment variable.** The value is a comma-separated list of groups. Do not put a space after a comma unless the space is part of the group name.

```shell
PATCH_ENDPOINT_GOOGLE_GROUPS="group1,group2,group3,group with whitespace,group4"
```

Each endpoint that needs a different set of groups needs its own variable. The variable has to be present locally and in the pipeline, through `serverless.yml` and `appsettings.json`.

3. **Apply the filter** to the action. The argument is the variable name, not the group list.

```csharp
[HttpPost]
[AuthorizeEndpointByGroups("ALLOWED_GOOGLE_GROUPS_POST")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    // Your code here
}
```

`CreateUser` then accepts callers whose token groups include a group from `ALLOWED_GOOGLE_GROUPS_POST`.

## Public types

`AuthorizeEndpointByGroups` is in `Hackney.Core.Authorization`. It is an MVC `TypeFilterAttribute`. The constructor argument is the environment variable name.

`EnvironmentVariableNullException` is in `Hackney.Core.Authorization.Exceptions`. The filter throws it when that variable is missing. The message is `Cannot resolve {variableName} environment variable.`

A missing or empty `Authorization` header, a user who is not in a permitted group, and an unknown token type end the action with `401` and an `UnauthorizedObjectResult`. That status is the filter's own result. It is not the 403 written by `AllowAccessTo`.

## Testing

1. Create a mock JWT for a user in some Google groups.
2. Set the environment variable to those same groups.
3. Change either the variable or the token so the groups no longer overlap, and confirm the action is unauthorized.
