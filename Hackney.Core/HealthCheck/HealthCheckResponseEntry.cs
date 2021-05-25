using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Hackney.Core.HealthCheck
{
    public class HealthCheckResponseEntry
    {
        public HealthCheckResponseEntry(HealthStatus status, string description, TimeSpan duration, string exceptionMessage,
            IReadOnlyDictionary<string, object> data)
        {
            Status = status;
            Description = description;
            DurationMs = duration.TotalMilliseconds;
            ExceptionMessage = exceptionMessage;
            Data = data;
        }
        public HealthCheckResponseEntry() { }

        public IReadOnlyDictionary<string, object> Data { get; set; }

        public string Description { get; set; }

        public double DurationMs { get; set; }

        public string ExceptionMessage { get; set; }

        public HealthStatus Status { get; set; }
    }
}
