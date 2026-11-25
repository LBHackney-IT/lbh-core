# Hackney.Core.Sns

`Hackney.Core.Sns` publishes an event message to an AWS SNS topic, and it shares the message shape Hackney APIs send and listeners read.

The package does two jobs:

- `AddSnsGatewayAndClient` registers the SNS client and `ISnsGateway`. The client uses AWS when `Sns_LocalMode` is not true, and a Localstack URL when it is.
- `Publish` checks the message and the topic, serializes the message, and sends it.

A null message, or a null or empty topic, is rejected with `ArgumentNullException` at the call. The send itself is asynchronous.

## Assumptions

The package is built for Hackney APIs on .NET 10 that already run in AWS, or against Localstack in development.

- The host has an AWS region available when it is not in local mode. The client is the parameterless `AmazonSimpleNotificationServiceClient`, so the region comes from the SDK's usual configuration, such as `AWS_REGION`.
- Local mode is explicit. `Sns_LocalMode` must parse as `true`. `false`, an empty value, and a missing variable all mean AWS.
- In local mode the topic endpoint is `Localstack_SnsServiceUrl`. `AddSnsGatewayAndClient` still returns if that variable is missing. The first publish then fails inside the SDK, because the client has no region and no service URL.
- Registration uses `TryAdd`. A client or gateway already in the container is left as it is. A second call does not replace it.
- The message is JSON. Property names are camel case, enums are strings, and the JSON is indented. A listener that deserializes `EntityEventSns` with the same options reads the same shape.
- Every publish sets `MessageGroupId`. When the caller omits it, the value is `"fake"`. A caller that passes `null` sends `null`. FIFO topics use this field to group messages.

## How to use it

Register the client and the gateway during startup.

```csharp
using Hackney.Core.Sns;

builder.Services.AddSnsGatewayAndClient();
```

For Localstack, set both variables. `true` is case-insensitive. Any other value selects AWS.

```text
Sns_LocalMode=true
Localstack_SnsServiceUrl=http://localhost:4566
```

Inject `ISnsGateway` and publish. `await` is how callers wait for the send. The argument checks throw whether or not the call is awaited.

```csharp
public sealed class TenurePublisher
{
    private readonly ISnsGateway _sns;

    public TenurePublisher(ISnsGateway sns)
    {
        _sns = sns;
    }

    public Task PublishCreated(EntityEventSns tenureEvent, string topicArn)
    {
        return _sns.Publish(tenureEvent, topicArn);
    }
}
```

Pass a message group id when the topic is FIFO and the group matters.

```csharp
await _sns.Publish(tenureEvent, topicArn, tenureEvent.EntityId.ToString());
```

## Public interface

### `AddSnsGatewayAndClient`

```csharp
public static IServiceCollection AddSnsGatewayAndClient(this IServiceCollection services)
```

Registers `IAmazonSimpleNotificationService` and a scoped `ISnsGateway`.

| Mode | Lifetime | Client |
| --- | --- | --- |
| `Sns_LocalMode` parses as `true` | Singleton | `ServiceURL` is `Localstack_SnsServiceUrl` |
| Otherwise | Scoped | The SDK resolves the region itself |

`ArgumentNullException` when `services` is null. `ParamName` is `services`.

### `ISnsGateway.Publish`

```csharp
Task Publish<T>(T snsMessage, string topicArn, string messageGroupId = "fake") where T : class
```

Serializes `snsMessage` and calls SNS `Publish`.

| Exception | When |
| --- | --- |
| `ArgumentNullException` | `snsMessage` is null. `ParamName` is `snsMessage`. Thrown at the call, before a task is returned. |
| `ArgumentNullException` | `topicArn` is null or empty. `ParamName` is `topicArn`. Thrown at the call, before a task is returned. |

An empty topic throws `ArgumentNullException`, not `ArgumentException`.

A publish that gets past those checks returns the SNS task. Failures from the SDK, including a missing region or a rejected request, come from that task.

### `EntityEventSns`

The message shape shared by publishers and listeners. `EventData` starts as an empty object. `OldData` and `NewData` are `object`, so the JSON keeps whatever the publisher put there.

| Member | Meaning |
| --- | --- |
| `Id` | Event id. |
| `EventType` | Name of the event. `EventTypes` holds the names this estate uses. |
| `SourceDomain` | Domain that raised the event. |
| `SourceSystem` | System that raised the event. |
| `Version` | Message version. |
| `CorrelationId` | Id that ties the event to the request that caused it. |
| `DateTime` | When the event happened. |
| `User` | `Name` and `Email` of the person who caused it. |
| `EntityId` | Id of the entity the event is about. |
| `EventData` | `OldData` and `NewData`. `OldData` is left unset when the event is a create. |

### `EventTypes`

Constants for the event names currently published: person created and updated, person added to and removed from a tenure, contact detail added and deleted, tenure created and updated.

## What this package does not do

- It does not create topics, subscriptions, or queues.
- It does not choose the topic. The caller passes the topic ARN.
- It does not check the fields inside the message. A publisher can leave `EventType` or `NewData` empty, and the gateway will still send the JSON.
- It does not retry beyond the AWS SDK default. That default is standard mode, which retries twice.
- It does not replace an SNS client or a gateway that the application has already registered.
