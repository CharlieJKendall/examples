using Microsoft.AspNetCore.Mvc.Filters;

namespace CancellationTokens;

public class SupportsRequestCancellationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var cancellationContext = context.HttpContext.RequestServices.GetRequiredService<ISettableCancellationContext>();
        cancellationContext.Token = context.HttpContext.RequestAborted;
    }
}