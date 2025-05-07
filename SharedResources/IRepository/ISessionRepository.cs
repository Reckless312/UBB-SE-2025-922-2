using System;
using DataAccess.Model.Authentication;

namespace IRepository
{
    public interface ISessionRepository
    {
        public Task<Session> CreateSession(Guid userId);
        public Task<bool> EndSession(Guid sessionId);
        public Task<Session> GetSession(Guid sessionId);
        public Task<Session> GetSessionByUserId(Guid userId);
    }
}