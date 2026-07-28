namespace GradeService.Dtos
{
    /// <summary>One row as received from Parser Service.</summary>
    public class GradeRecordDto
    {
        public int StudentId { get; set; }
        public double Value { get; set; }
    }

    /// <summary>
    /// Body of POST /api/grades. Sent only by Parser Service after it has
    /// already confirmed the course exists and validated the Excel file.
    /// </summary>
    public class BatchGradeUploadDto
    {
        public int CourseId { get; set; }
        public List<GradeRecordDto> Grades { get; set; } = new();
    }

    /// <summary>
    /// Shape returned by the internal read endpoints
    /// (GET /api/grades/course/{id}, GET /api/grades/student/{id}).
    /// Consumed only by Analytics Service — never by the frontend directly.
    /// </summary>
    public class GradeDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public double Value { get; set; }
    }

    /// <summary>Response for POST /api/grades.</summary>
    public class BatchGradeUploadResultDto
    {
        public int CourseId { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
    }
}
