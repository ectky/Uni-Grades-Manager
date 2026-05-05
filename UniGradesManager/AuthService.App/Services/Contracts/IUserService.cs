namespace AuthService.App.Services.Contracts
{
    public interface IUserService
    {
        Task<bool> CanUserLoginAsync(string username, string password);
    }
}
