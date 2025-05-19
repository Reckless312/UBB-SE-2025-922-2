using System;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.Github
{
    public interface IGitHubLocalOAuthServer
    {
        //static abstract event Action<string>? OnCodeReceived;

        Task StartAsync();
        void Stop();
    }
}