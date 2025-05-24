using System;
using System.Threading.Tasks;

namespace DataAccess.AuthProviders.Github
{
    public interface IGitHubLocalOAuthServer
    {
        Task StartAsync();
    }
}