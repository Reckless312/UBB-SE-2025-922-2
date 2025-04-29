using DrinkDb_Auth.OAuthProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public class GitHubOAuthHelper : IGitHubOAuthHelper
    {
        private const string ClientId = "Ov23ligheYgI7JILPWGY";
        private const string ClientSecret = "791dfaf36750b2a34a752c4fe3fb3703cef18836";
        private const string RedirectUri = "http://localhost:8890/auth";
        private const string Scope = "read:user user:email";
        private GenericOAuth2Provider gitHubOAuth2Provider;
        private TaskCompletionSource<AuthenticationResponse> taskCompletionSource;

        public GitHubOAuthHelper()
        {
            gitHubOAuth2Provider = new GitHubOAuth2Provider();
            taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();
            GitHubLocalOAuthServer.OnCodeReceived += OnCodeReceived;
        }

        public GitHubOAuthHelper(GenericOAuth2Provider gitHubOAuth2Provider)
        {
            this.gitHubOAuth2Provider = gitHubOAuth2Provider;
            taskCompletionSource = new TaskCompletionSource<AuthenticationResponse>();
        }

        /// <summary>
        /// Build the GitHub authorization URL using standard OAuth2 code flow.
        /// </summary>
        private string BuildAuthorizeUrl()
        {
            return $"https://github.com/login/oauth/authorize" +
                   $"?client_id={ClientId}" +
                   $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                   $"&scope={Uri.EscapeDataString(Scope)}" +
                   $"&response_type=code";
        }

        /// <summary>
        /// Called when the local server has received a GitHub code.
        /// We then exchange that code for an access token and set the result in _tcs.
        /// </summary>
        private async void OnCodeReceived(string code)
        {
            if (taskCompletionSource == null || taskCompletionSource.Task.IsCompleted)
            {
                return;
            }

            try
            {
                // Exchange code for an access token
                var token = await ExchangeCodeForToken(code);
                var result = gitHubOAuth2Provider.Authenticate(string.Empty, token);
                taskCompletionSource.SetResult(result);
            }
            catch (Exception exception)
            {
                taskCompletionSource.SetException(exception);
            }
        }

        /// <summary>
        /// Actually open the user's default browser to the GitHub authorize page,
        /// then wait for the local server to get the code and do the exchange.
        /// </summary>
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

        /// <summary>
        /// POST to GitHub's /login/oauth/access_token to get an access token from the code.
        /// </summary>
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
