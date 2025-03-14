using CancellationTokens.Tests.TestUtilities;

namespace CancellationTokens.Tests.Controllers;

public class CancellationTokenControllerTests
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
    public void Handles_request_cancellation() =>
        _MakeRequestAndCancel(
            endpoint: "CancellationToken/supports-cancellation",
            expectedLog: "Request was cancelled");

    [Test]
    public void Does_not_handle_request_cancellation() =>
        _MakeRequestAndCancel(
            endpoint: "CancellationToken/does-not-support-cancellation",
            expectedLog: "Request ran to completion");

    private void _MakeRequestAndCancel(string endpoint, string expectedLog)
    {
        var client = _webApplictionFactory.CreateClient();

        using var cts = new CancellationTokenSource();
        var requestTask = client.GetAsync(endpoint, cts.Token);
        cts.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(() => requestTask);
        Assert.That(TestLogger.Logs, Has.One.Match(expectedLog));
    }
}