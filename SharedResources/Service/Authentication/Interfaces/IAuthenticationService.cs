using System;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;

namespace DataAccess.Service.Authentication.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User> GetUser(Guid sessionId);

        void Logout();
    }
}