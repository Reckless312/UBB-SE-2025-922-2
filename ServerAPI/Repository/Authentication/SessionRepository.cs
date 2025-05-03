using System;
using System.Data;
using DataAccess.Model.Authentication;
using IRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Data;
using static Repository.AdminDashboard.UserRepository;

namespace Repository.Authentication
{
    public class SessionRepository : ISessionRepository
    {
        private readonly DatabaseContext _context;

        public SessionRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<Session> CreateSession(Guid userId)
        {
            try
            {
                var session = new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = userId
                };

                await _context.Sessions.AddAsync(session);
                await _context.SaveChangesAsync();

                return session;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to create a new session.", ex);
            }
        }

        public async Task<bool> EndSession(Guid sessionId)
        {
            try
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (session == null)
                {
                    throw new ArgumentException($"No session found with ID {sessionId}");
                }

                _context.Sessions.Remove(session);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to end session with ID {sessionId}.", ex);
            }
        }

        public async Task<Session> GetSession(Guid sessionId)
        {
            try
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (session == null)
                {
                    throw new ArgumentException($"No session found with ID {sessionId}");
                }

                return session;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve session with ID {sessionId}.", ex);
            }
        }

        public async Task<Session> GetSessionByUserId(Guid userId)
        {
            try
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.UserId == userId);

                if (session == null)
                {
                    throw new ArgumentException($"No session found for user with ID {userId}");
                }

                return session;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve session for user with ID {userId}.", ex);
            }
        }
    }
}