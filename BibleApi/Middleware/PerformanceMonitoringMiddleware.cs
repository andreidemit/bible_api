using System.Diagnostics;

namespace BibleApi.Middleware;

/// <summary>
/// Middleware to track performance metrics for requests
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value;
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var statusCode = context.Response.StatusCode;
            var method = context.Request.Method;
            var elapsed = stopwatch.ElapsedMilliseconds;
            
            // Log slow requests (>1 second)
            if (elapsed > 1000)
            {
                _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMs}ms, Status: {StatusCode}", 
                    method, path, elapsed, statusCode);
            }
            // Log all API requests in debug mode
            else if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Request: {Method} {Path} took {ElapsedMs}ms, Status: {StatusCode}", 
                    method, path, elapsed, statusCode);
            }

            // Add performance headers for debugging
            context.Response.Headers.TryAdd("X-Response-Time-Ms", elapsed.ToString());
        }
    }
}

/// <summary>
/// Extension method to register the performance monitoring middleware
/// </summary>
public static class PerformanceMonitoringExtensions
{
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
    }
}