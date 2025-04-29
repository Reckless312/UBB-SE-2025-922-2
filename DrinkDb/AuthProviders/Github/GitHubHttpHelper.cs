using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.Github
{
    /// <summary>
    /// Everything HTTP class. You need some HTTP? GitHubHttpHelper has it.
    /// </summary>
    public class GitHubHttpHelper : IGitHubHttpHelper
    {
        private readonly HttpListener httpListener = new HttpListener();
        public bool IsListening
        {
            get { return httpListener.IsListening; }
        }
        public void Start()
        {
            httpListener.Start();
        }
        public void Stop()
        {
            httpListener.Stop();
        }
        public HttpListenerPrefixCollection Prefixes
        {
            get { return httpListener.Prefixes; }
        }
        public (string gitHubId, string gitHubLogin) FetchGitHubUserInfo(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"token {token}");
                client.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");

                var response = client.GetAsync("https://api.github.com/user").Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch user info from GitHub.");
                }

                string userJson = response.Content.ReadAsStringAsync().Result;
                using (JsonDocument userDocument = JsonDocument.Parse(userJson))
                {
                    var root = userDocument.RootElement;
                    string gitHubId = root.GetProperty("id").GetRawText();
                    string? gitHubLogin = root.GetProperty("login").GetString();
                    if (gitHubLogin == null)
                    {
                        throw new Exception("GitHub login is null.");
                    }
                    return (gitHubId, gitHubLogin);
                }
            }
        }

        public Task<HttpListenerContext> GetContextAsync()
        {
            return httpListener.GetContextAsync();
        }
    }
}
