using System;
using System.Threading.Tasks;
using DataAccess.AuthProviders.Facebook;
using DataAccess.OAuthProviders;
using Repository.Authentication;

namespace DataAccess.AuthProviders.Facebook
{
    public class FacebookOAuthHelper : IFacebookOAuthHelper
    {
        private FacebookOAuth2Provider FacebookOAuth2Provider;

        private const string ClientId = "667671795847732";
        private string redirectUri = "http://localhost:8888/auth";
        private const string Scope = "email";
        private string BuildAuthorizeUrl()
        {
            Console.WriteLine($"RedirectUri: {redirectUri}");
            return $"https://www.facebook.com/v22.0/dialog/oauth?client_id={ClientId}" +
                   $"&display=popup" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&response_type=token&scope={Scope}";
        }

        private TaskCompletionSource<AuthenticationResponse> taskCompletionSource;

        public FacebookOAuthHelper(FacebookOAuth2Provider provider)
        {
            taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();
            FacebookLocalOAuthServer.OnTokenReceived += OnTokenReceived;
            this.FacebookOAuth2Provider = provider;
        }

        private void OnTokenReceived(string accessToken)
        {
            if (taskCompletionSource != null && !taskCompletionSource.Task.IsCompleted)
            {
                AuthenticationResponse res = FacebookOAuth2Provider.Authenticate(string.Empty, accessToken);
                taskCompletionSource.TrySetResult(res);
            }
        }

        public async Task<AuthenticationResponse> AuthenticateAsync()
        {
            taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();

            var authorizeUri = new Uri(BuildAuthorizeUrl());

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = authorizeUri.ToString(),
                UseShellExecute = true
            });

            AuthenticationResponse response = await taskCompletionSource.Task;
            return response;
        }
    }
}
