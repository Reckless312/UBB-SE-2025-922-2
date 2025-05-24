using System.Diagnostics;
using System.Text.Json;
using DataAccess.OAuthProviders;

namespace DataAccess.AuthProviders.LinkedIn
{
    public class LinkedInOAuthHelper : ILinkedInOAuthHelper
    {
        private readonly string clientId = "86j0ikb93jm78x";
        private readonly string clientSecret = "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==";
        private readonly string redirectUrl = "http://localhost:8891/auth";
        private readonly string scope = "openid profile email";
        private TaskCompletionSource<AuthenticationResponse>? taskCompletionSource;
        private LinkedInOAuth2Provider linkedInOAuth2Provider;

        public LinkedInOAuthHelper(string clientId, string clientSecret, string redirectUri, string scope, LinkedInOAuth2Provider provider)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirectUrl = redirectUri;
            this.scope = scope;
            LinkedInLocalOAuthServer.OnCodeReceived += OnCodeReceived;
            this.linkedInOAuth2Provider = provider;
        }

        private string BuildAuthorizeUrl()
        {
            string url = $"https://www.linkedin.com/oauth/v2/authorization" +
                      $"?response_type=code" +
                      $"&client_id={this.clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(this.redirectUrl)}" +
                      $"&scope={Uri.EscapeDataString(this.scope)}";
            Debug.WriteLine("Authorize URL: " + url);
            return url;
        }

        private async void OnCodeReceived(string code)
        {
            Debug.WriteLine("OnCodeReceived called with code: " + code);
            if (this.taskCompletionSource == null || this.taskCompletionSource.Task.IsCompleted)
            {
                Debug.WriteLine("TaskCompletionSource is null or already completed.");
                return;
            }

            try
            {
                string token = await this.ExchangeCodeForToken(code);
                AuthenticationResponse response = await this.linkedInOAuth2Provider.Authenticate(string.Empty, token);
                Debug.WriteLine("Authentication response received.");
                this.taskCompletionSource.SetResult(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in OnCodeReceived: " + ex);
                this.taskCompletionSource.SetException(ex);
            }
        }

        public async Task<AuthenticationResponse> AuthenticateAsync()
        {
            this.taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();

            Uri authorizeUri = new Uri(BuildAuthorizeUrl());
            Process.Start(new ProcessStartInfo
            {
                FileName = authorizeUri.ToString(),
                UseShellExecute = true
            });

            return await this.taskCompletionSource.Task;
        }

        private async Task<string> ExchangeCodeForToken(string code)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken");
                request.Headers.Add("Accept", "application/json");
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", this.redirectUrl),
                    new KeyValuePair<string, string>("client_id", this.clientId),
                    new KeyValuePair<string, string>("client_secret", this.clientSecret)
                });
                request.Content = content;

                HttpResponseMessage response = await client.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(body);
                if (document.RootElement.TryGetProperty("access_token", out var tokenProp))
                {
                    return tokenProp.GetString() ?? throw new Exception("Token is null");
                }
                throw new Exception("Token not found in response");
            }
        }
    }
}
