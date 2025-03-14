namespace CancellationTokens.BackgroundServices;

public class BlogProcessorBackgroundService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BlogProcessorBackgroundService>>();
        var repository = scope.ServiceProvider.GetRequiredService<IBlogRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<IBlogProcessor>();

        try
        {
            foreach (var blog in await repository.GetBlogs())
            {
                // Do time-consuming processing for each blog
                await processor.Process(blog);
            }

            logger.LogInformation("Completed background processing for BlogProcessorBackgroundService");
        }
        catch (TaskCanceledException ex)
        {
            logger.LogInformation(ex, "Background processing cancelled for BlogProcessorBackgroundService");
        }
    }
}