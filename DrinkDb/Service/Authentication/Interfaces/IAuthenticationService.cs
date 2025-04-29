using System;
using DrinkDb_Auth.Model.Authentication;

namespace DrinkDb_Auth.Service.Authentication.Interfaces
{
    public interface IAuthenticationService
    {
        User GetUser(Guid sessionId);
        void Logout();
    }
}