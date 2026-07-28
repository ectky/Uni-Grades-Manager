using CourseService.Dtos;

namespace CourseService.Services
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetAllAsync(CancellationToken ct = default);
        Task<List<CourseDto>> GetByInstructorAsync(int instructorId, CancellationToken ct = default);
        Task<CourseSummaryDto> GetSummaryAsync(int courseId, CancellationToken ct = default);
        Task<CourseDto> CreateAsync(CreateCourseDto dto, CancellationToken ct = default);

        /// <summary>
        /// requestingUserId/requestingRole identify the caller so the service can
        /// enforce "Admin, or the Instructor who owns this course" (Приложение №1).
        /// </summary>
        Task<CourseDto> UpdateAsync(
            int courseId, UpdateCourseDto dto, int requestingUserId, string requestingRole, CancellationToken ct = default);
    }
}
