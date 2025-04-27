using System;
using DrinkDb_Auth.Model;

namespace DrinkDb_Auth.Service
{
    public interface IAuthenticationService
    {
        User GetUser(Guid sessionId);
        void Logout();
    }
}