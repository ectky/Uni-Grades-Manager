using CourseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Data
{
    public class CoursesDbContext : DbContext
    {
        public CoursesDbContext(DbContextOptions<CoursesDbContext> options) : base(options) { }

        public DbSet<Course> Courses => Set<Course>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.Period)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(c => c.InstructorId);
            });
        }
    }
}
