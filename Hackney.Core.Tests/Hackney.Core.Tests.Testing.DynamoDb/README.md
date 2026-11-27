# Hackney.Core.Tests.Testing.DynamoDb

This project contains unit tests for the `Hackney.Core.Testing.DynamoDb` package.

These are mocked unit tests; they do not require a local DynamoDB instance.

## Why these tests exist

`Hackney.Core.Testing.DynamoDb` is itself a test helper: consuming APIs import it into their test projects to register a fixture, create tables, seed data, and clean up. Downstream suites (for example notes-api’s `AwsMockWebApplicationFactory`) treat that surface as a stable contract.

A successful package build only proves types still compile. It does not prove that:

- `ConfigureDynamoDbFixture` still registers the expected singleton
- `EnsureTablesExist` still builds the same table/key/index request shape and still treats “table already exists” as success
- `SaveEntityAsync` still seeds via the context and still registers dispose-time delete

Regressions of that kind still compile, then break consumer test hosts first. These unit tests lock the public behavioural interface of the helper package so SDK and target-framework upgrades (for example DynamoDB AWS SDK v3 → v4, or .NET 10) cannot change that contract unnoticed.

## Classes under test

| Class | What the tests protect |
|---|---|
| `ServiceCollectionExtensions` | `ConfigureDynamoDbFixture`: null guard, singleton `IDynamoDbFixture` → `DynamoDbFixture` registration, idempotent `TryAdd`, fluent return, resolve with injected client/context |
| `DynamoDbFixture` | Constructor exposes `DynamoDb` / `DynamoDbContext`; `EnsureTablesExist` (hash/range keys, LSI/GSI attribute inference, multi-table, empty list, `ResourceInUseException` vs other errors); `SaveEntityAsync` + dispose cleanup (including conditional-check failures and double dispose) |
| `TableDef` | Not tested alone; exercised as the public input model to `EnsureTablesExist` |
| `IDynamoDbFixture` | Interface only; covered through DI resolution and `DynamoDbFixture` behaviour |

## Run the tests

From the repository root:

```bash
dotnet test Hackney.Core.Tests/Hackney.Core.Tests.Testing.DynamoDb/Hackney.Core.Tests.Testing.DynamoDb.csproj
```
