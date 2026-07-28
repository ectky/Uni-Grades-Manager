namespace UserService.Exceptions
{
    /// <summary>
    /// Thrown for both "no such email" and "wrong password" — deliberately
    /// the same exception/message for both, so the controller can't
    /// accidentally leak which one it was (Chapter 4.5, avoids confirming
    /// which emails are registered).
    /// </summary>
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base("Invalid email or password.") { }
    }

    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string email)
            : base($"A user with email '{email}' already exists.") { }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(int userId) : base($"User with id {userId} was not found.") { }
    }

    /// <summary>Thrown when a non-Admin tries to view a profile that isn't their own.</summary>
    public class ForbiddenProfileAccessException : Exception
    {
        public ForbiddenProfileAccessException(int requestedUserId)
            : base($"Not authorized to view user {requestedUserId}'s profile.") { }
    }
}
