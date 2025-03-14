using System.Collections.Concurrent;

namespace CancellationTokens.Tests.TestUtilities;

public class TestLogger : ILogger
{
    public static ConcurrentBag<string> Logs { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) => Logs.Add(formatter(state, exception));
}
