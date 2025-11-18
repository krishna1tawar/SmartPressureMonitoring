using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SensoreApp.Services
{
    public interface ILoggingService
    {
        Task LogInfoAsync(string message);
        Task LogErrorAsync(string message, Exception ex = null);
    }

    /// <summary>
    /// Centralized logging service writing to console and file.
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly string _logDirectory;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        public async Task LogInfoAsync(string message)
        {
            _logger.LogInformation($"ℹ️ {message}");
            await WriteToFileAsync("INFO", message);
        }

        public async Task LogErrorAsync(string message, Exception ex = null)
        {
            _logger.LogError(ex, $"❌ {message}");
            await WriteToFileAsync("ERROR", $"{message} | {ex?.Message}");
        }

        private async Task WriteToFileAsync(string level, string message)
        {
            string logPath = Path.Combine(_logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.log");
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
            await File.AppendAllTextAsync(logPath, logEntry);
        }
    }
}