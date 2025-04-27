using DrinkDb_Auth.OAuthProviders;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public interface IGitHubOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}