using DataAccess.OAuthProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.Github
{
    public class GitHubOAuthHelper : IGitHubOAuthHelper
    {
        private const string ClientId = "Ov23ligheYgI7JILPWGY";
        private const string ClientSecret = "791dfaf36750b2a34a752c4fe3fb3703cef18836";
        private const string RedirectUri = "http://localhost:8890/auth";
        private const string Scope = "read:user user:email";
        private GenericOAuth2Provider gitHubOAuth2Provider;
        private TaskCompletionSource<AuthenticationResponse> taskCompletionSource;
        private GitHubLocalOAuthServer localServer;

        public GitHubOAuthHelper(GenericOAuth2Provider gitHubOAuth2Provider, GitHubLocalOAuthServer localServer)
        {
            this.gitHubOAuth2Provider = gitHubOAuth2Provider;
            this.localServer = localServer;
            taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();
            GitHubLocalOAuthServer.OnCodeReceived += OnCodeReceived;
        }

        private string BuildAuthorizeUrl()
        {
            return $"https://github.com/login/oauth/authorize" +
                   $"?client_id={ClientId}" +
                   $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                   $"&scope={Uri.EscapeDataString(Scope)}" +
                   $"&response_type=code";
        }

        private async void OnCodeReceived(string code)
        {
            Debug.WriteLine($"OnCodeReceived called with code: {code}");
            if (taskCompletionSource == null || taskCompletionSource.Task.IsCompleted)
            {
                Debug.WriteLine("OnCodeReceived: taskCompletionSource is null or already completed.");
                return;
            }

            try
            {
                Debug.WriteLine("Exchanging code for token...");
                var token = await ExchangeCodeForToken(code);
                Debug.WriteLine($"Token received: {token}");
                var result = gitHubOAuth2Provider.Authenticate(string.Empty, token);
                taskCompletionSource.SetResult(result);
                Debug.WriteLine("Authentication result set.");
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Exception in OnCodeReceived: {exception}");
                taskCompletionSource.SetException(exception);
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
                var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
                request.Headers.Add("Accept", "application/json"); // we want JSON response
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", RedirectUri)
                });
                request.Content = content;

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                using var responseDocument = JsonDocument.Parse(responseBody);
                if (responseDocument.RootElement.TryGetProperty("access_token", out var tokenProperty))
                {
                    return tokenProperty.GetString() ?? throw new Exception("Access token is null.");
                }
                throw new Exception("Failed to get access token from GitHub.");
            }
        }
    }
}
