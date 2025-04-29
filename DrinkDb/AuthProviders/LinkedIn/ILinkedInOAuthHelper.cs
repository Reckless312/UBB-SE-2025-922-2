using DrinkDb_Auth.OAuthProviders;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.LinkedIn
{
    public interface ILinkedInOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}