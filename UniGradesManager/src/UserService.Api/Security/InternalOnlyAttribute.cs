using Microsoft.AspNetCore.Mvc.Filters;

namespace UserService.Security
{
    /// <summary>
    /// Same pattern as Grades Service / Courses Service — guards
    /// GET /api/users/batch, whose only caller is Analytics Service
    /// (enriching StudentId/InstructorId values with names). Checked via a
    /// shared secret header rather than a user JWT, since there's no
    /// end-user identity behind this call.
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
