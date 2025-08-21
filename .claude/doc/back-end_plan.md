# MagnaPP Planning Poker Backend Implementation Plan

## Overview

This document provides a comprehensive implementation plan for the MagnaPP Planning Poker backend using C# .NET 9. The backend focuses on session identification and management with real-time communication capabilities.

## Project Structure

```
src/
├── MagnaPP.Api/                           # Main Web API project
│   ├── Controllers/                       # REST API controllers
│   │   ├── SessionController.cs           # Session CRUD operations
│   │   ├── UserController.cs              # User management within sessions
│   │   └── HealthController.cs            # Health check endpoint
│   ├── Hubs/                             # SignalR hubs
│   │   └── PlanningPokerHub.cs           # Real-time session communication
│   ├── Models/                           # Request/Response DTOs
│   │   ├── Requests/                     
│   │   │   ├── CreateSessionRequest.cs    
│   │   │   ├── JoinSessionRequest.cs      
│   │   │   ├── StartVotingRequest.cs      
│   │   │   └── SubmitVoteRequest.cs       
│   │   ├── Responses/                    
│   │   │   ├── SessionResponse.cs         
│   │   │   ├── SessionListResponse.cs     
│   │   │   ├── UserResponse.cs            
│   │   │   └── VotingResultResponse.cs    
│   │   └── Validation/                   
│   │       ├── CreateSessionValidator.cs  
│   │       └── JoinSessionValidator.cs    
│   ├── Services/                         # Business logic layer
│   │   ├── Interfaces/                   
│   │   │   ├── ISessionService.cs         
│   │   │   ├── IUserService.cs            
│   │   │   ├── IVotingService.cs          
│   │   │   └── ISessionCleanupService.cs  
│   │   ├── SessionService.cs             # Core session management
│   │   ├── UserService.cs                # User management within sessions
│   │   ├── VotingService.cs              # Voting round management
│   │   └── SessionCleanupService.cs      # Background cleanup service
│   ├── Domain/                           # Domain entities
│   │   ├── Session.cs                    # Core session entity
│   │   ├── User.cs                       # User entity
│   │   ├── Vote.cs                       # Vote entity
│   │   ├── VotingRound.cs                # Voting round entity
│   │   └── Enums/                        
│   │       ├── UserRole.cs               
│   │       ├── SessionStatus.cs          
│   │       └── VotingStatus.cs           
│   ├── Infrastructure/                   # Infrastructure concerns
│   │   ├── Cache/                        
│   │   │   ├── ISessionCache.cs          
│   │   │   └── MemorySessionCache.cs     
│   │   ├── Middleware/                   
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   └── SessionValidationMiddleware.cs
│   │   └── Extensions/                   
│   │       ├── ServiceCollectionExtensions.cs
│   │       └── ApplicationBuilderExtensions.cs
│   ├── Configuration/                    # Configuration classes
│   │   ├── SessionConfiguration.cs       
│   │   ├── SignalRConfiguration.cs       
│   │   └── CorsConfiguration.cs          
│   ├── Program.cs                        # Application entry point
│   ├── appsettings.json                  # Configuration file
│   ├── appsettings.Development.json      # Development configuration
│   └── MagnaPP.Api.csproj               # Project file
├── MagnaPP.Tests/                        # Test project
│   ├── Unit/                             # Unit tests
│   │   ├── Services/                     
│   │   │   ├── SessionServiceTests.cs    
│   │   │   ├── UserServiceTests.cs       
│   │   │   └── VotingServiceTests.cs     
│   │   └── Controllers/                  
│   │       ├── SessionControllerTests.cs 
│   │       └── UserControllerTests.cs    
│   ├── Integration/                      # Integration tests
│   │   ├── SessionIntegrationTests.cs    
│   │   └── SignalRIntegrationTests.cs    
│   └── MagnaPP.Tests.csproj             # Test project file
└── MagnaPP.sln                          # Solution file
```

## 1. Domain Models

### Session.cs
```csharp
public class Session
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid ScrumMasterId { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public List<User> Users { get; set; } = new();
    public List<VotingRound> VotingRounds { get; set; } = new();
    public VotingRound? CurrentVotingRound { get; set; }
    public int MaxUsers { get; set; } = 16;
    
    // Business logic methods
    public bool CanAcceptNewUser();
    public bool IsExpired(TimeSpan timeout);
    public void UpdateActivity();
    public User? GetScrumMaster();
}
```

