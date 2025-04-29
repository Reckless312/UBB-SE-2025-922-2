using System.Net;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public interface IGitHubHttpHelper
    {
        (string gitHubId, string gitHubLogin) FetchGitHubUserInfo(string token);
        void Start();
        bool IsListening { get; }

        Task<HttpListenerContext> GetContextAsync();
        HttpListenerPrefixCollection Prefixes { get; }
        void Stop();
    }
}