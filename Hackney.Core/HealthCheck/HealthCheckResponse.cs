using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.HealthCheck
{
    public class HealthCheckResponse
    {
        public HealthCheckResponse(HealthReport report)
        {
            Entries = report.Entries.ToDictionary(x => x.Key, y =>
                new HealthCheckResponseEntry(y.Value.Status, y.Value.Description,
                                             y.Value.Duration, y.Value.Exception?.Message, y.Value.Data));
            Status = report.Status;
            TotalDurationMs = report.TotalDuration.TotalMilliseconds;
        }
        public HealthCheckResponse() { }

        public Dictionary<string, HealthCheckResponseEntry> Entries { get; set; }

        public HealthStatus Status { get; set; }

        public double TotalDurationMs { get; set; }
    }
}
