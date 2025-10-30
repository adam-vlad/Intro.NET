using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Orders.Middleware;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationMiddleware> logger)
    {
        string correlationId = string.Empty;
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var values))
        {
            correlationId = values.ToString();
        }
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var scopeData = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        };

        using (logger.BeginScope(scopeData))
        {
            await _next(context);
        }
    }
}