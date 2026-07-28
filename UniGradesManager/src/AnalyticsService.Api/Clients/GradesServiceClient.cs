using System.Net.Http.Json;
using AnalyticsService.Dtos;

namespace AnalyticsService.Clients
{
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

        public Task<List<RawGradeDto>> GetByCourseAsync(int courseId, CancellationToken ct = default) =>
            GetAsync($"/api/grades/course/{courseId}", ct);

        public Task<List<RawGradeDto>> GetByStudentAsync(int studentId, CancellationToken ct = default) =>
            GetAsync($"/api/grades/student/{studentId}", ct);

        private async Task<List<RawGradeDto>> GetAsync(string path, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add(InternalApiKeyHeader, _configuration["Internal:ApiKey"]);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Grades Service returned {StatusCode} for {Path}", response.StatusCode, path);
                response.EnsureSuccessStatusCode();
            }

            return await response.Content.ReadFromJsonAsync<List<RawGradeDto>>(cancellationToken: ct)
                ?? new List<RawGradeDto>();
        }
    }
}
