using DataAccess.OAuthProviders;

namespace DataAccess.AuthProviders.Facebook
{
    public class FacebookOAuthHelper : IFacebookOAuthHelper
    {
        private FacebookOAuth2Provider facebookOAuth2Provider;

        private const string ClientId = "667671795847732";
        private string redirectUri = "http://localhost:8888/auth";
        private const string Scope = "email";

        private TaskCompletionSource<AuthenticationResponse> taskCompletionSource;

        public FacebookOAuthHelper(FacebookOAuth2Provider provider)
        {
            this.taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();
            FacebookLocalOAuthServer.OnTokenReceived += OnTokenReceived;
            this.facebookOAuth2Provider = provider;
        }

        private async void OnTokenReceived(string accessToken)
        {
            if (this.taskCompletionSource != null && !this.taskCompletionSource.Task.IsCompleted)
            {
                AuthenticationResponse response = await this.facebookOAuth2Provider.Authenticate(string.Empty, accessToken);
                this.taskCompletionSource.TrySetResult(response);
            }
        }

        public async Task<AuthenticationResponse> AuthenticateAsync()
        {
            this.taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();

            Uri authorizeUri = new Uri(BuildAuthorizeUrl());

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = authorizeUri.ToString(),
                UseShellExecute = true
            });

            AuthenticationResponse response = await taskCompletionSource.Task;
            return response;
        }

        private string BuildAuthorizeUrl()
        {
            Console.WriteLine($"RedirectUri: {redirectUri}");
            return $"https://www.facebook.com/v22.0/dialog/oauth?client_id={ClientId}" +
                   $"&display=popup" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&response_type=token&scope={Scope}";
        }
    }
}
