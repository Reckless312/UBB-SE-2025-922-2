using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;

namespace DataAccess.Service.Authentication.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> AuthWithUserPass(string username, string password);
        Task<AuthenticationResponse> AuthWithOAuth(OAuthService selectedService, object authProvider);
        Task<User?> GetUser(Guid sessionId);

        void Logout();
    }
}