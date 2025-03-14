using Microsoft.Extensions.Logging;

namespace CancellationTokens.BackgroundServices;

public abstract class CancellableBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var cancellationContext = scope.ServiceProvider.GetRequiredService<ISettableCancellationContext>();
        cancellationContext.Token = stoppingToken;

        await ExecuteAsync(scope);
    }

    protected abstract Task ExecuteAsync(AsyncServiceScope scope);
}
