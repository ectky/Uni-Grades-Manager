using GradeService.Data;
using GradeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradeService.Repositories
{
    public class GradeRepository : IGradeRepository
    {
        private readonly GradesDbContext _db;

        public GradeRepository(GradesDbContext db)
        {
            _db = db;
        }

        public async Task<(int inserted, int updated)> UpsertManyAsync(
            int courseId, IEnumerable<(int StudentId, double Value)> records, CancellationToken ct = default)
        {
            var recordList = records.ToList();
            var studentIds = recordList.Select(r => r.StudentId).ToList();

            // Aspire's SQL Server integration enables a retrying execution
            // strategy (SqlServerRetryingExecutionStrategy) by default. Two
            // things follow from that:
            //   1. A manually-opened BeginTransactionAsync() outside of
            //      CreateExecutionStrategy().ExecuteAsync throws "does not
            //      support user-initiated transactions" the moment a
            //      transient fault makes the strategy want to retry.
            //   2. If the strategy *does* retry, it re-runs this whole
            //      delegate from scratch — so the "which rows already exist"
            //      query and the change-tracking Add() calls both need to be
            //      inside it too. Loading `existing` once outside, then
            //      retrying only the save, would risk re-adding rows that a
            //      first (failed) attempt already queued on the tracked
            //      DbContext, producing duplicates.
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var existing = await _db.Grades
                    .Where(g => g.CourseId == courseId && studentIds.Contains(g.StudentId))
                    .ToDictionaryAsync(g => g.StudentId, ct);

                int insertedCount = 0, updatedCount = 0;

                await using var transaction = await _db.Database.BeginTransactionAsync(ct);

                foreach (var record in recordList)
                {
                    if (existing.TryGetValue(record.StudentId, out var grade))
                    {
                        grade.Value = record.Value;
                        updatedCount++;
                    }
                    else
                    {
                        _db.Grades.Add(new Grade
                        {
                            CourseId = courseId,
                            StudentId = record.StudentId,
                            Value = record.Value,
                        });
                        insertedCount++;
                    }
                }

                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return (insertedCount, updatedCount);
            });
        }

        public Task<List<Grade>> GetByCourseAsync(int courseId, CancellationToken ct = default) =>
            _db.Grades.AsNoTracking().Where(g => g.CourseId == courseId).ToListAsync(ct);

        public Task<List<Grade>> GetByStudentAsync(int studentId, CancellationToken ct = default) =>
            _db.Grades.AsNoTracking().Where(g => g.StudentId == studentId).ToListAsync(ct);
    }
}