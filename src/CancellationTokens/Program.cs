using CancellationTokens;
using CancellationTokens.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ISettableCancellationContext, CancellationContext>();
builder.Services.AddScoped<ICancellationContext>(x => x.GetRequiredService<ISettableCancellationContext>());
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IBlogProcessor, BlogProcessor>();

builder.Services.AddHostedService<BlogProcessorBackgroundService>();
builder.Services.AddHostedService<CancellableBlogProcessorBackgroundService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        await next(context);
        logger.LogInformation("Request ran to completion");
    }
    catch (TaskCanceledException ex)
    {
        logger.LogInformation(ex, "Request was cancelled");
    }
});

app.MapControllers();
app.Run();

public partial class Program
{
}