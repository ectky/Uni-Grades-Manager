using GradeService.Domain.Entities;

namespace GradeService.Repositories
{
    public interface IGradeRepository
    {
        /// <summary>
        /// Inserts new (CourseId, StudentId) rows and updates the Value of
        /// existing ones, in a single transaction. Returns how many of each.
        /// </summary>
        Task<(int inserted, int updated)> UpsertManyAsync(
            int courseId, IEnumerable<(int StudentId, double Value)> records, CancellationToken ct = default);

        Task<List<Grade>> GetByCourseAsync(int courseId, CancellationToken ct = default);

        Task<List<Grade>> GetByStudentAsync(int studentId, CancellationToken ct = default);
    }
}
