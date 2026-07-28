using ParserService.Dtos;

namespace ParserService.Clients
{
    /// <summary>
    /// Read-only client to Courses Service. Parser Service only ever asks
    /// "does this course exist / who owns it" — it never creates or edits courses.
    /// </summary>
    public interface ICoursesServiceClient
    {
        /// <summary>Returns null if the course does not exist.</summary>
        Task<CourseSummaryDto?> GetCourseAsync(int courseId, CancellationToken ct = default);
    }
}
