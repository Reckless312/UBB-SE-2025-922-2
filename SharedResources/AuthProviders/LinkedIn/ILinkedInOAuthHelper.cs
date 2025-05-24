using DataAccess.OAuthProviders;

namespace DataAccess.AuthProviders.LinkedIn
{
    public interface ILinkedInOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}