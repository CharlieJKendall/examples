namespace CancellationTokens;

public interface IBlogProcessor
{
    Task Process(Blog blog);
}

public class BlogProcessor(ICancellationContext cancellationContext) : IBlogProcessor
{
    private readonly ICancellationContext _cancellationContext = cancellationContext;

    public async Task Process(Blog blog)
    {
        // Simulate time-consuming processing
        await Task.Delay(1000, _cancellationContext.Token);
    }
}