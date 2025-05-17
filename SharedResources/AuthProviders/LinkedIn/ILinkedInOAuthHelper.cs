using DataAccess.OAuthProviders;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.LinkedIn
{
    public interface ILinkedInOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}