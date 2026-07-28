using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Exceptions;
using UserService.Services;

namespace UserService.Controllers
{
    /// <summary>
    /// Приложение №1, User Service section:
    ///   POST /api/auth/login     — Public
    ///   POST /api/auth/register  — Admin
    /// There is no self-service sign-up — an Admin creates every account and
    /// assigns its role explicitly (decided earlier in this project).
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _authService.LoginAsync(dto, ct);
                return Ok(result);
            }
            catch (InvalidCredentialsException)
            {
                // Deliberately generic — see InvalidCredentialsException's own comment.
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid credentials",
                    Detail = "Invalid email or password.",
                    Status = StatusCodes.Status401Unauthorized,
                });
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserProfileDto>> Register([FromBody] RegisterUserDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _authService.RegisterAsync(dto, ct);
                return CreatedAtAction(
                    nameof(UsersController.GetProfile), "Users", new { id = created.Id }, created);
            }
            catch (EmailAlreadyExistsException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Email already registered",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                });
            }
        }
    }
}
