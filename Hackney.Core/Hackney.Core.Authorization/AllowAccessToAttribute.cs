using Microsoft.AspNetCore.Authorization;
using System;

namespace Hackney.Core.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowAccessToAttribute : AuthorizeAttribute
    {
        public string? User { get; set; }
        public string? Machine { get; set; }

        public AllowAccessToAttribute()
        {
            Policy = "HybridAccessPolicy";
        }
    }
}
