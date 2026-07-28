using CourseService.Domain.Entities;

namespace CourseService.Repositories
{
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Course>> GetAllAsync(CancellationToken ct = default);
        Task<List<Course>> GetByInstructorAsync(int instructorId, CancellationToken ct = default);
        Task<Course> AddAsync(Course course, CancellationToken ct = default);
        Task UpdateAsync(Course course, CancellationToken ct = default);
    }
}
