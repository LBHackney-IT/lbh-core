using Microsoft.AspNetCore.Authorization;
using System;

namespace Hackney.Core.Authorization
{
    /// <summary>
    /// Declares who may call an endpoint. Applies to a controller or an action; an action's attribute replaces the controller's.
    /// <c>[AllowAnonymous]</c> on the same endpoint, or inherited from the controller, takes precedence and skips this rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AllowAccessToAttribute : Attribute, IAuthorizeData
    {
        public const string PolicyName = "HybridAccessPolicy";

        /// <summary>
        /// Policy name in the groups document from Parameter Store. A user token is allowed when one of its groups is listed
        /// for this policy. Omitted means user tokens are denied.
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Endpoint name (as defined in resource server definition) that a Cognito scope must contain. A machine token is
        /// allowed when one of its scopes names this endpoint. Omitted means Cognito machine tokens are denied.
        /// </summary>
        public string? Machine { get; set; }

        /// <summary>
        /// When <see langword="true"/>, a legacy machine token may call this endpoint.
        /// Omitted means <see langword="false"/>.
        /// </summary>
        public bool LegacyMachine { get; set; } = false;

        public string? Policy => PolicyName;

        // IAuthorizeData requires these getters & setters. Implementing these interfaces through the
        // IAuthorizeData class reference so that AllowAccessToAttribute can keep them hidden as private.
        // Setters deliberately chosen as noop to prevent the users of this class from overriding values
        // that this class needs to function as expected. Policy should stay hardcoded, while Roles and
        // AuthenticationSchemes aren't use due to authentication being handled by lambda authorizer;
        // setting them to concrete values would either make an endpoint return 403, or make it throw.
        string? IAuthorizeData.Policy
        {
            get => PolicyName;
            set { }
        }

        string? IAuthorizeData.Roles
        {
            get => null;
            set { }
        }

        string? IAuthorizeData.AuthenticationSchemes
        {
            get => null;
            set { }
        }
    }
}
