# MagnaPP Planning Poker - Big Picture Implementation Plan

## Project Overview

MagnaPP is a web-based Planning Poker application for distributed agile teams, supporting real-time collaborative estimation sessions through a virtual boardroom interface.

## System Architecture

### Technology Stack
- **Frontend**: Angular 18+ SPA with responsive design
- **Backend**: .NET 9 Web API with SignalR for real-time communication
- **Data Storage**: In-memory only (no persistent database)
- **Communication**: RESTful APIs + WebSocket (SignalR)
- **Deployment**: Corporate environment with IIS/Kestrel

### Key Constraints
- Maximum 3 concurrent sessions
- Maximum 16 users per session
- In-memory session storage only
- 10-minute inactivity timeout
- 5-minute Scrum Master disconnection grace period

## Backend Integration Architecture

### Core Components

#### 1. Session Management System
**Primary Responsibility**: GUID-based session identification and lifecycle management
- **Location**: `src/MagnaPP.Api/Services/SessionService.cs`
- **Cache**: `src/MagnaPP.Api/Infrastructure/Cache/MemorySessionCache.cs`
- **Endpoints**: `src/MagnaPP.Api/Controllers/SessionController.cs`
- **Models**: `src/MagnaPP.Api/Domain/Session.cs`

**Integration Points**:
- Frontend calls `/api/session` endpoints for CRUD operations
- Real-time updates via `/hubs/planning-poker` SignalR hub
- Background cleanup service maintains session limits and timeouts

#### 2. Real-time Communication Hub
**Primary Responsibility**: WebSocket-based real-time session synchronization
- **Location**: `src/MagnaPP.Api/Hubs/PlanningPokerHub.cs`
- **URL**: `/hubs/planning-poker`
- **Connection**: Session-based groups for targeted messaging

**Integration Points**:
- Frontend establishes SignalR connection on session join
- Broadcasts user actions (join/leave, voting, role changes)
- Maintains connection state with automatic reconnection logic

#### 3. Voting System
**Primary Responsibility**: Fibonacci-based estimation rounds with statistics
- **Location**: `src/MagnaPP.Api/Services/VotingService.cs`
- **Models**: `src/MagnaPP.Api/Domain/VotingRound.cs`, `Vote.cs`
- **Endpoints**: `/api/session/{id}/voting/*` endpoints

**Integration Points**:
- Frontend voting UI calls REST endpoints for vote submission
- Real-time vote status updates via SignalR
- Statistics calculation and revelation logic

#### 4. User Role Management
**Primary Responsibility**: Scrum Master vs Participant role enforcement
- **Location**: `src/MagnaPP.Api/Services/UserService.cs`
- **Models**: `src/MagnaPP.Api/Domain/User.cs`
- **Authorization**: Role-based endpoint protection

**Integration Points**:
- Frontend UI adapts based on user role permissions
- SignalR hub enforces role-based message broadcasting
- Session transfer capabilities for Scrum Master role

### Data Flow Architecture

#### Session Creation Flow
1. **Frontend**: User fills session creation form
2. **API**: `POST /api/session` validates and creates session
3. **Cache**: Session stored in memory with GUID identifier
4. **Response**: Session details returned with Scrum Master role assigned
5. **SignalR**: Connection established for real-time updates

#### User Join Flow
1. **Frontend**: User selects session from discovery list
2. **API**: `POST /api/session/{id}/join` validates capacity and user data
3. **Cache**: User added to session's user list
4. **SignalR**: User joins session group, broadcasts `UserJoined` event
5. **Response**: Updated session state returned to all clients

#### Voting Flow
1. **Frontend**: Scrum Master starts voting round
2. **API**: `POST /api/session/{id}/voting/start` creates voting round
3. **SignalR**: Broadcasts `VotingStarted` to all session participants
4. **Frontend**: Users submit votes via `POST /api/session/{id}/voting/vote`
5. **SignalR**: Vote status updates broadcast as `VoteSubmitted` events
6. **API**: `POST /api/session/{id}/voting/reveal` calculates statistics
7. **SignalR**: Broadcasts `VotesRevealed` with full results

### Memory Management Integration

#### Session Caching Strategy
- **Primary Cache**: `IMemoryCache` with sliding expiration
- **Session Index**: `ConcurrentDictionary<Guid, DateTime>` for efficient lookup
- **Cleanup Service**: Background service runs every minute
- **Capacity Management**: Hard limits enforced before session creation

#### Cleanup Integration Points
1. **Background Service**: Identifies expired sessions
2. **SignalR Notifications**: Warns users of impending expiration
3. **Cache Removal**: Removes session and associated data
4. **Client Handling**: Frontend redirects users to session discovery

### API Contract Integration

#### REST API Endpoints
```
Session Management:
- POST /api/session                    # Create session
- GET /api/session/{id}               # Get session details
- GET /api/session                    # List active sessions
- POST /api/session/{id}/join         # Join session
- DELETE /api/session/{id}            # End session

Voting Operations:
- POST /api/session/{id}/voting/start # Start voting round
- POST /api/session/{id}/voting/vote  # Submit vote
- POST /api/session/{id}/voting/reveal # Reveal votes
- POST /api/session/{id}/voting/end   # End voting round

User Management:
- POST /api/session/{id}/leave        # Leave session
- POST /api/session/{id}/kick         # Kick user (Scrum Master)
- POST /api/session/{id}/transfer     # Transfer Scrum Master role
```

#### SignalR Events
```
Client to Server:
- JoinSession(sessionId, userId)
- LeaveSession(sessionId, userId)
- SubmitVote(sessionId, voteValue)

Server to Client:
- UserJoined(user)
- UserLeft(userId)
- VotingStarted(votingRound)
- VoteSubmitted(userId, hasVoted)
- VotesRevealed(results)
- SessionEnded()
```

### Security Integration

#### Session Security
- GUID-based session IDs prevent enumeration
- Role-based authorization for protected operations
- Session capacity limits enforced at API level
- Automatic timeout and cleanup prevents resource exhaustion

#### Real-time Security
- SignalR connections validated against active sessions
- Group-based message broadcasting prevents cross-session communication
- Connection tracking prevents unauthorized access

### Error Handling Integration

#### API Error Responses
- Standardized error format across all endpoints
- Appropriate HTTP status codes (400, 403, 404, 409)
- Client-friendly error messages with validation details

#### Real-time Error Handling
- SignalR connection failures handled with exponential backoff
- Session expiration gracefully communicated to clients
- Network interruption recovery with state synchronization

### Development Workflow Integration

#### Frontend-Backend Development
1. **API First**: Implement REST endpoints with OpenAPI documentation
2. **SignalR Integration**: Add real-time events after basic functionality
3. **Testing**: API integration tests before frontend implementation
4. **Documentation**: Maintain API contract documentation

#### Deployment Integration
1. **Development**: Use HTTPS dev certificates for SignalR
2. **Staging**: Configure CORS for Angular development server
3. **Production**: IIS/Kestrel configuration with proper WebSocket support
4. **Monitoring**: Health check endpoints for system monitoring

This big picture plan ensures seamless integration between the backend session management system and the frontend Angular application, with clear separation of concerns and well-defined integration points.