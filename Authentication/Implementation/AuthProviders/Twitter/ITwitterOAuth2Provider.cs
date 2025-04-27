using System;
using System.Threading.Tasks;
using DrinkDb_Auth.OAuthProviders;
using Microsoft.UI.Xaml;

namespace DrinkDb_Auth.AuthProviders.Twitter
{
    public interface ITwitterOAuth2Provider
    {
        Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code);
        string GetAuthorizationUrl();
        Task<AuthenticationResponse> SignInWithTwitterAsync(Window parentWindow);
    }
}