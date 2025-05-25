using DataAccess.OAuthProviders;

namespace DataAccess.AuthProviders.Github
{
    public interface IGitHubOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}