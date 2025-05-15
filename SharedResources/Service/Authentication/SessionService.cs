using System;
using System.Threading.Tasks;
using Data;
using DataAccess.Model.Authentication;
using IRepository;
using Repository.AdminDashboard;

namespace DataAccess.Service.Authentication
{
    public class SessionService
    {
        private readonly ISessionRepository sessionRepository;

        private DatabaseContext context;

        public SessionService(ISessionRepository sessionRepository, DatabaseContext context)
        {
            this.sessionRepository = sessionRepository;
            this.context = context;
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

            var userService = new UserService(new UserRepository(context), new AuthenticationService(context));
            return await userService.ValidateAction(session.UserId, resource, action);
        }
    }
}