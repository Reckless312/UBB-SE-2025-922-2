using DataAccess.OAuthProviders;

namespace DataAccess.AuthProviders.Twitter
{
    public interface ITwitterOAuth2Provider
    {
        Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code);
        string GetAuthorizationUrl();
        Task<AuthenticationResponse> SignInWithTwitterAsync();
    }
}