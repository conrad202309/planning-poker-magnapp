using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MagnaPP.Domain.Entities;
using MagnaPP.Domain.Enums;
using System.Collections.Concurrent;

namespace MagnaPP.Infrastructure.Services;

public class MemorySessionService : ISessionService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemorySessionService> _logger;
    private readonly ConcurrentDictionary<Guid, DateTime> _sessionIndex;
    private readonly SemaphoreSlim _semaphore;

    private const string SessionKeyPrefix = "session_";
    private const string SessionIndexKey = "session_index";
    private const int MaxConcurrentSessions = 3;

    public MemorySessionService(IMemoryCache cache, ILogger<MemorySessionService> logger)
    {
        _cache = cache;
        _logger = logger;
        _sessionIndex = new ConcurrentDictionary<Guid, DateTime>();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<Session> CreateSessionAsync(string name, User creator)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await CanCreateNewSessionAsync())
            {
                throw new InvalidOperationException("Maximum number of concurrent sessions reached");
            }

            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                Name = name,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                Status = SessionStatus.Active
            };

            session.AddUser(creator);

            var cacheKey = GetSessionCacheKey(session.SessionId);
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = Session.InactivityTimeout,
                Priority = CacheItemPriority.High
            };
            cacheOptions.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = OnSessionEvicted,
                State = session.SessionId
            });

            _cache.Set(cacheKey, session, cacheOptions);
            _sessionIndex[session.SessionId] = session.CreatedAt;

            _logger.LogInformation("Session created: {SessionId} - {SessionName}", session.SessionId, session.Name);
            return session;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Session?> GetSessionAsync(Guid sessionId)
    {
        await Task.CompletedTask;
        var cacheKey = GetSessionCacheKey(sessionId);
        
        if (_cache.TryGetValue(cacheKey, out Session? session))
        {
            if (session != null && session.IsExpired)
            {
                await DeleteSessionAsync(sessionId);
                return null;
            }
            return session;
        }

        return null;
    }

    public async Task<List<Session>> GetActiveSessionsAsync()
    {
        await Task.CompletedTask;
        var activeSessions = new List<Session>();

        foreach (var sessionId in _sessionIndex.Keys.ToList())
        {
            var session = await GetSessionAsync(sessionId);
            if (session != null && session.Status == SessionStatus.Active)
            {
                activeSessions.Add(session);
            }
        }

        return activeSessions.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public async Task<bool> UpdateSessionAsync(Session session)
    {
        if (session == null) return false;

        await _semaphore.WaitAsync();
        try
        {
            var cacheKey = GetSessionCacheKey(session.SessionId);
            
            if (!_cache.TryGetValue(cacheKey, out _))
            {
                return false;
            }

            session.UpdateActivity();
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = Session.InactivityTimeout,
                Priority = CacheItemPriority.High
            };
            cacheOptions.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = OnSessionEvicted,
                State = session.SessionId
            });

            _cache.Set(cacheKey, session, cacheOptions);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        await _semaphore.WaitAsync();
        try
        {
            var cacheKey = GetSessionCacheKey(sessionId);
            _cache.Remove(cacheKey);
            _sessionIndex.TryRemove(sessionId, out _);
            
            _logger.LogInformation("Session deleted: {SessionId}", sessionId);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> AddUserToSessionAsync(Guid sessionId, User user)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null) return false;

        if (session.AddUser(user))
        {
            await UpdateSessionAsync(session);
            _logger.LogInformation("User {UserName} joined session {SessionId}", user.Name, sessionId);
            return true;
        }

        return false;
    }

    public async Task<bool> RemoveUserFromSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null) return false;

        var user = session.Users.FirstOrDefault(u => u.UserId == userId);
        if (user != null && session.RemoveUser(userId))
        {
            await UpdateSessionAsync(session);
            _logger.LogInformation("User {UserName} left session {SessionId}", user.Name, sessionId);
            
            // Delete session if no users remain
            if (!session.Users.Any())
            {
                await DeleteSessionAsync(sessionId);
            }
            
            return true;
        }

        return false;
    }

    public async Task<List<Session>> GetExpiredSessionsAsync()
    {
        await Task.CompletedTask;
        var expiredSessions = new List<Session>();

        foreach (var sessionId in _sessionIndex.Keys.ToList())
        {
            var session = await GetSessionAsync(sessionId);
            if (session != null && session.IsExpired)
            {
                expiredSessions.Add(session);
            }
        }

        return expiredSessions;
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        var expiredSessions = await GetExpiredSessionsAsync();
        
        foreach (var session in expiredSessions)
        {
            await DeleteSessionAsync(session.SessionId);
            _logger.LogInformation("Cleaned up expired session: {SessionId}", session.SessionId);
        }
    }

    public async Task<int> GetActiveSessionCountAsync()
    {
        var activeSessions = await GetActiveSessionsAsync();
        return activeSessions.Count;
    }

    public async Task<bool> CanCreateNewSessionAsync()
    {
        var activeCount = await GetActiveSessionCountAsync();
        return activeCount < MaxConcurrentSessions;
    }

    private static string GetSessionCacheKey(Guid sessionId) => $"{SessionKeyPrefix}{sessionId}";

    private void OnSessionEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        if (state is Guid sessionId)
        {
            _sessionIndex.TryRemove(sessionId, out _);
            _logger.LogInformation("Session evicted from cache: {SessionId}, Reason: {Reason}", sessionId, reason);
        }
    }
}