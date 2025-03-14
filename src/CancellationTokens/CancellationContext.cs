namespace CancellationTokens;

public interface ICancellationContext
{
    CancellationToken Token { get; }
}

public interface ISettableCancellationContext : ICancellationContext
{
    new CancellationToken Token { get; set; }
}

public class CancellationContext : ISettableCancellationContext
{
    public CancellationToken Token { get; set; }
}
