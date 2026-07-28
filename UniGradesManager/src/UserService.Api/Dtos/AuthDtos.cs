using UserService.Domain.Enums;

namespace UserService.Dtos
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = default!;
        public int UserId { get; set; }
        public string Name { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    /// <summary>
    /// Body of POST /api/auth/register. Admin-only (Приложение №1) — an Admin
    /// assigns the role explicitly; there is no self-service sign-up.
    /// </summary>
    public class RegisterUserDto
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public UserType Type { get; set; }
        public int? StudentId { get; set; }
    }
}
