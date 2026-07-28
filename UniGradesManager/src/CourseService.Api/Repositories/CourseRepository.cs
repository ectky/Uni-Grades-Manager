using CourseService.Data;
using CourseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly CoursesDbContext _db;

        public CourseRepository(CoursesDbContext db)
        {
            _db = db;
        }

        public Task<Course?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);

        public Task<List<Course>> GetAllAsync(CancellationToken ct = default) =>
            _db.Courses.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

        public Task<List<Course>> GetByInstructorAsync(int instructorId, CancellationToken ct = default) =>
            _db.Courses.AsNoTracking()
                .Where(c => c.InstructorId == instructorId)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);

        public async Task<Course> AddAsync(Course course, CancellationToken ct = default)
        {
            _db.Courses.Add(course);
            await _db.SaveChangesAsync(ct);
            return course;
        }

        public async Task UpdateAsync(Course course, CancellationToken ct = default)
        {
            _db.Courses.Update(course);
            await _db.SaveChangesAsync(ct);
        }
    }
}
