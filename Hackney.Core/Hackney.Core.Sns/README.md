# `Hackney.Core.Sns` NuGet Package

## Sns Gateway
The `SnsGateway` implementation of the `ISnsGateway` interface allows the easy publishing of an event message to an Sns topic.
The `ISnsGateway` interface is made available by using the `AddSnsGateway()` extension method during your application start-up.

## Shared Classes
- EntityEventSns - Model of the event message received by a function
- EventData - Contains the data changed in an event
- User - Contains information about a user triggering an event
- EventTypes - Names all events we are currently using

## SnsIntialisationExtensions

The `SnsIntialisationExtensions` class configures an application to use the Amazon Simple Notification Service (SNS). This will allow the application to interact with SNS Topics and subscriptions.

## Usage

```csharp
using Hackney.Core.Sns;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.ConfigureSns();
            ...
        }
    }
}

```

Within the `docker-compose.yml` you would need to set the Environment Variable 

```yml

services:
  some-api:
    image: some-api
    ...
    environment:
      - Sns_LocalMode=true
    ...

```

You would also need to set the Environment Variable in the `DockerFile`

```Dockerfile

ENV Sns_LocalMode='true'

```

In order to launch the application successfully you would need to setup the environment variable in the `launchSettings`

```json

  "profiles": {
    "some_api": {
    
      "environmentVariables": {
        "Sns_LocalMode": "true"
      }

    }
  }

```