using System.Collections.Generic;

namespace Hackney.Core.JWT;

/// <summary>
/// A Domain layer model representing a Cognito M2M token schema Hackney
/// APIs rely on. This model does include any other JWT claim fields as
/// those are validated by the lambda authorizer and do not get used in
/// any way within individual APIs.
/// </summary>
public class CognitoM2MToken
{
    /// <summary>
    /// ClientId is effectively a consumer identifier. Our authorization model
    /// expects 1 Cognito App Client per machine consumer. As such, ClientId
    /// can be used as a pretty accurate identifier of who the token belongs to.
    /// It's almost an equivalent to the token id for the legacy M2M solution.
    /// </summary>
    public required string ClientId { get; set; }
    public required List<CognitoM2MAccessScope> Scopes { get; set; }
}

/// <summary>
/// A Domain layer model representing a single API access scope granted to
/// a Cognito machine-to-machine client in an object form so a claim is
/// easy to evaluate.
/// </summary>
public class CognitoM2MAccessScope
{
    /// <summary>
    /// This authorization option assumes that APIs protected by it are exposed
    /// via AWS API Gateway proxy. As such, a 1:1 mapping between an API Gateway
    /// Id and an actual API application is expected.
    /// </summary>
    public required string ApiGatewayId { get; set; }
    /// <summary>
    /// EndpointName represents a name for an endpoint that will be used in
    /// a C# controller method's authorization attribute to allow/deny access.
    /// 
    /// Note! Do not confuse this with the 'endpointName' column from the
    /// Legacy M2M token database, which is actually a poorly-named endpoint
    /// path representation. This endpoint name should be a human-friendly
    /// name that represents that path uniquelly enough in the context of a
    /// single API. For example, instead of it being: "tenures/:id/persons/:id"
    /// it can simply be named: "tenurePerson". 
    /// </summary>
    public required string EndpointName { get; set; }
    /// <summary>
    /// AccessType represents either an http verb (GET, POST, ..., DELETE), or a
    /// generic access type like: 'read', 'write' (only 2 options).
    /// </summary>
    public required string AccessType { get; set; }
}
