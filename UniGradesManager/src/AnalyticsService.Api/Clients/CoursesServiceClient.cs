using System.Net;
using System.Net.Http.Json;
using AnalyticsService.Dtos;

namespace AnalyticsService.Clients
{
    /// <summary>
    /// Analytics Service's only path to a course's name/period — Grades
    /// Service only knows a bare CourseId. Calls Courses Service's
    /// internal-only GET /api/courses/{id}.
    /// </summary>
    public interface ICoursesServiceClient
    {
        Task<CourseSummaryDto?> GetCourseAsync(int courseId, CancellationToken ct = default);
    }

    public class CoursesServiceClient : ICoursesServiceClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CoursesServiceClient> _logger;

        public CoursesServiceClient(HttpClient http, IConfiguration configuration, ILogger<CoursesServiceClient> logger)
        {
            _http = http;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CourseSummaryDto?> GetCourseAsync(int courseId, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/courses/{courseId}");
            request.Headers.Add(InternalApiKeyHeader, _configuration["Internal:ApiKey"]);

            var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Courses Service returned {StatusCode} for course {CourseId}", response.StatusCode, courseId);
                response.EnsureSuccessStatusCode();
            }

            return await response.Content.ReadFromJsonAsync<CourseSummaryDto>(cancellationToken: ct);
        }
    }
}
