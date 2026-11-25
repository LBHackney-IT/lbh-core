# Hackney.Core.Tests.Sns

This project contains unit tests for the `Hackney.Core.Sns` package. It covers publishing a message, the argument checks on `Publish` both when the call is awaited and when it is not, SNS client registration for local and AWS modes, and gateway dependency injection registration.

## Run the tests

From the repository root:

```bash
dotnet test Hackney.Core.Tests/Hackney.Core.Tests.Sns/Hackney.Core.Tests.Sns.csproj
```
