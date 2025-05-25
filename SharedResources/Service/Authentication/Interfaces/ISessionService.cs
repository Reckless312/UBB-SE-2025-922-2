using DataAccess.Model.Authentication;

namespace DataAccess.Service.Authentication.Interfaces
{
    public interface ISessionService
    {
        Task<Session?> CreateSessionAsync(Guid userId);
        Task<bool> EndSessionAsync(Guid sessionId);
        Task<Session?> GetSessionAsync(Guid sessionId);
        Task<Session?> GetSessionByUserIdAsync(Guid userId);
    }
}
