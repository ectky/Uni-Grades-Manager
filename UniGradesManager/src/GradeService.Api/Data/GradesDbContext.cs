using GradeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradeService.Data
{
    public class GradesDbContext : DbContext
    {
        public GradesDbContext(DbContextOptions<GradesDbContext> options) : base(options) { }

        public DbSet<Grade> Grades => Set<Grade>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grades");
                entity.HasKey(g => g.Id);

                // NOTE: Value is a C# double, so EF Core maps it to SQL Server's float(53)
                // by default. Приложение №2 earlier described this column as decimal(4,2) —
                // that mismatch is still open; forcing HasColumnType("decimal(4,2)") here
                // without a value converter would silently corrupt data on save/read.
                // If you want true decimal storage, change the entity property to
                // `decimal Value` first, then map it — don't convert only on the DB side.

                // One grade per student per course — re-uploading an Excel file
                // for the same course overwrites the existing value (Chapter 3.6),
                // it never creates a duplicate row.
                entity.HasIndex(g => new { g.CourseId, g.StudentId })
                    .IsUnique();

                entity.HasIndex(g => g.CourseId);
                entity.HasIndex(g => g.StudentId);
            });
        }
    }
}
