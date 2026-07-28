namespace AnalyticsService.Dtos
{
    /// <summary>Mirrors GradeService.Api's GradeDto (GET /api/grades/course/{id}, /student/{id}).</summary>
    public class RawGradeDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public double Value { get; set; }
    }

    /// <summary>Mirrors UserService.Api's UserBatchItemDto (GET /api/users/batch).</summary>
    public class UserBatchItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int? StudentId { get; set; }
    }

    /// <summary>Mirrors CourseService.Api's CourseSummaryDto (GET /api/courses/{id}).</summary>
    public class CourseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Period { get; set; } = default!;
        public int InstructorId { get; set; }
    }
}
