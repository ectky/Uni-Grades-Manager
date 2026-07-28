namespace AnalyticsService.Dtos
{
    /// <summary>GET /api/analytics/course/{id} — matches frontend's analyticsApi.js comment exactly.</summary>
    public class CourseStatsDto
    {
        public int CourseId { get; set; }
        public double Average { get; set; }
        public double Median { get; set; }
        public double Mode { get; set; }
        public double StdDev { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double PassRatePercent { get; set; }
        public int StudentCount { get; set; }
    }

    /// <summary>One bar in GET /api/analytics/course/{id}/distribution.</summary>
    public class DistributionItemDto
    {
        public int Grade { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// One row in GET /api/analytics/course/{id}/grades — raw (StudentId, Value)
    /// from Grades Service enriched with StudentName from User Service.
    /// </summary>
    public class CourseGradeDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = default!;
        public double Value { get; set; }
    }

    /// <summary>GET /api/analytics/student/{id} — aggregate only, no per-course detail.</summary>
    public class StudentStatsDto
    {
        public int StudentId { get; set; }
        public double Average { get; set; }
        public int GradedCourseCount { get; set; }
    }

    /// <summary>
    /// One row in GET /api/analytics/student/{id}/grades — raw (CourseId, Value)
    /// from Grades Service enriched with CourseName/Period from Courses Service.
    /// This is what serves as the student's "my courses" view — see
    /// CourseService.Api's README for why that's not served by Courses Service directly.
    /// </summary>
    public class StudentGradeDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = default!;
        public string Period { get; set; } = default!;
        public double Value { get; set; }
    }
}
