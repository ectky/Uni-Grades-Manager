using AnalyticsService.Dtos;

namespace AnalyticsService.Services
{
    public interface IAnalyticsService
    {
        Task<CourseStatsDto> GetCourseStatsAsync(int courseId, CancellationToken ct = default);
        Task<List<DistributionItemDto>> GetCourseDistributionAsync(int courseId, CancellationToken ct = default);
        Task<List<CourseGradeDto>> GetCourseGradesAsync(int courseId, CancellationToken ct = default);

        /// <summary>
        /// "My" stats/grades for the currently logged-in Student. Resolves
        /// their roll number (User.StudentId) from their JWT identity
        /// (User.Id) internally — the frontend never needs to know or pass
        /// that number. See AnalyticsService.Api/README.md.
        /// </summary>
        Task<StudentStatsDto> GetMyStatsAsync(int requestingUserId, CancellationToken ct = default);
        Task<List<StudentGradeDto>> GetMyGradesAsync(int requestingUserId, CancellationToken ct = default);

        /// <summary>
        /// Instructor-only lookup by roll number directly — e.g. to check a
        /// specific student's record. studentId here IS the roll number
        /// (Grades Service's StudentId), not a User.Id.
        /// </summary>
        Task<StudentStatsDto> GetStudentStatsByRollNumberAsync(int studentRollNumber, CancellationToken ct = default);
        Task<List<StudentGradeDto>> GetStudentGradesByRollNumberAsync(int studentRollNumber, CancellationToken ct = default);
    }
}
