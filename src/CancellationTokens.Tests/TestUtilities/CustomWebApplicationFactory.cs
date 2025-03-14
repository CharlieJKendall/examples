using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CancellationTokens.Tests.TestUtilities;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(_ => { }).ConfigureServices(services =>
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>());
        });
    }
}
