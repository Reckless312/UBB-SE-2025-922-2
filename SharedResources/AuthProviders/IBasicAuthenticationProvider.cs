namespace DataAccess.AuthProviders
{
    public interface IBasicAuthenticationProvider
    {
        abstract bool Authenticate(string username, string password);
    }
}