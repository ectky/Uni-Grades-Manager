using AuthService.App.Clients.Contracts;
using AuthService.App.Services.Contracts;
using RestSharp;

namespace AuthService.App.Services
{
    public class UserService : IUserService
    {
        private readonly IUserServiceClient _userServiceClient;

        public UserService(IUserServiceClient userServiceClient)
        {
            _userServiceClient = userServiceClient;
        }

        public async Task<bool> CanUserLoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            var result = await _userServiceClient.CanUserLogin(username, password);
            return result;
        }
    }
}
