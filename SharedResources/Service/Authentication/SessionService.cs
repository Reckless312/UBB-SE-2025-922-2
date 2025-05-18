using System;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;
using DataAccess.Service;
using DataAccess.Service.Authentication;
using DataAccess.Service.Authentication.Interfaces;
using IRepository;
using Repository.AdminDashboard;
using ServerAPI.Data;

namespace DrinkDb_Auth.Service.Authentication
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository sessionRepository;

        private DatabaseContext context;

        public SessionService(ISessionRepository sessionRepository, DatabaseContext context)
        {
            this.sessionRepository = sessionRepository;
            this.context = context;
        }

        public async Task<Session> CreateSessionAsync(Guid userId)
        {
            return await this.sessionRepository.CreateSession(userId);
        }

        public async Task<bool> EndSessionAsync(Guid sessionId)
        {
            return await this.sessionRepository.EndSession(sessionId);
        }

        public async Task<Session> GetSessionAsync(Guid sessionId)
        {
            return await this.sessionRepository.GetSession(sessionId);
        }

        public async Task<bool> ValidateSessionAsync(Guid sessionId)
        {
            var session = await this.GetSessionAsync(sessionId);
            return session != null && session.IsActive();
        }

        public async Task<bool> AuthorizeActionAsync(Guid sessionId, string resource, string action)
        {
            var session = await this.GetSessionAsync(sessionId);
            if (session == null || !session.IsActive())
            {
                return false;
            }

            var userService = new UserService(new UserRepository(context), new AuthenticationService(context));
            return await userService.ValidateAction(session.UserId, resource, action);
        }

        public async Task<Session> GetSessionByUserIdAsync(Guid userId)
        {
            return await this.sessionRepository.GetSessionByUserId(userId);
        }
    }
}