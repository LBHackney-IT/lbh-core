using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hackney.Core.HealthCheck
{
    public static class HealthCheckResponseWriter
    {
        /// <summary>
        /// Custom response writer to provide a full json serilaisation of the HealthReport
        /// into the HttpResponse
        /// </summary>
        /// <param name="httpContext">The HttpContext</param>
        /// <param name="report">The full HealthReport generated from all configured health checks</param>
        public static Task WriteResponse(HttpContext httpContext, HealthReport report)
        {
            httpContext.Response.ContentType = "application/json; charset=utf-8";

            var response = new HealthCheckResponse(report);
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
