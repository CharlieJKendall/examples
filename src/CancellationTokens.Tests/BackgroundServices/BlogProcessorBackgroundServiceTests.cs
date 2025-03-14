using CancellationTokens.Tests.TestUtilities;

namespace CancellationTokens.Tests.BackgroundServices;

public class BlogProcessorBackgroundServiceTests
{
    private CustomWebApplicationFactory _webApplictionFactory = null!;

    [SetUp]
    public void SetUp()
    {
        TestLogger.Logs.Clear();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _webApplictionFactory = new CustomWebApplicationFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _webApplictionFactory.Dispose();
    }

    [Test]
    public async Task Does_not_cancel_processing_when_host_stops()
    {
        // Act
        await _webApplictionFactory.Services.GetRequiredService<IHost>().StopAsync();

        // Assert
        Assert.That(TestLogger.Logs, Has.One.Match("Completed background processing for BlogProcessorBackgroundService"));
    }
}
