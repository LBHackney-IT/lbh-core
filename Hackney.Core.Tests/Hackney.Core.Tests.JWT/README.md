# Hackney.Core.Tests.JWT

This project contains unit tests for the `Hackney.Core.JWT` package. It covers JWT payload decoding, token type identification, presentation-to-domain mapping, malformed input handling, and dependency injection registration.

## Test token helpers

`TokenTestsHelper` creates test JWTs for the test suite. It can generate signed and unsigned payloads using a test-only secret. Signed tokens are useful when testing `IdentifyHackneyToken`, which requires a three-part token with a non-empty signature segment to classify known token types. Unsigned tokens are used to verify that identification returns `Unknown`.

The helper and its secret are for tests only. Do not use them to issue application tokens or validate production JWTs.

## Run the tests

From the repository root:

```bash
dotnet test Hackney.Core.Tests/Hackney.Core.Tests.JWT/Hackney.Core.Tests.JWT.csproj
```
