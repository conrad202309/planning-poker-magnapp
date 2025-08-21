using MagnaPP.Domain.Entities;

namespace MagnaPP.Infrastructure.Services;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(string name, User creator);
    Task<Session?> GetSessionAsync(Guid sessionId);
    Task<List<Session>> GetActiveSessionsAsync();
    Task<bool> UpdateSessionAsync(Session session);
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> AddUserToSessionAsync(Guid sessionId, User user);
    Task<bool> RemoveUserFromSessionAsync(Guid sessionId, Guid userId);
    Task<List<Session>> GetExpiredSessionsAsync();
    Task CleanupExpiredSessionsAsync();
    Task<int> GetActiveSessionCountAsync();
    Task<bool> CanCreateNewSessionAsync();
}