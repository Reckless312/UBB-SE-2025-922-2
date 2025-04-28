using System;
using CombinedProject.Model;

namespace CombinedProject.Service
{
    public interface IAuthenticationService
    {
        User GetUser(Guid sessionId);
        void Logout();
    }
}