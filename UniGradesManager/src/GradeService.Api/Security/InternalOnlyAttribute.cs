using Microsoft.AspNetCore.Mvc.Filters;

namespace GradeService.Security
{
    /// <summary>
    /// Grades Service has no user-facing endpoints — every action is called by
    /// another service (Parser Service writes, Analytics Service reads), never
    /// directly by the React frontend. Since the system has no API Gateway to
    /// enforce that boundary centrally, each internal endpoint checks a shared
    /// secret header instead of a user JWT + role.
    ///
    /// The header name and expected value come from configuration
    /// (Internal:ApiKey), which .NET Aspire injects identically into
    /// Parser Service, Analytics Service, and Grades Service.
    /// </summary>
    public class InternalOnlyAttribute : ActionFilterAttribute
    {
        private const string HeaderName = "X-Internal-Api-Key";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expectedKey = config["Internal:ApiKey"];

            if (string.IsNullOrEmpty(expectedKey))
            {
                // Fail closed: a missing configuration value must not silently open the endpoint.
                context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey) ||
                providedKey != expectedKey)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
