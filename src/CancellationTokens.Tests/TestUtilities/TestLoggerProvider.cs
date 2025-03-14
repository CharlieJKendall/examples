namespace CancellationTokens.Tests.TestUtilities;

public class TestLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new TestLogger();

    public void Dispose()
    {
    }
}
