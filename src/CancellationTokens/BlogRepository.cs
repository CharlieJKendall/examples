namespace CancellationTokens;

public interface IBlogRepository
{
    Task<Blog[]> GetBlogs();
}

public class BlogRepository(ICancellationContext cancellationContext) : IBlogRepository
{
    private readonly ICancellationContext _cancellationContext = cancellationContext;

    public async Task<Blog[]> GetBlogs()
    {
        // Simulate a time-consuming database query
        await Task.Delay(1000, _cancellationContext.Token);

        return [];
    }
}
