using System;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;

namespace DrinkDb_Auth.Service.Authentication.Interfaces
{
    public interface IAuthenticationService
    {
        User GetUser(Guid sessionId);

        void Logout();
    }
}