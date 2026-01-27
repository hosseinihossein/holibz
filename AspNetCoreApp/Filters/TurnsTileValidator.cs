using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCoreApp.Filters;

public class ValidateTurnstileTokenAttribute : ActionFilterAttribute
{
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        return base.OnActionExecutionAsync(context, next);
    }
}