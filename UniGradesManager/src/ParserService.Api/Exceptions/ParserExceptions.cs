namespace ParserService.Exceptions
{
    /// <summary>
    /// Thrown when the uploaded Excel file is missing required columns
    /// (StudentId, Value), has an unreadable structure, or contains no data rows.
    /// </summary>
    public class InvalidExcelStructureException : Exception
    {
        public InvalidExcelStructureException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when the CourseId supplied with the upload does not exist
    /// according to Courses Service. Parser Service only checks existence —
    /// it never creates or edits courses.
    /// </summary>
    public class CourseNotFoundException : Exception
    {
        public CourseNotFoundException(int courseId)
            : base($"Course with id {courseId} was not found.") { }
    }

    /// <summary>
    /// Thrown when the uploaded file exceeds the 10 MB limit described in Chapter 4.4.
    /// </summary>
    public class FileTooLargeException : Exception
    {
        public FileTooLargeException(long sizeBytes, long maxBytes)
            : base($"File size {sizeBytes} bytes exceeds the {maxBytes} byte limit.") { }
    }

    /// <summary>
    /// Thrown when Grades Service rejects or fails to persist the batch of grades.
    /// </summary>
    public class GradesUploadFailedException : Exception
    {
        public GradesUploadFailedException(string message) : base(message) { }
    }
}
