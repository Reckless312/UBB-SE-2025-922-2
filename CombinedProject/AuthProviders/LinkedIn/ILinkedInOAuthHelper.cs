using CombinedProject.OAuthProviders;
using System.Threading.Tasks;

namespace CombinedProject.AuthProviders.LinkedIn
{
    public interface ILinkedInOAuthHelper
    {
        Task<AuthenticationResponse> AuthenticateAsync();
    }
}