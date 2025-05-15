using System;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using Windows.UI.Xaml;

namespace DataAccess.Service.Authentication.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> AuthWithUserPass(string username, string password);
        Task<AuthenticationResponse> AuthWithOAuth(Window window, OAuthService selectedService, object authProvider);
        Task<User> GetUser(Guid sessionId);

        void Logout();
    }
}