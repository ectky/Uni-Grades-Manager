namespace AnalyticsService.Exceptions
{
    public class CourseNotFoundException : Exception
    {
        public CourseNotFoundException(int courseId)
            : base($"Course with id {courseId} was not found.") { }
    }

    /// <summary>
    /// Thrown by GetMyStatsAsync/GetMyGradesAsync when the logged-in user's
    /// account has no User.StudentId set — shouldn't happen for a real
    /// Student account, but guards against a misconfigured one instead of
    /// silently returning empty/zeroed data that looks like "no grades yet".
    /// </summary>
    public class NoStudentRollNumberException : Exception
    {
        public NoStudentRollNumberException(int userId)
            : base($"User {userId} has no student roll number (User.StudentId) assigned.") { }
    }
}
