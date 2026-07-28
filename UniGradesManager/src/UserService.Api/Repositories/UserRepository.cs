using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Domain.Entities;

namespace UserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _db;

        public UserRepository(UsersDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<List<User>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
            _db.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).ToListAsync(ct);

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
            _db.Users.AnyAsync(u => u.Email == email, ct);

        public async Task<User> AddAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return user;
        }
    }
}
