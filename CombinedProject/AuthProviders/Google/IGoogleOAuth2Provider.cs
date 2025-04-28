using System;
using System.Threading.Tasks;
using CombinedProject.OAuthProviders;
using Microsoft.UI.Xaml;

namespace CombinedProject.AuthProviders.Google
{
    public interface IGoogleOAuth2Provider
    {
        AuthenticationResponse Authenticate(string userId, string token);
        Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code);
        string GetAuthorizationUrl();
        Task<AuthenticationResponse> SignInWithGoogleAsync(Window parentWindow);
    }
}