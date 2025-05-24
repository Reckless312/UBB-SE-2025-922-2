using System.Net;
using System.Text.Json;

namespace DataAccess.AuthProviders.Github
{
    public class GitHubHttpHelper : IGitHubHttpHelper
    {
        private HttpListener httpListener = new HttpListener();
        public bool IsListening
        {
            get { return this.httpListener.IsListening; }
        }

        public void Start()
        {
            this.httpListener.Start();
        }

        public void Stop()
        {
            this.httpListener.Stop();
        }

        public HttpListenerPrefixCollection Prefixes
        {
            get { return this.httpListener.Prefixes; }
        }

        public Task<HttpListenerContext> GetContextAsync()
        {
            return this.httpListener.GetContextAsync();
        }
    }
}
