using System;
using DataAccess.Model.Authentication;

namespace IRepository
{
    public interface ISessionRepository
    {
        public Session CreateSession(Guid userId);
        public bool EndSession(Guid sessionId);
        public Session GetSession(Guid sessionId);
        public Session GetSessionByUserId(Guid userId);
    }
}