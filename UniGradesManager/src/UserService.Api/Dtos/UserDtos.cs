namespace UserService.Dtos
{
    /// <summary>Returned by GET /api/users/{id}. Never includes the password hash.</summary>
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    /// <summary>
    /// Minimal shape for GET /api/users/batch — only what Analytics Service
    /// needs. Two different jobs use this same endpoint now:
    ///   1. Turning a bare StudentId (roll number) into a readable name
    ///      (Приложение №1: /api/analytics/course/{id}/grades).
    ///   2. Resolving a logged-in Student's own roll number from their
    ///      JWT's User.Id, so "my grades" works without the frontend ever
    ///      needing to know or guess that number (see AnalyticsService.Api's
    ///      README for why the two are different values).
    /// Deliberately excludes Email — Analytics has no legitimate need for
    /// it, and every field returned from an internal endpoint is one more
    /// thing to leak if that endpoint is ever reached by the wrong caller.
    /// </summary>
    public class UserBatchItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int? StudentId { get; set; }
    }
}
