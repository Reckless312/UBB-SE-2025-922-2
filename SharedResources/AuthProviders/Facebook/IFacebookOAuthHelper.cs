using System.Threading.Tasks;
using DataAccess.OAuthProviders;

namespace DataAccess.AuthProviders.Facebook
{
    public interface IFacebookOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}