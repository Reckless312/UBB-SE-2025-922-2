using System;
using System.Threading.Tasks;
using DrinkDb_Auth.OAuthProviders;

namespace DrinkDb_Auth.AuthProviders.Google
{
    public interface IGoogleOAuth2Provider
    {
        AuthenticationResponse Authenticate(string userId, string token);
        Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code);
        string GetAuthorizationUrl();
        Task<AuthenticationResponse> SignInWithGoogleAsync(Window parentWindow);
    }
}