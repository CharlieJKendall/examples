namespace CancellationTokens.BackgroundServices;

public class CancellableBlogProcessorBackgroundService(IServiceScopeFactory scopeFactory)
    : CancellableBackgroundService(scopeFactory)
{
    protected override async Task ExecuteAsync(AsyncServiceScope scope)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CancellableBlogProcessorBackgroundService>>();
        var repository = scope.ServiceProvider.GetRequiredService<IBlogRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<IBlogProcessor>();

        try
        {
            foreach (var blog in await repository.GetBlogs())
            {
                // Do time-consuming processing for each blog
                await processor.Process(blog);
            }

            logger.LogInformation("Completed background processing for CancellableBlogProcessorBackgroundService");
        }
        catch (TaskCanceledException ex)
        {
            logger.LogInformation(ex, "Background processing cancelled for CancellableBlogProcessorBackgroundService");
        }
    }
}
