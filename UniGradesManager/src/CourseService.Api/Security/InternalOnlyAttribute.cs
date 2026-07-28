using Microsoft.AspNetCore.Mvc.Filters;

namespace CourseService.Security
{
    /// <summary>
    /// Same pattern as Grades Service's InternalOnlyAttribute — guards the one
    /// endpoint here (GET /api/courses/{id}) that exists purely for
    /// service-to-service calls (Parser Service's existence check, Analytics
    /// Service's name/period enrichment), not for the frontend. Checked via a
    /// shared secret header instead of a user JWT, since neither caller has
    /// (or should forward) an end-user token for this.
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
