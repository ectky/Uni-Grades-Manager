using GradeService.Dtos;

namespace GradeService.Services
{
    public interface IGradeService
    {
        Task<BatchGradeUploadResultDto> UploadGradesAsync(BatchGradeUploadDto batch, CancellationToken ct = default);
        Task<List<GradeDto>> GetByCourseAsync(int courseId, CancellationToken ct = default);
        Task<List<GradeDto>> GetByStudentAsync(int studentId, CancellationToken ct = default);
    }
}
