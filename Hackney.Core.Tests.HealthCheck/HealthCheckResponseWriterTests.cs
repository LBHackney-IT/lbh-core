using FluentAssertions;
using Hackney.Core.HealthCheck;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Hackney.Core.Tests.HealthCheck
{
    public class HealthCheckResponseWriterTests
    {
        [Fact]
        public void WriteResponseTest()
        {
            var err = "Some exception message";
            var totalDuration = TimeSpan.FromMilliseconds(200);
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var reports = new Dictionary<string, HealthReportEntry>
            {
                { "Test1", new HealthReportEntry(HealthStatus.Healthy, "Check 1", TimeSpan.FromMilliseconds(50), null, null) },
                { "Test2", new HealthReportEntry(HealthStatus.Degraded, "Check 2", TimeSpan.FromMilliseconds(45), null, null) },
                { "Test3", new HealthReportEntry(HealthStatus.Unhealthy, "Check 3", TimeSpan.FromMilliseconds(50), new Exception(err), null) }
            };
            var fullReport = new HealthReport(reports, totalDuration);

            HealthCheckResponseWriter.WriteResponse(httpContext, fullReport).Wait();

            httpContext.Response.Body.Position = 0;
            using (var reader = new StreamReader(httpContext.Response.Body))
            {
                var responseText = reader.ReadToEnd();
                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                options.Converters.Add(new JsonStringEnumConverter());
                var jsonResponse = JsonSerializer.Deserialize<HealthCheckResponse>(responseText, options);

                var expected = new HealthCheckResponse(fullReport);
                jsonResponse.Should().BeEquivalentTo(expected);
            }
        }
    }
}
