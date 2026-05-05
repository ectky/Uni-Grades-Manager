using AuthService.App.Clients.Contracts;
using RestSharp;
using RestSharp.Authenticators;

namespace AuthService.App.Clients
{
    public class UserServiceClient : IUserServiceClient, IDisposable
    {
        private readonly RestClient _client;

        public UserServiceClient(IConfiguration configuration)
        {
            var options = new RestClientOptions(configuration["UserService:BaseUrl"])
            {
                Authenticator = new JwtAuthenticator(configuration["Secrets:JWTToken"])
            };
            _client = new RestClient(options);
        }

        public async Task<bool> CanUserLogin(string username, string password)
        {
            var response = await _client.GetAsync<bool>(
                "/Users/can-user-login",
                new { username, password });

            return response;
        }

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
