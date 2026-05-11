using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace API.HealthChecks;


public static class HealthCheckResponseWriter
{
    public static async Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                tags = e.Value.Tags,
                data = e.Value.Data.Count > 0 ? e.Value.Data : null,
                exception = e.Value.Exception?.Message
            })
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
