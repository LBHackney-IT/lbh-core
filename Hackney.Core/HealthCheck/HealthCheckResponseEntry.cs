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

        //
        // Summary:
        //     Gets additional key-value pairs describing the health of the component.
        public IReadOnlyDictionary<string, object> Data { get; set; }
        //
        // Summary:
        //     Gets a human-readable description of the status of the component that was checked.
        public string Description { get; set; }
        //
        // Summary:
        //     Gets the health check execution duration.
        public double DurationMs { get; set; }
        //
        // Summary:
        //     The message from the exception that was thrown when checking
        //     for status (if any).
        public string ExceptionMessage { get; set; }
        //
        // Summary:
        //     Gets the health status of the component that was checked.
        public HealthStatus Status { get; set; }
    }
}