### User.cs
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public Guid SessionId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public bool IsConnected { get; set; }
    
    // Business logic methods
    public bool IsScrumMaster();
    public void UpdateActivity();
}
```

### VotingRound.cs
```csharp
public class VotingRound
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public VotingStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? RevealedAt { get; set; }
    public List<Vote> Votes { get; set; } = new();
    
    // Business logic methods
    public bool AllUsersVoted(int totalUsers);
    public VotingStatistics CalculateStatistics();
    public bool CanReveal();
}
```

### Vote.cs
```csharp
public class Vote
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VotingRoundId { get; set; }
    public string Value { get; set; } // "1", "2", "3", "5", "8", "13", "21", "Coffee"
    public DateTime SubmittedAt { get; set; }
}
```

## 2. Enumerations

### UserRole.cs
```csharp
public enum UserRole
{
    ScrumMaster = 1,
    Participant = 2
}
```

### SessionStatus.cs
```csharp
public enum SessionStatus
{
    Active = 1,
    Voting = 2,
    Completed = 3,
    Expired = 4
}
```

### VotingStatus.cs
```csharp
public enum VotingStatus
{
    NotStarted = 1,
    InProgress = 2,
    Revealed = 3,
    Completed = 4
}
```

## 3. Request/Response DTOs

### CreateSessionRequest.cs
```csharp
public class CreateSessionRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ScrumMasterName { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string ScrumMasterEmail { get; set; }
}
```

### SessionResponse.cs
```csharp
public class SessionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public UserResponse ScrumMaster { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UserResponse> Users { get; set; } = new();
    public VotingRoundResponse? CurrentVotingRound { get; set; }
    public int UserCount { get; set; }
    public int MaxUsers { get; set; }
    public bool CanJoin { get; set; }
}
```

## 4. Service Layer Interfaces

### ISessionService.cs
```csharp
public interface ISessionService
{
    Task<SessionResponse> CreateSessionAsync(CreateSessionRequest request);
    Task<SessionResponse?> GetSessionAsync(Guid sessionId);
    Task<List<SessionResponse>> GetActiveSessionsAsync();
    Task<bool> DeleteSessionAsync(Guid sessionId, Guid userId);
    Task<SessionResponse?> JoinSessionAsync(Guid sessionId, JoinSessionRequest request);
    Task<bool> LeaveSessionAsync(Guid sessionId, Guid userId);
    Task<bool> TransferScrumMasterAsync(Guid sessionId, Guid currentScrumMasterId, Guid newScrumMasterId);
    Task<bool> KickUserAsync(Guid sessionId, Guid scrumMasterId, Guid userToKick);
    Task UpdateSessionActivityAsync(Guid sessionId);
    Task<int> GetActiveSessionCountAsync();
    Task<bool> CanCreateNewSessionAsync();
}
```

### IVotingService.cs
```csharp
public interface IVotingService
{
    Task<VotingRoundResponse> StartVotingRoundAsync(Guid sessionId, Guid scrumMasterId, StartVotingRequest request);
    Task<bool> SubmitVoteAsync(Guid sessionId, Guid userId, SubmitVoteRequest request);
    Task<VotingResultResponse> RevealVotesAsync(Guid sessionId, Guid scrumMasterId);
    Task<VotingRoundResponse?> GetCurrentVotingRoundAsync(Guid sessionId);
    Task<bool> EndVotingRoundAsync(Guid sessionId, Guid scrumMasterId);
}
```

## 5. Session Cache Implementation

### ISessionCache.cs
```csharp
public interface ISessionCache
{
    Task<Session?> GetSessionAsync(Guid sessionId);
    Task<List<Session>> GetAllSessionsAsync();
    Task SetSessionAsync(Session session);
    Task RemoveSessionAsync(Guid sessionId);
    Task<bool> ExistsAsync(Guid sessionId);
    Task<int> GetSessionCountAsync();
    Task<List<Session>> GetExpiredSessionsAsync(TimeSpan timeout);
}
```

### MemorySessionCache.cs
```csharp
public class MemorySessionCache : ISessionCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemorySessionCache> _logger;
    private readonly ConcurrentDictionary<Guid, DateTime> _sessionIndex;
    private readonly object _lockObject = new();

    public MemorySessionCache(IMemoryCache cache, ILogger<MemorySessionCache> logger)
    {
        _cache = cache;
        _logger = logger;
        _sessionIndex = new ConcurrentDictionary<Guid, DateTime>();
    }

    // Implementation focuses on:
    // - Thread-safe operations using ConcurrentDictionary
    // - Proper cache key management
    // - Efficient session indexing for cleanup operations
    // - Memory pressure handling
    // - Cache entry eviction callbacks
}
```

## 6. Controllers

### SessionController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    // Endpoints:
    // POST /api/session - Create new session
    // GET /api/session/{id} - Get session details
    // GET /api/session - Get all active sessions (for discovery)
    // DELETE /api/session/{id} - Delete session (Scrum Master only)
    // POST /api/session/{id}/join - Join session
    // POST /api/session/{id}/leave - Leave session
    // POST /api/session/{id}/transfer-master - Transfer Scrum Master role
    // POST /api/session/{id}/kick-user - Kick user from session
}
```

