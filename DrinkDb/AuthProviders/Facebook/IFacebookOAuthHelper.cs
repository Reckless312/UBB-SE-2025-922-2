using System.Threading.Tasks;
using DrinkDb_Auth.OAuthProviders;

namespace DrinkDb_Auth.AuthProviders.Facebook
{
    public interface IFacebookOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}