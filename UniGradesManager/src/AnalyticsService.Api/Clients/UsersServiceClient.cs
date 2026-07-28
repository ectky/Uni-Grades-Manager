using System.Net.Http.Json;
using System.Web;
using AnalyticsService.Dtos;

namespace AnalyticsService.Clients
{
    /// <summary>
    /// Analytics Service's only path to human-readable names — StudentId/
    /// InstructorId values from Grades Service mean nothing on their own.
    /// Also the only path to resolving a logged-in Student's own roll number
    /// (User.StudentId) from their JWT identity (User.Id) — those are two
    /// different numbers; see AnalyticsService.Api/README.md.
    /// Calls User Service's internal-only GET /api/users/batch.
    /// </summary>
    public interface IUsersServiceClient
    {
        Task<Dictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

        /// <summary>Returns null if the user has no StudentId (e.g. an Instructor or Admin account).</summary>
        Task<int?> GetStudentRollNumberAsync(int userId, CancellationToken ct = default);
    }

    public class UsersServiceClient : IUsersServiceClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersServiceClient> _logger;

        public UsersServiceClient(HttpClient http, IConfiguration configuration, ILogger<UsersServiceClient> logger)
        {
            _http = http;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Dictionary<int, string>> GetNamesByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default)
        {
            var users = await GetBatchAsync(ids, ct);
            return users.ToDictionary(u => u.Id, u => u.Name);
        }

        public async Task<int?> GetStudentRollNumberAsync(int userId, CancellationToken ct = default)
        {
            var users = await GetBatchAsync(new[] { userId }, ct);
            return users.FirstOrDefault(u => u.Id == userId)?.StudentId;
        }

        private async Task<List<UserBatchItemDto>> GetBatchAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var distinctIds = ids.Distinct().ToList();
            if (distinctIds.Count == 0) return new List<UserBatchItemDto>();

            var idsParam = HttpUtility.UrlEncode(string.Join(",", distinctIds));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/batch?ids={idsParam}");
            request.Headers.Add(InternalApiKeyHeader, _configuration["Internal:ApiKey"]);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("User Service returned {StatusCode} for batch lookup", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            return await response.Content.ReadFromJsonAsync<List<UserBatchItemDto>>(cancellationToken: ct)
                ?? new List<UserBatchItemDto>();
        }
    }
}
