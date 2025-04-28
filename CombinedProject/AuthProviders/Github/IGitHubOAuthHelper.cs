using CombinedProject.OAuthProviders;
using System.Threading.Tasks;

namespace CombinedProject.AuthProviders.Github
{
    public interface IGitHubOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}