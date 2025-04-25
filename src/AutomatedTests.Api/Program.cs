using AutomatedTests.Api.Options;
using AutomatedTests.Api.Services;
using AutomatedTests.LibraryB;

namespace AutomatedTests.Api
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddOptions<MyOptions>();
            builder.Services.AddScoped<IMyService, MyService>();
            builder.Services.AddScoped<IMyUnusedService, MyUnusedService>();
            builder.Services.AddScoped<IMyUnusedService, MyUnusedService>();
            builder.Services.AddDbContext<MyDbContext>();
            
            builder.Services
                .AddControllers()
                .AddControllersAsServices();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}