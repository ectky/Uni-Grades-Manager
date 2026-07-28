using System.Security.Claims;

namespace AnalyticsService.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("JWT is missing a NameIdentifier (sub) claim.");
            return int.Parse(value);
        }
    }
}
