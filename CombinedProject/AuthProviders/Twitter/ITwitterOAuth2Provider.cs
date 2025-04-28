using System;
using System.Threading.Tasks;
using CombinedProject.OAuthProviders;
using Microsoft.UI.Xaml;

namespace CombinedProject.AuthProviders.Twitter
{
    public interface ITwitterOAuth2Provider
    {
        Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code);
        string GetAuthorizationUrl();
        Task<AuthenticationResponse> SignInWithTwitterAsync(Window parentWindow);
    }
}