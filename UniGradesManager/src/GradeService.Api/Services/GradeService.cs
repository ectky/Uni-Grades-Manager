using GradeService.Domain.Entities;
using GradeService.Dtos;
using GradeService.Exceptions;
using GradeService.Repositories;

namespace GradeService.Services
{
    public class GradeService : IGradeService
    {
        // Bulgarian grading scale — kept in sync with Parser Service's own check.
        // Re-validated here because Grades Service should not blindly trust callers.
        private const double MinGradeValue = 2.00;
        private const double MaxGradeValue = 6.00;

        private readonly IGradeRepository _repository;
        private readonly ILogger<GradeService> _logger;

        public GradeService(IGradeRepository repository, ILogger<GradeService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<BatchGradeUploadResultDto> UploadGradesAsync(
            BatchGradeUploadDto batch, CancellationToken ct = default)
        {
            Validate(batch);

            var records = batch.Grades
                .Select(g => (g.StudentId, g.Value))
                .ToList();

            var (inserted, updated) = await _repository.UpsertManyAsync(batch.CourseId, records, ct);

            _logger.LogInformation(
                "Course {CourseId}: inserted {Inserted}, updated {Updated} grade(s)",
                batch.CourseId, inserted, updated);

            return new BatchGradeUploadResultDto
            {
                CourseId = batch.CourseId,
                Inserted = inserted,
                Updated = updated,
            };
        }

        public async Task<List<GradeDto>> GetByCourseAsync(int courseId, CancellationToken ct = default)
        {
            var grades = await _repository.GetByCourseAsync(courseId, ct);
            return grades.Select(ToDto).ToList();
        }

        public async Task<List<GradeDto>> GetByStudentAsync(int studentId, CancellationToken ct = default)
        {
            var grades = await _repository.GetByStudentAsync(studentId, ct);
            return grades.Select(ToDto).ToList();
        }

        private static void Validate(BatchGradeUploadDto batch)
        {
            if (batch.CourseId <= 0)
                throw new InvalidGradeBatchException("CourseId must be a positive integer.");

            if (batch.Grades is null || batch.Grades.Count == 0)
                throw new InvalidGradeBatchException("The batch must contain at least one grade.");

            var duplicateStudentIds = batch.Grades
                .GroupBy(g => g.StudentId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateStudentIds.Count > 0)
            {
                throw new InvalidGradeBatchException(
                    $"The batch contains duplicate StudentId value(s): {string.Join(", ", duplicateStudentIds)}.");
            }

            foreach (var g in batch.Grades)
            {
                if (g.StudentId <= 0)
                    throw new InvalidGradeBatchException($"Invalid StudentId: {g.StudentId}.");

                if (g.Value < MinGradeValue || g.Value > MaxGradeValue)
                {
                    throw new InvalidGradeBatchException(
                        $"Grade value {g.Value} for StudentId {g.StudentId} is outside the allowed range " +
                        $"[{MinGradeValue:0.00}, {MaxGradeValue:0.00}].");
                }
            }
        }

        private static GradeDto ToDto(Grade g) => new()
        {
            Id = g.Id,
            CourseId = g.CourseId,
            StudentId = g.StudentId,
            Value = g.Value,
        };
    }
}
