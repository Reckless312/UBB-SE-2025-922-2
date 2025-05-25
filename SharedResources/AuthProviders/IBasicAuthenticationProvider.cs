namespace DataAccess.AuthProviders
{
    public interface IBasicAuthenticationProvider
    {
        bool Authenticate(string username, string password);
        Task<bool> AuthenticateAsync(string username, string password);
    }
}