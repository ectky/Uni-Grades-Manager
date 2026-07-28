using AnalyticsService.Dtos;

namespace AnalyticsService.Clients
{
    /// <summary>
    /// Analytics Service's only way to see raw grades — it has no database of
    /// its own (Chapter 2.3). Calls Grades Service's internal-only
    /// GET /api/grades/course/{id} and GET /api/grades/student/{id}.
    /// </summary>
    public interface IGradesServiceClient
    {
        Task<List<RawGradeDto>> GetByCourseAsync(int courseId, CancellationToken ct = default);
        Task<List<RawGradeDto>> GetByStudentAsync(int studentId, CancellationToken ct = default);
    }
}
