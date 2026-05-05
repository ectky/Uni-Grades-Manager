namespace AuthService.App.Clients.Contracts
{
    public interface IUserServiceClient
    {
        public Task<bool> CanUserLogin(string username, string password);
    }
}
