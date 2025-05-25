using DataAccess.Model.Authentication;
using IRepository;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Data;

namespace Repository.Authentication
{
    public class SessionRepository : ISessionRepository
    {
        private DatabaseContext dataContext;

        public SessionRepository(DatabaseContext context)
        {
            this.dataContext = context;
        }
        public async Task<Session> CreateSession(Guid userId)
        {
            Session session = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = userId
            };

            await this.dataContext.Sessions.AddAsync(session);
            await this.dataContext.SaveChangesAsync();

            return session;
        }

        public async Task<bool> EndSession(Guid sessionId)
        {
            Session? session = await this.dataContext.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
            {
                // I consider this true if we didn't have the session itself
                return true;
            }

            this.dataContext.Sessions.Remove(session);
            return await this.dataContext.SaveChangesAsync() > 0;
        }

        public async Task<Session?> GetSession(Guid sessionId)
        {
            return await this.dataContext.Sessions.FirstOrDefaultAsync(item => item.SessionId == sessionId);
        }

        public async Task<Session?> GetSessionByUserId(Guid userId)
        {
            return await dataContext.Sessions.FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}