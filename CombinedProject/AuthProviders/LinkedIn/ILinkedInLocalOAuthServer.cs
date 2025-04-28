using System;
using System.Threading.Tasks;

namespace CombinedProject.AuthProviders.LinkedIn
{
    public interface ILinkedInLocalOAuthServer
    {
        static abstract event Action<string>? OnCodeReceived;

        Task StartAsync();
        void Stop();
    }
}