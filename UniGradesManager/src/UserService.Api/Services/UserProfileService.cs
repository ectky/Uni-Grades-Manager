using UserService.Dtos;
using UserService.Exceptions;
using UserService.Repositories;

namespace UserService.Services
{
    public interface IUserProfileService
    {
        /// <summary>
        /// requestingUserId/requestingRole enforce "Admin, or the profile's
        /// own owner" (Приложение №1: GET /api/users/{id} — Admin, Owner).
        /// </summary>
        Task<UserProfileDto> GetProfileAsync(
            int userId, int requestingUserId, string requestingRole, CancellationToken ct = default);

        /// <summary>Used only by Analytics Service to enrich StudentId lists with names.</summary>
        Task<List<UserBatchItemDto>> GetBatchAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }

    public class UserProfileService : IUserProfileService
    {
        private const string AdminRole = "Admin";

        private readonly IUserRepository _repository;

        public UserProfileService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserProfileDto> GetProfileAsync(
            int userId, int requestingUserId, string requestingRole, CancellationToken ct = default)
        {
            if (requestingRole != AdminRole && requestingUserId != userId)
            {
                throw new ForbiddenProfileAccessException(userId);
            }

            var user = await _repository.GetByIdAsync(userId, ct)
                ?? throw new UserNotFoundException(userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Type.ToString(),
            };
        }

        public async Task<List<UserBatchItemDto>> GetBatchAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var distinctIds = ids.Distinct().ToList();
            var users = await _repository.GetByIdsAsync(distinctIds, ct);

            return users
                .Select(u => new UserBatchItemDto { Id = u.Id, Name = u.Name, StudentId = u.StudentId })
                .ToList();
        }
    }
}
