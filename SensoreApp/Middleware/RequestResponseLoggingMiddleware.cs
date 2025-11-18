using System.Diagnostics;
using System.Text;
using SensoreApp.Services;

namespace SensoreApp.Middleware
{
    /// <summary>
    /// 📊 Logs every incoming request and outgoing response.
    /// Helps with debugging and performance tracking.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggingService logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log the incoming request
            var request = context.Request;
            string requestBody = string.Empty;

            // Read request body (if present)
            if (request.ContentLength > 0 && request.Body.CanSeek)
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            await _logger.LogInfoAsync(
                $"➡️ Request: {request.Method} {request.Path}{request.QueryString} | Body: {requestBody}"
            );

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context); // Call next middleware / controller

            stopwatch.Stop();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            await _logger.LogInfoAsync(
                $"⬅️ Response: {context.Response.StatusCode} | Duration: {stopwatch.ElapsedMilliseconds} ms | Body: {responseText}"
            );

            // Copy back the original response
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    // ✅ Extension method for cleaner registration
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}