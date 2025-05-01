using System;
using Model.Authentication;

namespace Repository.Authentication.Interfaces
{
    public interface ISessionRepository
    {
        public Session CreateSession(Guid userId);
        public bool EndSession(Guid sessionId);
        public Session GetSession(Guid sessionId);
        public Session GetSessionByUserId(Guid userId);
    }
}