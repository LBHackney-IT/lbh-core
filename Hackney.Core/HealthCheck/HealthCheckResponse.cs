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

        //
        // Summary:
        //     A System.Collections.Generic.IReadOnlyDictionary`2 containing the results from
        //     each health check.
        //
        // Remarks:
        //     The keys in this dictionary map the name of each executed health check to a Microsoft.Extensions.Diagnostics.HealthChecks.HealthReportEntry
        //     for the result data retruned from the corresponding health check.
        public Dictionary<string, HealthCheckResponseEntry> Entries { get; set; }
        //
        // Summary:
        //     Gets a Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus representing
        //     the aggregate status of all the health checks. The value of Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport.Status
        //     will be the most servere status reported by a health check. If no checks were
        //     executed, the value is always Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy.
        public HealthStatus Status { get; set; }
        //
        // Summary:
        //     Gets the time the health check service took to execute.
        public double TotalDurationMs { get; set; }
    }
}
