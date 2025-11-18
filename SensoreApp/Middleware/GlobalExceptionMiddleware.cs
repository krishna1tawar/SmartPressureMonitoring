using System.Net;
using System.Text.Json;
using SensoreApp.Models;

namespace SensoreApp.Middleware
{
    /// <summary>
    /// 🌐 Handles all unhandled exceptions globally
    /// and returns unified ApiResponse in JSON.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Continue pipeline
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Unhandled Exception Occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ApiResponse<string>
            {
                Success = false,
                Message = $"An unexpected error occurred: {ex.Message}",
                Data = null,
                StatusCode = HttpStatusCode.InternalServerError
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }

    // Extension method to register the middleware easily
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}