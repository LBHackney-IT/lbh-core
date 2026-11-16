using Microsoft.AspNetCore.Authorization;
using System;

namespace Hackney.Core.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowAccessToAttribute : AuthorizeAttribute
    {
        public const string PolicyName = "HybridAccessPolicy";

        public string? User { get; set; }
        public string? Machine { get; set; }

        /// <summary>
        /// When <see langword="true"/>, a legacy machine token may call this endpoint.
        /// Omitted means <see langword="false"/>.
        /// </summary>
        public bool LegacyMachine { get; set; } = false;

        public AllowAccessToAttribute()
        {
            Policy = PolicyName;
        }
    }
}
