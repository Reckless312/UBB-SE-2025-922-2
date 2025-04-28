using System;
using System.Threading.Tasks;

namespace CombinedProject.AuthProviders.Github
{
    public interface IGitHubLocalOAuthServer
    {
        static abstract event Action<string>? OnCodeReceived;

        Task StartAsync();
        void Stop();
    }
}