using CancellationTokens.Tests.TestUtilities;

namespace CancellationTokens.Tests.BackgroundServices;

public class CancellableBlogProcessorBackgroundServiceTests
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
    public async Task Cancels_processing_when_host_stops()
    {
        // Act
        await _webApplictionFactory.Services.GetRequiredService<IHost>().StopAsync();

        // Assert
        Assert.That(TestLogger.Logs, Has.One.Match("Background processing cancelled for CancellableBlogProcessorBackgroundService"));
    }
}
