# Hackney.Core.Authorization

`Hackney.Core.Authorization` decides whether a request that has already reached an API may call the endpoint it is aimed at. A request is denied unless the endpoint, or its controller, says who may call it.

The package covers three callers:

- A **user** token is allowed when one of its groups is listed for the policy named on the endpoint.
- A **Cognito machine** token is allowed when one of its scopes names the endpoint.
- A **legacy machine** token is allowed only on endpoints that opt in with `LegacyMachine = true`.

Anything else is denied with **403 Forbidden**. That includes an endpoint nobody decorated, a token of the wrong kind, and a token that does not match the rule.

## Assumptions

The package is built for Hackney APIs that run behind the Lambda authorizer.

- The authorizer has already authenticated the caller. This package does not check signatures, expiry, or issuer.
- The authorizer has already done the authorization checks it is able to do. For user tokens, the API target was checked. For a Cognito machine token that includes the API name and the HTTP verb. For a legacy machine token that includes the endpoint itself. This package does the check the authorizer cannot finish.
- The bearer token is still on the `Authorization` header when the request enters the API. The package reads that header. It does not read `HttpContext.User`.
- User groups are published as one JSON document in AWS Systems Manager Parameter Store. `AddHackneyAuthorization` registers a hosted service that reads that document once, when the host starts, before the server accepts a request. A later change to the parameter is picked up when a new process starts.
- The host calls `app.UseAuthorization()`. Registration only puts the services in the container. It cannot insert the middleware into the pipeline.
- This middleware needs to be the only Authorization middleware used by the consuming application. It's meant to handle all traffic.
- The group document is a JSON object. Each key is a policy name. Each value is the list of group names that satisfy that policy. Policy names are compared without case, and two keys that differ only by case are rejected. Group names are compared exactly.

```json
{
  "housing-admins": ["housing-officers", "housing-managers"],
  "wildlife-keepers": ["mammal-surveyors"]
}
```

A Cognito scope has the form `{api}/{endpoint}.{access}`, for example `wildlife-api/barbastellus-bat.read`. Only the endpoint segment is compared with `Machine`, and the comparison ignores case.

## How to use it

Register the package, then turn authorization on. Pass the name of the SSM parameter. Pass your own SSM client when the application has its own constructed and you do not wish for this Nuget package to construct its own client instance.

```csharp
builder.Services.AddHackneyAuthorization("/auth/groups");

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### A user, a Cognito machine, or both

`User` is the policy name in the group document. `Machine` is the endpoint segment of a Cognito scope.

```csharp
[AllowAccessTo(User = "housing-admins", Machine = "create-user")]
[HttpPost]
public IActionResult CreateUser() => Ok();
```

A user token is allowed when one of its groups is in `housing-admins`. A Cognito token is allowed when a scope names `create-user`. A token of one kind is not rescued by a rule for the other kind.

### A legacy machine

Set `LegacyMachine = true`. Leaving it out means `false`, and a legacy token is denied even though the endpoint allows users or Cognito machines.

```csharp
[AllowAccessTo(Machine = "create-user", LegacyMachine = true)]
```

The package then checks that the token can be read. It does not repeat the endpoint check the authorizer already made. The flag is the record, on the endpoint, that a legacy consumer is expected there.

### A whole controller

A controller attribute applies to every action that does not have its own `AllowAccessTo`.

```csharp
[AllowAccessTo(User = "housing-admins")]
public class ResidentsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok();

    [AllowAccessTo(Machine = "create-resident")]
    [HttpPost]
    public IActionResult Create() => Ok();
}
```

`List` uses `housing-admins`, because it has no attribute of its own. `Create` uses only `create-resident`. The action attribute replaces the controller attribute. It does not keep `User` from the controller.

An attribute on one action does not open the other actions.

### An endpoint with no rule

An action with no `AllowAccessTo`, on a controller that also has none, is denied. The route is not itself a rule.

`[AllowAccessTo]` with no `User`, no `Machine`, and no `LegacyMachine = true` also denies every caller. The attribute is present, so this is not the fallback rule. Each token is denied because its own rule is missing.

To leave one endpoint public, mark that action `[AllowAnonymous]`. That skips the fallback denial and any `AllowAccessTo` inherited from the controller.

`[AllowAnonymous]` on a controller makes every action on that controller public. An `[AllowAccessTo]` on one of those actions does not close it. ASP.NET Core skips authorization for the whole endpoint when it finds `[AllowAnonymous]` anywhere on it, including on the controller.

## Public interface

### `AllowAccessToAttribute`

Namespace `Hackney.Core.Authorization`. Apply it to a controller or an action. `AllowMultiple` is `false`.

| Member          | Meaning                                                                                   |
| --------------- | ----------------------------------------------------------------------------------------- |
| `PolicyName`    | `"HybridAccessPolicy"`. A const. The policy the attribute selects.                        |
| `Policy`        | Always `PolicyName`. There is no setter, so a caller cannot select another policy.        |
| `User`          | Policy name in the group document. Null means user tokens are denied.                     |
| `Machine`       | Endpoint name a Cognito scope must contain. Null means Cognito machine tokens are denied. |
| `LegacyMachine` | `false` unless set to `true`. Gates legacy machine tokens.                                |

### `AddHackneyAuthorization`

```csharp
public static IServiceCollection AddHackneyAuthorization(
    this IServiceCollection services,
    string ssmParameterName,
    IAmazonSimpleSystemsManagement? ssmClient = null)
