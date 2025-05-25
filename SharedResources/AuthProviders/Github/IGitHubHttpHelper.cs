using System.Net;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.Github
{
    public interface IGitHubHttpHelper
    {
        void Start();
        bool IsListening { get; }

        Task<HttpListenerContext> GetContextAsync();
        HttpListenerPrefixCollection Prefixes { get; }
    }
}