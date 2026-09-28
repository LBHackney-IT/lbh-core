# Hackney.Core.JWT

## Purpose

Lightweight helpers for decoding, inspecting and identifying JWTs used across Hackney APIs and services.

This package is tailored for read-only JWT handling in application code. It does not issue tokens or validate signatures against public keys. Its primary goals are:

- Identify supported Hackney token types (`IdentifyHackneyToken`).
- Decode user and Cognito machine-to-machine tokens into domain objects (`DecodeStandardToken`, `DecodeCognitoM2MToken`).
- Decode other JWT payloads into caller-provided types (`Decode<T>`).

The library favors defensive behaviour: malformed tokens and unsupported inputs log warnings and return safe values (`null` or `Unknown`) rather than throwing.

## Install and setup

Install the package from NuGet:

```bash
dotnet add package Hackney.Core.JWT --version <version>
```

Register the token factory during application startup:

```csharp
using Hackney.Core.JWT;

builder.Services.AddTokenFactory();
```

Inject `ITokenFactory` where token handling is required:

```csharp
public sealed class MyService
{
    private readonly ITokenFactory _tokenFactory;

    public MyService(ITokenFactory tokenFactory)
    {
        _tokenFactory = tokenFactory;
    }
}
```

`AddTokenFactory` registers the factory and logging services with the dependency injection container.

## Intended usage

Identify a supported Hackney token before choosing a decoder. `IdentifyHackneyToken` recognizes payload markers for user, legacy machine-to-machine and Cognito machine-to-machine tokens. It returns `Unknown` for unsupported or unrecognized payloads.

```csharp
switch (tokenFactory.IdentifyHackneyToken(jwtString))
{
    case HackneyTokenType.User:
        var userToken = tokenFactory.DecodeStandardToken(jwtString);
        break;

    case HackneyTokenType.MachineLegacy:
        var legacyMachineToken = tokenFactory.Decode<LegacyMachineToken>(jwtString);
        break;

    case HackneyTokenType.CognitoM2M:
        var cognitoMachineToken = tokenFactory.DecodeCognitoM2MToken(jwtString);
        break;

    case HackneyTokenType.Unknown:
    default:
        // Handle unsupported or unrecognized token types.
        break;
}
```

For JWT payloads that do not use one of these supported Hackney token formats, use `Decode<T>` with an application-defined payload type:

```csharp
var payload = tokenFactory.Decode<MyTokenPayload>(jwtString);
```

## Token handling

- `IdentifyHackneyToken` requires a three-part JWT with a non-empty signature segment to classify a token. This is a structural requirement only; it does not verify the signature cryptographically.
- Identification checks `consumerName` for `MachineLegacy`, `scope` for `CognitoM2M`, and `email` for `User`, in that order. Other payloads are `Unknown`.
- `DecodeStandardToken` maps the legacy and Cognito user token payloads to the `Token` domain model. Missing group claims produce an empty group list. The cognito token `customGroups` only get mapped when `groups` is not provided.
- `DecodeCognitoM2MToken` maps Cognito's `client_id` and space-separated `scope` claims to `CognitoM2MToken`. Each valid scope in the form `{apiGatewayId}/{endpointName}.{accessType}` becomes a `CognitoM2MAccessScope`; malformed entries are ignored.
- `Decode<T>` deserializes a JWT payload into the requested type. Use it for the legacy machine token or other payload formats not handled by a dedicated decoder.
- Decode methods accept an optional `Bearer ` prefix and return `null` for malformed tokens, deserialization failures, or an empty JSON object payload.
- `Create(IHeaderDictionary, ...)` is a deprecated HTTP-header convenience method. Extract the token string in application code, only then call a decoder.

## Security and limitations

This package decodes JWT payloads and performs lightweight structural classification only. It does not validate signatures, issuer, audience, expiry, or other security claims. Validate tokens through the appropriate authentication middleware or trusted authorizer before relying on their claims. This choice is a deliberate design decision as these checks are performed at a lambda authorizer level.
