# Hackney.Core.Testing.Shared

This package contains helpers and common functionality that can be employed when wrinting unti and BDDfy E2E tests.

* [End to end tests helpers](#End-to-end-tests-helpers)
  * [BaseApiFixture](#BaseApiFixture)
  * [BaseSteps](#BaseSteps)
* [LogCallAspectFixture](#LogCallAspectFixture)
* [MockLoggerExtensions](#MockLoggerExtensions)
* [ServiceCollectionExtensions](#ServiceCollectionExtensions)


## End to end tests helpers

### BaseApiFixture

The `BaseApiFixture` class is a base class for implementing a fixture representing an external Api. 
To provide end-to-end testing of functionality that makes use of an external Api then create a new fixtures class dervied from `BaseApiFixture`.
The stub Api can accept multiple requests and provides properties a number of properties to assist with the pre-test setup 
and post-test verification.

#### Available properties:

| Property | Description |
| --- | --- |
| ApiRoute | The base route that the stub Api is listening on. |
| ApiToken | The token that is expceted on all Http requests received by the stub Api. (Optional) |
| Requests | A list of all requests made to the stub during the test. |
| ResponseObject | A single response object that will be returned for each request. Used if the `Responses` property is empty. |
| Responses | A collection of response objects keyed by id. The route used by the request is examined for the id, and if the `Responses` collection has an entry for that id then it is returned. |
| CallsMade | The count of all calls received by the stub Api. |
| ReceivedCorrelationIds | The list of correlation ids from all requests received. |

**Note:** When using the `Responses` property it is assumed that the request route is in the format
`<some url route>/<the object id requested>`. (I.e. The standard format for a REST get-by-id endpoint.)

#### Usage
* Derive a class from `BaseApiFixture`
* Set the required route (and token if needed)
* Implement the required fixture methods to set up the necessary responses required by any tests.

```csharp
using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using SomeNamespace.Boundary.Response;
using System;

namespace SomeNamespace.Tests.E2E.Fixture
{
    public class PersonApiFixture : BaseApiFixture<SomeResponseObject>
    {
        private readonly Fixture _fixture = new Fixture();
        public static string TheApiRoute => "http://localhost:5678/api/v1/";
        public static string TheApiToken => "sdjkhfgsdkjfgsdjfgh";

        public SomeApiFixture()
            : base(TheApiRoute, TheApiToken)
        {
            // These config values will be needed by the code under test.
            Environment.SetEnvironmentVariable("TheApiUrl", TheApiRoute);
            Environment.SetEnvironmentVariable("TheApiToken", TheApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        public SomeResponseObject GivenTheObjectExists(Guid id)
        {
            ResponseObject = _fixture.Build<SomeResponseObject>()
                                      .With(x => x.Id, id)
                                      .Create();
            return ResponseObject;
        }

        public void GivenTheObjectDoesNotExist(Guid id)
        {
            // Nothing to do here
        }
    }
}
```

### BaseSteps

The `BaseSteps` class is a base class for implementing a BDDfy "steps" class.

## LogCallAspectFixture
This provides a pre-configured [xunit collection](https://xunit.net/docs/shared-context#collection-fixture) 
for use with testing any class that makes use of the [LogCall functionality](/README.md#Log-call-aspect) 
within the Hackney.Core.Logging package.
If a class under test has this attribute then unit tests will fail unless the appropriate pre-test configuration is done, 
and this is provided by this pre-built collection.
The reason tests will fail without it is because the AspectInjector framework (used by the `LogCall` attribute) 
operates at compile-time. 
This means that simply constructing an instance of a class that uses the attribute requires all of the 
supporting objects used by the LogCallAspect to also be set up.


#### Usage
```csharp
using Hackney.Core.Testing.Shared;
using Xunit;

namespace SomeDomain.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class SomeUseCaseTests
    {
        ...
    }
}

```

## MockLoggerExtensions
Because the 'Microsoft.Extensions.Logging.ILogger' interface makes extensive use of extension methods to 
actually do its job it is difficult to determine how to verify these log statements within unit tests.
There are 3 extension methods here to assist with this.

### VerifyAny
This method simply verifies that the mock logger object was called the number of times for the specified log type 
regardless of the log message.

### VerifyContains
This method simply verifies that the mock logger object was called the number of times for the specified log type 
and the logged message contains the supplied string somehwere within it.

### VerifyExact
This method simply verifies that the mock logger object was called the number of times for the specified log type 
with the exact log message supplied.

#### Usage
```csharp
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Logging
using Xunit;

namespace SomeDomain.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class SomeUseCaseTests
    {
        [Fact]
        public void OperationLogsVerifyAny()
        {
            var mockLogger = new Mock<ILogger>();
            var sut = new SomeUseCase(mockLogger.Object);
            sut.DoSomething();

            mockLogger.VerifyAny(LogLevel.Information, Times.Once());
        }

        [Fact]
        public void OperationLogsSomethingVerifyContains()
        {
            var mockLogger = new Mock<ILogger>();
            var sut = new SomeUseCase(mockLogger.Object);
            sut.DoSomething();

            mockLogger.VerifyContains(LogLevel.Information, " part of the log message", Times.Once());
        }

        [Fact]
        public void OperationLogsSomethingVerifyExact()
        {
            var mockLogger = new Mock<ILogger>();
            var sut = new SomeUseCase(mockLogger.Object);
            sut.DoSomething();

            mockLogger.VerifyExact(LogLevel.Information, "The complete part of the log message", Times.Once());
        }
    }
}
```

## ServiceCollectionExtensions
Due to the fact that DI registration using the `ServiceCollection` make extensive use of extension 
methods, it is difficult to write unit tests to validate if a particular DI registration has been made.

The `IsServiceRegistered` helper method makes this verification easy.

#### Usage
```csharp
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.DependencyInjection
using Xunit;

namespace SomeDomain.Tests.UseCase
{
    public class SomeRegistrationHelperClassTests
    {
        [Fact]
        public void OperationRegistersSuccessfully()
        {
            var services = new ServiceCollection();
            var sut = new SomeRegistrationHelperClass();
            sut.RegisterMyStuff(services);

            // These 2 lines do the same thing
            services.IsServiceRegistered<IMyInterface>("MyInterfaceClass");
            services.IsServiceRegistered<IMyInterface, MyInterfaceClass>();
        }
    }
}
```


