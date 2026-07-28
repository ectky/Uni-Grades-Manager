using System.Net.Http.Json;
using ParserService.Dtos;
using ParserService.Exceptions;

namespace ParserService.Clients
{
    /// <summary>
    /// Calls Grades Service through the logical "grades-service" endpoint name,
    /// resolved by .NET Aspire service discovery. Grades Service's endpoints are
    /// internal-only (see its InternalOnlyAttribute) and expect the shared
    /// "X-Internal-Api-Key" header — configured identically on both services
    /// via Aspire (Internal:ApiKey), not tied to the instructor's own JWT.
    /// </summary>
    public class GradesServiceClient : IGradesServiceClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GradesServiceClient> _logger;

        public GradesServiceClient(HttpClient http, IConfiguration configuration, ILogger<GradesServiceClient> logger)
        {
            _http = http;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task UploadGradesAsync(BatchGradeUploadDto batch, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/grades")
            {
                Content = JsonContent.Create(batch),
            };
            request.Headers.Add(InternalApiKeyHeader, _configuration["Internal:ApiKey"]);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Grades Service rejected batch for course {CourseId}: {StatusCode} {Body}",
                    batch.CourseId, response.StatusCode, body);

                throw new GradesUploadFailedException(
                    $"Grades Service returned {response.StatusCode} while uploading {batch.Grades.Count} grade(s).");
            }
        }
    }
}
