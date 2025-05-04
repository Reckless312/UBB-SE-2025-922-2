using System;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.ProxyRepository.AdminDashboard;
using IRepository;

namespace DrinkDb_Auth.Service.Authentication
{
    public class SessionService
    {
        private readonly ISessionRepository sessionRepository;

        public SessionService(ISessionRepository sessionRepository)
        {
            this.sessionRepository = sessionRepository;
        }

        public Session CreateSession(Guid userId)
        {
            return sessionRepository.CreateSession(userId).Result;
        }

        public bool EndSession(Guid sessionId)
        {
            return sessionRepository.EndSession(sessionId).Result;
        }

        public Session GetSession(Guid sessionId)
        {
            return sessionRepository.GetSession(sessionId).Result;
        }

        public bool ValidateSession(Guid sessionId)
        {
            var session = GetSession(sessionId);
            return session != null && session.IsActive();
        }

        public async Task<bool> AuthorizeAction(Guid sessionId, string resource, string action)
        {
            var session = GetSession(sessionId);
            if (session == null || !session.IsActive())
            {
                return false;
            }

            var userService = new UserService();
            return await userService.ValidateAction(session.UserId, resource, action);
        }
    }
}