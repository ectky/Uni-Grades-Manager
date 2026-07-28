namespace GradeService.Exceptions
{
    /// <summary>
    /// Thrown when a batch from Parser Service fails Grades Service's own
    /// validation — defense in depth, since Grades Service should not blindly
    /// trust that Parser Service already validated everything correctly.
    /// </summary>
    public class InvalidGradeBatchException : Exception
    {
        public InvalidGradeBatchException(string message) : base(message) { }
    }
}
