using System;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.LinkedIn
{
    public interface ILinkedInLocalOAuthServer
    {
        static abstract event Action<string>? OnCodeReceived;

        Task StartAsync();
        void Stop();
    }
}