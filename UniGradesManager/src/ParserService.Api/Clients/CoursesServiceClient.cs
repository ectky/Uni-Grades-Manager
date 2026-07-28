using System.Net;
using System.Net.Http.Json;
using ParserService.Dtos;

namespace ParserService.Clients
{
    /// <summary>
    /// Calls Courses Service through the logical "courses-service" endpoint name.
    /// .NET Aspire's service discovery resolves this to the real host:port at runtime —
    /// see Program.cs, where the HttpClient is registered with base address
    /// "https+http://courses-service". The endpoint itself is internal-only
    /// (Courses Service's InternalOnlyAttribute), so every call carries the
    /// shared "X-Internal-Api-Key" header, same convention as GradesServiceClient.
    /// </summary>
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
                _logger.LogError(
                    "Courses Service returned {StatusCode} while checking course {CourseId}",
                    response.StatusCode, courseId);
                response.EnsureSuccessStatusCode();
            }

            return await response.Content.ReadFromJsonAsync<CourseSummaryDto>(cancellationToken: ct);
        }
    }
}
