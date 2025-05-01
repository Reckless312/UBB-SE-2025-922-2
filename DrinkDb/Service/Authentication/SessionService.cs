using System;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.Repository.Authentication;

namespace DrinkDb_Auth.Service.Authentication
{
    public class SessionService
    {
        private readonly SessionRepository sessionRepository;

        public SessionService()
        {
            sessionRepository = new SessionRepository();
        }

        public Session CreateSession(Guid userId)
        {
            return sessionRepository.CreateSession(userId);
        }

        public bool EndSession(Guid sessionId)
        {
            return sessionRepository.EndSession(sessionId);
        }

        public Session GetSession(Guid sessionId)
        {
            return sessionRepository.GetSession(sessionId);
        }

        public bool ValidateSession(Guid sessionId)
        {
            var session = GetSession(sessionId);
            return session != null && session.IsActive;
        }

        public bool AuthorizeAction(Guid sessionId, string resource, string action)
        {
            var session = GetSession(sessionId);
            if (session == null || !session.IsActive)
            {
                return false;
            }

            var userService = new UserService();
            return userService.ValidateAction(session.UserId, resource, action);
        }
    }
}