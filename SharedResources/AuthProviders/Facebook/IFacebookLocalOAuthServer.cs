using System;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.Facebook
{
    public interface IFacebookLocalOAuthServer
    {
        static abstract event Action<string>? OnTokenReceived;

        Task StartAsync();
        void Stop();
    }
}