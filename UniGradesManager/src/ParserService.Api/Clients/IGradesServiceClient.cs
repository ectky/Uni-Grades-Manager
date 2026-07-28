using ParserService.Dtos;

namespace ParserService.Clients
{
    /// <summary>
    /// Write-only client to Grades Service. POST /api/grades is internal —
    /// only Parser Service is authorized to call it (service-to-service auth,
    /// not exposed to the React frontend).
    /// </summary>
    public interface IGradesServiceClient
    {
        Task UploadGradesAsync(BatchGradeUploadDto batch, CancellationToken ct = default);
    }
}
