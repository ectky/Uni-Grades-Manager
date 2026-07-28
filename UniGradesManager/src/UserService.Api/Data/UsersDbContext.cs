using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Data
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name).IsRequired().HasMaxLength(200);

                entity.Property(u => u.Email).IsRequired().HasMaxLength(320);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Password).IsRequired();

                entity.Property(u => u.Type)
                    .HasConversion<string>() // store "Student"/"Instructor"/"Admin", not 0/1/2
                    .HasMaxLength(20);
            });
        }
    }
}
