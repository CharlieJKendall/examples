using AutomatedTests.Api.Options;
using Microsoft.Extensions.Options;

namespace AutomatedTests.Api.Services;

public class MyService : IMyService
{
    private readonly IOptions<MyOptions> _options;

    public MyService(IOptions<MyOptions> options)
    {
        _options = options;
    }
}

public interface IMyService { }
