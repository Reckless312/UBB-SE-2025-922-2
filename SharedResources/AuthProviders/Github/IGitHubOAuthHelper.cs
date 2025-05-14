using DataAccess.OAuthProviders;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.Github
{
    public interface IGitHubOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}