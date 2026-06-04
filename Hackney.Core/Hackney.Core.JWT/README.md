# Hackney.Core.JWT

## Purpose

Lightweight helpers for decoding, inspecting and identifying JWTs used across Hackney APIs and services.

This package is tailored for read-only JWT handling in application code and tests — it does not provide token issuing or validation against public keys. Its primary goals are:

- Decode token payloads into typed objects (`Decode<T>`).
- Provide a convenience mapping to a legacy `Token` domain object (`DecodeStandardToken`).
- Fast, dependency-light token-type identification (`IdentifyHackneyToken`).

The library favors defensive behaviour: malformed tokens and unsupported inputs log warnings and return safe values (`null` or `Unknown`) rather than throwing.

## Install

Install from NuGet (replace with actual package id/version):

```bash
dotnet add package Hackney.Core.JWT --version <version>
```

## Quick start

Add the factory into your DI and use the provided decode helpers:

```csharp
// Startup.cs
services.AddTokenFactory();

// usage
var factory = serviceProvider.GetRequiredService<ITokenFactory>();
var dto = factory.Decode<MyDto>(jwtString);
var domainToken = factory.DecodeStandardToken(jwtString);
var kind = factory.IdentifyHackneyToken(jwtString);
```

## API summary

The following methods are exposed via `ITokenFactory` (see the interface docs for authoritative behaviour):

- `T? Decode<T>(string jwtStr)` — Decode the JWT payload JSON into `T`. Returns `null` for malformed tokens or empty payloads.
- `Token? DecodeStandardToken(string jwtStr)` — Convenience wrapper that decodes a standard token payload into the project's legacy `Token` domain object.
- `HackneyTokenType IdentifyHackneyToken(string jwtStr)` — Fast heuristic to identify `User`, `MachineLegacy`, or `Unknown` tokens. Requires a three-segment (signed) JWT to return `User` or `MachineLegacy`.
- `Token? Create(IHeaderDictionary headerDictionary, string headerName = "Authorization")` — Legacy helper (marked `[Obsolete]`) which extracts authorization header and delegates to `DecodeStandardToken`. Being deprecated as it violates this package's single responsibility principle by tapping into Http abstractions.

## Behavior & compatibility notes

- Token formats: identification requires a three-part JWT: `header.payload.signature`. Two-part tokens (`header.payload`) or tokens with an empty signature segment (`header.payload.`) are considered unsigned and will be classified as `Unknown` by `IdentifyHackneyToken`.
- The `Decode*` methods accept tokens prefixed with `Bearer ` (case-insensitive) and will strip that prefix.
- An empty JSON payload (`{}`) is treated as non-meaningful — decoding returns `null` and a warning is logged.
- `IdentifyHackneyToken` only inspects the decoded payload claims (it does not validate signatures). It returns `MachineLegacy` when `consumerName` claim is present, `User` when `email` or `sub` look like a user, otherwise `Unknown`.

## Examples

Decode a typed payload:

```csharp
var payload = factory.Decode<Dictionary<string,object>>(jwt);
if (payload != null && payload.ContainsKey("email")) {
    // treat as user token
}
```

Decode the standard token presentation into legacy `Token` domain object:

```csharp
var token = factory.DecodeStandardToken(jwt);
if (token == null) {
    // handle invalid token
}
```

Identify token type cheaply:

```csharp
switch (factory.IdentifyHackneyToken(jwt))
{
    case HackneyTokenType.MachineLegacy:
        // legacy machine-to-machine token
        break;
    case HackneyTokenType.User:
        // user token
        break;
    default:
        // unknown/unsupported
        break;
}
```

## Testing helpers

The test project (`Hackney.Core.Tests`) contains `TokenTestsHelper` utilities used in unit tests to generate both signed and unsigned test tokens. The helper uses a test secret and produces base64url-encoded header.payload and an HMAC-SHA256 signature when requested — see tests for exact behaviour.

When writing tests that depend on `IdentifyHackneyToken`, prefer generating three-part signed tokens if you need the token to be recognised. Unsigned two-part tokens should be used to assert `Unknown` classification.

## Security and limitations

- This package does not perform cryptographic validation of tokens (no signature verification against public keys). It is intended for decoding and application-level light-weight classification only.
