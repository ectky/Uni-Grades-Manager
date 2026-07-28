using UserService.Domain.Entities;

namespace UserService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<List<User>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task<User> AddAsync(User user, CancellationToken ct = default);
    }
}
