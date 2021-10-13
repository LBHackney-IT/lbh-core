# Hackney.Core.Testing

This collection of packages includes test helper methods and classes that are used to assist in writing both unit and E2E tests. 

There are 3 packages:
* [Hackney.Core.Testing.Shared](#Hackney.Core.Testing.Shared) - Shared test helpers that can be used in various projects
* [Hackney.Core.Testing.DynamoDb](#Hackney.Core.Testing.DynamoDb) - Test helpers designed for projects that utilise AWS DynamoDb
* [Hackney.Core.Testing.ElasticSearch](#Hackney.Core.Testing.ElasticSearch) - Test helpers designed for projects that utilise ElasticSearch

## Hackney.Core.Testing.Shared

### LogCallAspectFixture

This fixture & collection should be used for on all test classes that are testing classes that use the LogCall attribute. This is required because the AspectInjector framework operates at compile-time. This means that simply constructing an instance of a class that uses the attribute requires all of the supporting objects used by the LogCallAspect to also be set up.

#### Usage
TODO

### MockLoggerExtensions
Helpers for verifying logs / logging mechanisms

#### VerifyExact
Verifies if the exact log message was logged for the stated level, a specific number of times.

##### Usage
*Parameters*
- `T`: (TypeParamater) The class type for which the logger is dedicated
- `logger`: The mock logger object
- `level`: log level
- `msg`: The log message
- `times`: The expected number of times the mock was called.

#### VerifyContains
Verifies if the stated string appears anywhere in a logged log message for the stated level, a specific number of times.

##### Usage
- `T`: The class type for which the logger is dedicated
- `logger`: The mock logger object
- `level`: The log level
- `msg`: The log message
- `times`: The expected number of times the mock was called.

#### VerifyAny
Verifies if *any* log messages were logged for the stated level, a specific number of times.

##### Usage
- `T`: The class type for which the logger is dedicated
- `logger`: The mock logger object
- `level`: The log level
- `times`: The expected number of times the mock was called.

### ServiceCollectionExtensions
TODO
#### Usage
TODO

### BaseApiFixture

This mocks an external API endpoint, good for isolating tests if your project relies on another API.

#### Usage

Create your API fixture by inheriting from the `BaseApiFixture` class. Pass the response object returned from the API as a parameter.

```csharp
using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;

namespace SomeListener.Tests.E2ETests.Fixtures
{
    public class SomeApiFixture : BaseApiFixture<SomeResponseObject>
    {
        private readonly Fixture _fixture = new Fixture();
        public static string AccountApiRoute => "http://localhost:5678/api/v1/";
        public static string AccountApiToken => "sdjkhfgsdkjfgsdjfgh";

        public SomeApiFixture()
            : base(AccountApiRoute, AccountApiToken)
        {
            Environment.SetEnvironmentVariable("AccountApiUrl", ApiRoute);
            Environment.SetEnvironmentVariable("AccountApiToken", ApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        private SomeResponseObject ConstructSomeResponseObject(Guid id)
        {
            return _fixture.Build<SomeResponseObject>()
                                                 .With(x => x.Id, id)
                                                 .Create();
        }

        public void GivenSomeObjectDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        // Add more usecases here
    }
}
```

### BaseSteps
TODO
#### Usage
TODO

## Hackney.Core.Testing.DynamoDb

Not yet implemented!

## Hackney.Core.Testing.ElasticSearch 

Not yet implemented!