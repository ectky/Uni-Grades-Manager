namespace CourseService.Exceptions
{
    public class CourseNotFoundException : Exception
    {
        public CourseNotFoundException(int courseId)
            : base($"Course with id {courseId} was not found.") { }
    }

    /// <summary>
    /// Thrown when an Instructor tries to edit a course they don't own.
    /// Admin bypasses this check entirely (see CourseService.UpdateCourseAsync).
    /// </summary>
    public class ForbiddenCourseAccessException : Exception
    {
        public ForbiddenCourseAccessException(int courseId, int instructorId)
            : base($"Instructor {instructorId} does not own course {courseId}.") { }
    }

    public class InvalidCourseException : Exception
    {
        public InvalidCourseException(string message) : base(message) { }
    }
}