```

Registers the token factory, an empty group map, the hosted service that reads the group document when the host starts, the authorization handler, the always-deny fallback policy, the `HybridAccessPolicy`, and the result handler that writes 403. `ArgumentNullException` when the parameter name is null, and `ArgumentException` when it is empty or whitespace, are thrown by this call. A second call returns without registering again. The parameter name from the first call is the one that is read. The fetch failure in the table is thrown when the host starts. A bad group document stops the host from starting as well.

| Exception                       | When                                                                                                                                                                                                                                                                                                                                              |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ArgumentNullException`         | `ssmParameterName` is null. `ParamName` is `ssmParameterName`.                                                                                                                                                                                                                                                                                    |
| `ArgumentException`             | `ssmParameterName` is empty or whitespace. `ParamName` is `ssmParameterName`.                                                                                                                                                                                                                                                                     |
| `InvalidOperationException`     | The parameter cannot be fetched when the host starts. The SSM error is the inner exception and the message names the parameter.                                                                                                                                                                                                                  |
| Bad group document              | The host does not start. The message names the parameter. JSON that is not an object, a JSON `null`, a policy whose group list is null, and two policy keys that differ only by case are rejected.                                                                                                                                              |
| AWS SDK exception               | You passed no client, and constructing the default SSM client fails, for example because no region is configured. That exception is not wrapped.                                                                                                                                                                                                  |

`{}` is a valid document. It publishes no policies, so every user token is denied.

### Legacy types

`AuthorizeEndpointByGroups` and `EnvironmentVariableNullException` remain public for the previous filter. Their use is described in [LEGACY.md](LEGACY.md). New endpoints should use `AllowAccessTo`.

## What this package does not do

- It does not authenticate. A missing or rejected token is still **403**, because the caller was already admitted by the authorizer and then refused by a policy. It is not **401**.
- It does not validate the token signature, and it does not set `HttpContext.User`. Code that reads `User.Identity` or `User.Claims` finds the anonymous user the pipeline started with.
- It does not refresh the group document after startup.
- It does not merge a controller attribute with an action attribute.
- It does not check the Cognito API name or the HTTP verb. The authorizer does that.
- It does not treat a legacy token as allowed on every decorated endpoint. The endpoint must set `LegacyMachine = true`.

## Previous filter

The environment-variable filter `[AuthorizeEndpointByGroups]` is the previous way to restrict an action to Google groups. Those instructions are in [LEGACY.md](LEGACY.md).