## 7. SignalR Hub

### PlanningPokerHub.cs
```csharp
public class PlanningPokerHub : Hub
{
    private readonly ISessionService _sessionService;
    private readonly IVotingService _votingService;
    private readonly IUserService _userService;

    // Hub methods:
    // - JoinSession(sessionId, userId)
    // - LeaveSession(sessionId, userId)
    // - StartVoting(sessionId, title, description)
    // - SubmitVote(sessionId, voteValue)
    // - RevealVotes(sessionId)
    // - EndVotingRound(sessionId)

    // Client notifications:
    // - UserJoined(user)
    // - UserLeft(user)
    // - VotingStarted(votingRound)
    // - VoteSubmitted(userId, hasVoted)
    // - VotesRevealed(results)
    // - SessionEnded()
    // - UserKicked(userId)
    // - ScrumMasterChanged(newScrumMaster)

    // Connection management:
    // - OnConnectedAsync() - Associate connection with user/session
    // - OnDisconnectedAsync() - Handle graceful disconnection
    // - Group management for session-based broadcasting
}
```

## 8. Background Services

### SessionCleanupService.cs
```csharp
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(10);

    // Responsibilities:
    // - Run every minute to check for expired sessions
    // - Remove sessions with no activity for 10+ minutes
    // - Handle Scrum Master disconnection grace period (5 minutes)
    // - Notify remaining users when session expires
    // - Clean up associated voting rounds and user data
    // - Log cleanup activities for monitoring
}
```

## 9. Configuration Classes

### SessionConfiguration.cs
```csharp
public class SessionConfiguration
{
    public int MaxConcurrentSessions { get; set; } = 3;
    public int MaxUsersPerSession { get; set; } = 16;
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan ScrumMasterGracePeriod { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
}
```

## 10. Middleware

### ErrorHandlingMiddleware.cs
```csharp
public class ErrorHandlingMiddleware
{
    // Global exception handling
    // Standardized error responses
    // Logging of unhandled exceptions
    // Security-focused error messages
}
```

### SessionValidationMiddleware.cs
```csharp
public class SessionValidationMiddleware
{
    // Validate session existence for session-specific endpoints
    // Check session capacity limits
    // Verify user permissions for protected actions
    // Session activity tracking
}
```

## 11. Program.cs Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVotingService, VotingService>();
builder.Services.AddSingleton<ISessionCache, MemorySessionCache>();
builder.Services.AddHostedService<SessionCleanupService>();

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

// Configure validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSessionValidator>();

// Configure options
builder.Services.Configure<SessionConfiguration>(
    builder.Configuration.GetSection("SessionConfiguration"));

var app = builder.Build();

// Configure pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowAngularApp");
app.UseRouting();
app.UseMiddleware<SessionValidationMiddleware>();

app.MapControllers();
app.MapHub<PlanningPokerHub>("/hubs/planning-poker");

app.Run();
```

## 12. Memory Management Strategy

### Cache Configuration
- Use IMemoryCache with size limits
- Implement sliding expiration for session activity
- Configure memory pressure eviction policies
- Monitor cache hit ratios and memory usage

### Cleanup Strategy
- Background service runs every minute
- Identify sessions with LastActivity > 10 minutes
- Graceful session termination with user notifications
- Immediate cleanup of associated data (votes, rounds)

## 13. API Endpoint Specifications

### Session Endpoints
```
POST /api/session
- Create new session (if under limit)
- Return 409 if session limit reached
- Return 201 with session details

