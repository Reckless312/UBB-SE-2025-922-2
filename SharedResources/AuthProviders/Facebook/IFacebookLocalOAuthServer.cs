namespace DataAccess.AuthProviders.Facebook
{
    public interface IFacebookLocalOAuthServer
    {
        Task StartAsync();
        void Stop();
    }
}