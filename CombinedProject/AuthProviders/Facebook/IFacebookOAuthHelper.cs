using System.Threading.Tasks;
using CombinedProject.OAuthProviders;

namespace CombinedProject.AuthProviders.Facebook
{
    public interface IFacebookOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}