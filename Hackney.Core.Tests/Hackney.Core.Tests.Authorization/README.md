# Hackney.Core.Tests.Authorization

Tests for [Hackney.Core.Authorization](../../Hackney.Core/Hackney.Core.Authorization/README.md). The project is an xUnit v3 executable that references the authorization package and can see its internal types.

Run them from this directory:

```shell
dotnet test
```

## Unit tests

These call the package directly. They do not start a host.

| Area | What it locks |
| --- | --- |
| `ServiceCollectionExtensionsTests` | Parameter name, the SSM fetch, the group map, service registration, and the fallback policy denying both an anonymous and an authenticated caller. |
| `AllowAccessToAttributeTests` | The policy name, the default `LegacyMachine` value, and controller versus action metadata. |
| `Internal/HybridAccessHandlerTests` | Each token kind: a matching user group, a Cognito scope, a legacy token with the flag set, and the denials when the rule does not match. |
| `Internal/ForbiddenAuthorizationResultHandlerTests` | An allowed request continues along the pipeline. A challenge and a forbid both stop at 403. |
| `Internal/PolicyGroupMapMapperTests` and `PolicyGroupMapTests` | The group document: case of policy keys, exact group names, and documents that cannot be read. |
| `AuthoriseEndpointsByGroupsTests` | The legacy filter, including a missing environment variable. |

## End-to-end tests

The `E2E` folder sends real HTTP requests through the authorization middleware. The host is in memory. There is no socket, no SSM network call, and no login handler. Denied requests are 403 because the package's result handler writes that status.

Only the test classes sit in `E2E`. The mock API and the helpers are beside them.

### `E2E/WildlifeApi`

A mock API used only as a target. It has no business behaviour.

- `WildlifeApiFixture` builds one `WebApplication` for the collection, points it at a fake SSM document, calls `UseAuthorization`, and exposes an `HttpClient`.
- `WildlifeApiCollection` shares that host across the end-to-end tests.
- `AnimalsController` is the token-flow target. `GET animals/barbastellus-bat` allows the Cognito endpoint `barbastellus-bat`, the user policy `wildlife-keepers` (group `mammal-surveyors`), and legacy machine tokens.
- `BotanicalController` is decorated, so its undecorated action inherits that rule and its decorated action replaces it.
- `AquaticController` is not decorated. One action has `AllowAccessTo`. The other has nothing.

The group document is `{"wildlife-keepers":["mammal-surveyors"]}`.

### `E2E/Helpers`

`TestJwt` builds unsigned bearer tokens the package can classify. `TestHttp` sends them and reads the body, passing xUnit's cancellation token. `WildlifePaths` builds the mock routes. `FakeSsm` returns the group document without calling AWS.

### Suites

| Class | What it locks |
| --- | --- |
| `CognitoM2MAuthorizationTests` | A matching Cognito scope is allowed. A different endpoint name is 403. |
| `UserAuthorizationTests` | A group in `wildlife-keepers` is allowed. Another group is 403. |
| `LegacyMachineAuthorizationTests` | A readable legacy token is allowed on the barbastelle endpoint. An unreadable one is 403. The same readable token is 403 on an endpoint that did not set `LegacyMachine = true`. |
| `AttributePlacementAuthorizationTests` | An undecorated action inherits the controller attribute. An action attribute replaces it. An action attribute does not open another action. No attribute on either is 403. |

These tests do not prove signature checks, a real SSM read, or a Lambda authorizer. Those happen before a request reaches this package.
