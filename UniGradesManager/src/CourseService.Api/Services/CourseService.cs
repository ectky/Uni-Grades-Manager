using CourseService.Domain.Entities;
using CourseService.Dtos;
using CourseService.Exceptions;
using CourseService.Repositories;

namespace CourseService.Services
{
    public class CourseService : ICourseService
    {
        private const string AdminRole = "Admin";

        private readonly ICourseRepository _repository;
        private readonly ILogger<CourseService> _logger;

        public CourseService(ICourseRepository repository, ILogger<CourseService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<CourseDto>> GetAllAsync(CancellationToken ct = default)
        {
            var courses = await _repository.GetAllAsync(ct);
            return courses.Select(ToDto).ToList();
        }

        public async Task<List<CourseDto>> GetByInstructorAsync(int instructorId, CancellationToken ct = default)
        {
            var courses = await _repository.GetByInstructorAsync(instructorId, ct);
            return courses.Select(ToDto).ToList();
        }

        public async Task<CourseSummaryDto> GetSummaryAsync(int courseId, CancellationToken ct = default)
        {
            var course = await _repository.GetByIdAsync(courseId, ct)
                ?? throw new CourseNotFoundException(courseId);

            return new CourseSummaryDto
            {
                Id = course.Id,
                Name = course.Name,
                Period = course.Period,
                InstructorId = course.InstructorId,
            };
        }

        public async Task<CourseDto> CreateAsync(CreateCourseDto dto, CancellationToken ct = default)
        {
            Validate(dto.Name, dto.Period);

            if (dto.InstructorId <= 0)
                throw new InvalidCourseException("InstructorId must be a positive integer.");

            var course = new Course
            {
                Name = dto.Name.Trim(),
                Period = dto.Period.Trim(),
                InstructorId = dto.InstructorId,
            };

            var created = await _repository.AddAsync(course, ct);

            _logger.LogInformation(
                "Course {CourseId} \"{Name}\" created for instructor {InstructorId}",
                created.Id, created.Name, created.InstructorId);

            return ToDto(created);
        }

        public async Task<CourseDto> UpdateAsync(
            int courseId, UpdateCourseDto dto, int requestingUserId, string requestingRole,
            CancellationToken ct = default)
        {
            Validate(dto.Name, dto.Period);

            var course = await _repository.GetByIdAsync(courseId, ct)
                ?? throw new CourseNotFoundException(courseId);

            // Admin can edit any course; an Instructor may only edit their own
            // (Приложение №1: "Admin, Instructor-собственик").
            var isOwner = course.InstructorId == requestingUserId;
            if (requestingRole != AdminRole && !isOwner)
            {
                throw new ForbiddenCourseAccessException(courseId, requestingUserId);
            }

            course.Name = dto.Name.Trim();
            course.Period = dto.Period.Trim();

            await _repository.UpdateAsync(course, ct);

            _logger.LogInformation("Course {CourseId} updated by user {UserId} ({Role})",
                courseId, requestingUserId, requestingRole);

            return ToDto(course);
        }

        private static void Validate(string name, string period)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCourseException("Name is required.");

            if (string.IsNullOrWhiteSpace(period))
                throw new InvalidCourseException("Period is required.");
        }

        private static CourseDto ToDto(Course c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Period = c.Period,
            InstructorId = c.InstructorId,
        };
    }
}
