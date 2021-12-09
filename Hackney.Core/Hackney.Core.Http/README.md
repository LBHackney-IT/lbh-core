# Hackney.Core.Http

This package contains helpers and common funcitonality that can be employed when using Http requests and responses.

* [IHttpContextWrapper](#IHttpContextWrapper)


## IHttpContextWrapper

The `IHttpContextWrapper` interface provdes a helper method to easily get at the headres dictionary of the current Http request.
It can be registered within the DI container inthe applicaiton startup using the `IServiceCollection` extension method `AddHttpContextWrapper()`.
