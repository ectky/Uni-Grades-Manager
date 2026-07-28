using System.Security.Claims;

namespace CourseService.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Reads the caller's own id from the JWT "sub" (NameIdentifier) claim,
        /// set by User Service at login (Chapter 4.2). Used to decide whether an
        /// Instructor owns the course they're trying to edit.
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("JWT is missing a NameIdentifier (sub) claim.");
            return int.Parse(value);
        }
    }
}
