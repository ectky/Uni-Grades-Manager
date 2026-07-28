using Microsoft.AspNetCore.Http;

namespace ParserService.Dtos
{
    /// <summary>
    /// Multipart/form-data request for POST /api/parse.
    /// CourseId is supplied by the Instructor's client (selected from GET /api/courses/my),
    /// since the Excel file itself only carries StudentId + Value columns.
    /// </summary>
    public class ParseGradesRequest
    {
        public int CourseId { get; set; }
        public IFormFile File { get; set; } = default!;
    }

    /// <summary>
    /// One successfully parsed row from the Excel file.
    /// </summary>
    public class GradeRecordDto
    {
        public int StudentId { get; set; }
        public double Value { get; set; }
    }

    /// <summary>
    /// Response returned after a file has been parsed and the grades written
    /// to Grades Service. Mirrors the confirmation screen (Fig. 3.6).
    /// </summary>
    public class ParseResultDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = default!;
        public string Period { get; set; } = default!;
        public int RecordCount { get; set; }
        public long ProcessingTimeMs { get; set; }
        public List<GradeRecordDto> Records { get; set; } = new();
    }

    /// <summary>
    /// Payload sent internally from Parser Service to Grades Service (POST /api/grades).
    /// Batched to avoid one HTTP round-trip per student.
    /// </summary>
    public class BatchGradeUploadDto
    {
        public int CourseId { get; set; }
        public List<GradeRecordDto> Grades { get; set; } = new();
    }

    /// <summary>
    /// Minimal shape Parser Service needs back from Courses Service
    /// to confirm the course exists and to enrich the response with a readable name/period.
    /// </summary>
    public class CourseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Period { get; set; } = default!;
        public int InstructorId { get; set; }
    }
}
