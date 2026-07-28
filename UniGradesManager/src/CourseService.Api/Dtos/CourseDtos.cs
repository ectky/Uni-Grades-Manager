namespace CourseService.Dtos
{
    /// <summary>Public shape returned by GET /api/courses and GET /api/courses/my.</summary>
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Period { get; set; } = default!;
        public int InstructorId { get; set; }
    }

    /// <summary>Body of POST /api/courses. Admin only — see Приложение №1.</summary>
    public class CreateCourseDto
    {
        public string Name { get; set; } = default!;
        public string Period { get; set; } = default!;
        public int InstructorId { get; set; }
    }

    /// <summary>
    /// Body of PUT /api/courses/{id}. Note there's no InstructorId here —
    /// reassigning a course to a different instructor is an Admin-only concern
    /// and, if needed, should be its own explicit action rather than folded into
    /// a generic edit that an owning Instructor is also allowed to call.
    /// </summary>
    public class UpdateCourseDto
    {
        public string Name { get; set; } = default!;
        public string Period { get; set; } = default!;
    }

    /// <summary>
    /// Shape returned by the internal GET /api/courses/{id} lookup, used by
    /// Parser Service (existence check) and Analytics Service (name/period
    /// enrichment). Identical fields to CourseDto today, but kept as a
    /// separate type since the public and internal contracts are free to
    /// diverge later without one accidentally breaking the other.
    /// </summary>
    public class CourseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Period { get; set; } = default!;
        public int InstructorId { get; set; }
    }
}
