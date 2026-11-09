using System.IO;
using Microsoft.Extensions.Logging;

namespace SnipletManager.Desktop.Services;

public sealed class FileLoggerProvider(string path) : ILoggerProvider
{
    private readonly object gate = new();
    public ILogger CreateLogger(string categoryName) => new FileLogger(path, categoryName, gate);
    public void Dispose() { }

    private sealed class FileLogger(string path, string category, object gate) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            lock (gate)
            {
                // Do not log snippet content or database exception messages, which can contain user text.
                File.AppendAllText(path, $"{DateTimeOffset.Now:O} {logLevel} {category}: {formatter(state, null)}"
                    + (exception is null ? "" : $" [{exception.GetType().Name}]") + Environment.NewLine);
            }
        }
    }
}
