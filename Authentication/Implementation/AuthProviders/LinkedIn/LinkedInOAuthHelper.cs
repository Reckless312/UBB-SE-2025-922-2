using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.OAuthProviders;

namespace DrinkDb_Auth.AuthProviders.LinkedIn
{
    public class LinkedInOAuthHelper : ILinkedInOAuthHelper
    {
        private readonly string clientId = "86j0ikb93jm78x";
        private readonly string clientSecret = "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==";
        private readonly string redirectUrl = "http://localhost:8891/auth";
        private readonly string scope = "openid profile email";
        private TaskCompletionSource<AuthenticationResponse>? taskCompletionSource;
        private readonly UserAdapter userAdapter = new UserAdapter();
        private readonly static LinkedInOAuth2Provider LinkedInOAuth2Provider = new ();

        public LinkedInOAuthHelper(string clientId, string clientSecret, string redirectUri, string scope)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            redirectUrl = redirectUri;
            this.scope = scope;
            LinkedInLocalOAuthServer.OnCodeReceived += OnCodeReceived;
        }

        private string BuildAuthorizeUrl()
        {
            var url = $"https://www.linkedin.com/oauth/v2/authorization" +
                      $"?response_type=code" +
                      $"&client_id={clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUrl)}" +
                      $"&scope={Uri.EscapeDataString(scope)}";
            Debug.WriteLine("Authorize URL: " + url);
            return url;
        }

        private async void OnCodeReceived(string code)
        {
            if (taskCompletionSource == null || taskCompletionSource.Task.IsCompleted)
            {
                return;
            }

            try
            {
                var token = await ExchangeCodeForToken(code);
                var response = LinkedInOAuth2Provider.Authenticate(string.Empty, token);
                taskCompletionSource.SetResult(response);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        }

        public async Task<AuthenticationResponse> AuthenticateAsync()
        {
            taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();

            var authorizeUri = new Uri(BuildAuthorizeUrl());
            Process.Start(new ProcessStartInfo
            {
                FileName = authorizeUri.ToString(),
                UseShellExecute = true
            });

            return await taskCompletionSource.Task;
        }

        private async Task<string> ExchangeCodeForToken(string code)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken");
                request.Headers.Add("Accept", "application/json");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUrl),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });
                request.Content = content;

                var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(body);
                if (document.RootElement.TryGetProperty("access_token", out var tokenProp))
                {
                    return tokenProp.GetString() ?? throw new Exception("Token is null");
                }
                throw new Exception("Token not found in response");
            }
        }
    }
}
