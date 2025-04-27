using System;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.Facebook
{
    public interface IFacebookLocalOAuthServer
    {
        static abstract event Action<string>? OnTokenReceived;

        Task StartAsync();
        void Stop();
    }
}