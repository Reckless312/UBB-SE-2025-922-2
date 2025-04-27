using System;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public interface IGitHubLocalOAuthServer
    {
        static abstract event Action<string>? OnCodeReceived;

        Task StartAsync();
        void Stop();
    }
}