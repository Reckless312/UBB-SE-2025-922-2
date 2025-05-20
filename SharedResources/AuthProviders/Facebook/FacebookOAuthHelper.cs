using System;
using System.Threading.Tasks;
using DataAccess.AuthProviders.Facebook;
using DataAccess.OAuthProviders;
using Repository.Authentication;

namespace DataAccess.AuthProviders.Facebook
{
    public class FacebookOAuthHelper : IFacebookOAuthHelper
    {
        private static readonly FacebookOAuth2Provider FacebookOAuth2Provider;
        private static readonly SessionRepository SessionAdapter;

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

        public FacebookOAuthHelper()
        {
            taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();
            FacebookLocalOAuthServer.OnTokenReceived += OnTokenReceived;
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