GET /api/session/{id}
- Return session details with user list
- Return 404 if session not found
- Include current voting round if active

GET /api/session
- Return list of active sessions for discovery
- Include user count and availability status

POST /api/session/{id}/join
- Add user to session (if under capacity)
- Return 409 if session full
- Return 200 with updated session state

DELETE /api/session/{id}
- End session (Scrum Master only)
- Notify all users via SignalR
- Return 403 if not Scrum Master
```

### Voting Endpoints
```
POST /api/session/{id}/voting/start
- Start new voting round
- Return 403 if not Scrum Master
- Return 409 if voting already in progress

POST /api/session/{id}/voting/vote
- Submit vote for current round
- Return 404 if no active voting
- Return 409 if already voted

POST /api/session/{id}/voting/reveal
- Reveal all votes (Scrum Master only)
- Calculate and return statistics
- Broadcast results via SignalR

POST /api/session/{id}/voting/end
- End current voting round
- Prepare for next round
```

## 14. Real-time Communication Events

### Server to Client Events
```typescript
// User management
UserJoined(user: UserResponse)
UserLeft(userId: string)
UserKicked(userId: string)
ScrumMasterChanged(newScrumMaster: UserResponse)

// Voting events
VotingStarted(votingRound: VotingRoundResponse)
VoteSubmitted(userId: string, hasVoted: boolean)
AllVotesSubmitted()
VotesRevealed(results: VotingResultResponse)
VotingEnded()

// Session events
SessionEnded()
SessionExpiring(minutesRemaining: number)
```

### Client to Server Events
```typescript
// Connection management
JoinSession(sessionId: string, userId: string)
LeaveSession(sessionId: string, userId: string)

// Voting actions
SubmitVote(sessionId: string, voteValue: string)
RequestReveal(sessionId: string)

// Session management (Scrum Master only)
StartVoting(sessionId: string, title: string, description: string)
EndVoting(sessionId: string)
KickUser(sessionId: string, userIdToKick: string)
TransferScrumMaster(sessionId: string, newScrumMasterId: string)
```

## 15. Error Handling Strategy

### Validation Errors
- Use FluentValidation for request validation
- Return standardized 400 responses with detailed field errors
- Client-friendly error messages

### Business Logic Errors
- Custom exceptions for domain violations
- Appropriate HTTP status codes (409 for conflicts, 403 for authorization)
- Consistent error response format

### System Errors
- Global exception middleware
- Structured logging with correlation IDs
- Sanitized error messages for security

## 16. Implementation Priority Order

### Phase 1: Core Infrastructure
1. Create project structure and solution file
2. Implement domain models and enums
3. Set up dependency injection container
4. Configure basic middleware pipeline
5. Implement memory cache infrastructure

### Phase 2: Session Management
1. Implement ISessionCache with MemorySessionCache
2. Create SessionService with CRUD operations
3. Build SessionController with REST endpoints
4. Add session validation middleware
5. Implement session cleanup background service

### Phase 3: User Management
1. Implement UserService for session membership
2. Add user management endpoints to SessionController
3. Create user validation logic
4. Implement role-based authorization

### Phase 4: Real-time Communication
1. Set up SignalR hub infrastructure
2. Implement connection/disconnection handling
3. Add session group management
4. Implement basic real-time notifications

### Phase 5: Voting System
1. Implement VotingService with round management
2. Create voting-specific endpoints
3. Add vote submission and revelation logic
4. Implement voting statistics calculation

### Phase 6: Advanced Features
1. Add comprehensive error handling
2. Implement detailed logging and monitoring
3. Add health check endpoints
4. Performance optimization and load testing

### Phase 7: Testing and Documentation
1. Unit tests for all services
2. Integration tests for API endpoints
3. SignalR hub testing
4. API documentation generation

## 17. Security Considerations

### Session Security
- GUID-based session IDs prevent enumeration attacks
- Validate user permissions for all operations
- Implement rate limiting for session creation
- Session timeout enforcement

### Real-time Security
- Validate SignalR connections against active sessions
- Prevent cross-session message broadcasting
- Connection ID validation and tracking

### Data Protection
- No persistent storage reduces attack surface
- Memory-only data automatically cleaned up
- No sensitive data logging
- CORS properly configured for frontend

This implementation plan provides a comprehensive foundation for the MagnaPP Planning Poker backend, focusing on robust session management, real-time communication, and scalable architecture using modern .NET 9 patterns and best practices.