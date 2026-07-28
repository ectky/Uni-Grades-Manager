using UserService.Domain.Entities;
using UserService.Dtos;
using UserService.Exceptions;
using UserService.Repositories;

namespace UserService.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
        Task<UserProfileDto> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository repository, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _repository = repository;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
        {
            var user = await _repository.GetByEmailAsync(dto.Email, ct);

            // Same exception whether the email doesn't exist or the password is
            // wrong — never let a caller distinguish "unknown email" from
            // "wrong password" (Chapter 4.5).
            if (user is null || !user.VerifyPassword(dto.Password))
            {
                throw new InvalidCredentialsException();
            }

            var token = _tokenService.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Role = user.Type.ToString(),
            };
        }

        public async Task<UserProfileDto> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
        {
            if (await _repository.EmailExistsAsync(dto.Email, ct))
            {
                throw new EmailAlreadyExistsException(dto.Email);
            }

            var user = new User(dto.Name, dto.Email, dto.Password, dto.Type, dto.StudentId);
            var created = await _repository.AddAsync(user, ct);

            _logger.LogInformation(
                "User {UserId} registered with role {Role}", created.Id, created.Type);

            return new UserProfileDto
            {
                Id = created.Id,
                Name = created.Name,
                Email = created.Email,
                Role = created.Type.ToString(),
            };
        }
    }
}
