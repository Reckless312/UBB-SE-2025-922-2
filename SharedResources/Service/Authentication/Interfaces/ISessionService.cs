using DataAccess.Model.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Service.Authentication.Interfaces
{
    public interface ISessionService
    {
        Task<Session> CreateSessionAsync(Guid userId);
        Task<bool> EndSessionAsync(Guid sessionId);
        Task<Session> GetSessionAsync(Guid sessionId);
        Task<Session> GetSessionByUserIdAsync(Guid userId);
        Task<bool> AuthorizeActionAsync(Guid sessionId, string resource, string action);
        Task<bool> ValidateSessionAsync(Guid sessionId);
    }
}
