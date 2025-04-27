using System;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.LinkedIn
{
    public interface ILinkedInLocalOAuthServer
    {
        static abstract event Action<string>? OnCodeReceived;

        Task StartAsync();
        void Stop();
    }
}