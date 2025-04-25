using AutomatedTests.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace AutomatedTests.Tests.Regression;

internal class DependencyRegistrationTests
{
    [Test]
    public async Task Validate_dependency_injection_registrations()
    {
        var webApplicationFactory = new CustomWebApplicationFactory();
        webApplicationFactory.StartServer();

        var successfullyResolvedTypes = new HashSet<Type>();
        var unsuccessfullyResolvedTypes = new HashSet<Type>();

        foreach (var controllerType in TestHelpers.GetAllControllerTypes())
        {
            await Impl(controllerType, webApplicationFactory.Services);
        }

        var registeredServices = webApplicationFactory.ServiceCollection
            .Where(x => x.ServiceType.Assembly.GetName().Name!.StartsWith("AutomatedTests"))
            .Select(x => x.ServiceType)
            .ToArray();

        var unusedRegistrations = registeredServices
            .Except(successfullyResolvedTypes)
            .ToArray();

        var duplicateRegistrations = registeredServices
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.First())
            .ToArray();

        Assert.Multiple(() =>
        {
            CollectionAssert.IsEmpty(unusedRegistrations, "Unused registrations should be removed from the DI container");
            CollectionAssert.IsEmpty(duplicateRegistrations, "Duplicate registrations should be removed from the DI container");
            CollectionAssert.IsEmpty(unsuccessfullyResolvedTypes, "Unable to resolve some required dependencies from the DI container");
        });

        async Task Impl(Type type, IServiceProvider serviceProvider)
        {
            if (successfullyResolvedTypes.Contains(type))
            {
                return;
            }

            if (unsuccessfullyResolvedTypes.Contains(type))
            {
                return;
            }

            try
            {
                await using var scope = webApplicationFactory.Services.CreateAsyncScope();
                var resolvedService = scope.ServiceProvider.GetRequiredService(type);
                successfullyResolvedTypes.Add(type);

                if (type.Namespace!.StartsWith("AutomatedTests") is false)
                {
                    return;
                }

                var construtorParameterTypes = resolvedService
                    .GetType()
                    .GetConstructors()
                    .SelectMany(x => x.GetParameters())
                    .Select(x => x.ParameterType);

                foreach (var parameterType in construtorParameterTypes)
                {
                    await Impl(parameterType, serviceProvider);
                }
            }
            catch
            {
                unsuccessfullyResolvedTypes.Add(type);
            }
        }
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IServiceCollection ServiceCollection { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services => ServiceCollection = services);
    }

    public void StartServer()
    {
        _ = Server;
    }
}