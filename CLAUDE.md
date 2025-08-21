# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MagnaPP is a web-based Planning Poker application for distributed agile teams. The system enables real-time collaborative estimation sessions through a virtual boardroom interface supporting up to 16 concurrent users per session.

## Core Architecture

### System Components
- **Frontend**: Single Page Application (SPA) with responsive design
- **Backend**: RESTful Web API service
- **Real-time Communication**: WebSocket-based for live updates
- **Data Storage**: In-memory session storage only (no persistent database)
- **Session Management**: GUID-based sessions with 10-minute inactivity timeout

### Key Technical Constraints
- Maximum 3 concurrent sessions
- Maximum 16 users per session  
- Session data stored in memory only
- User preferences stored in browser local storage
- No persistent database - all data is transient

## Development Setup

## Technology Stack & Commands

**Backend**: .NET 9 Web API with SignalR (located in `magnapp-backend/`)
- Build: `dotnet build`
- Run API: `cd magnapp-backend/MagnaPP.Api && dotnet run`
- API runs on: `http://localhost:5102`
- Swagger docs: `http://localhost:5102/swagger`

**Frontend**: Angular (to be implemented)

## Implementation Guidelines

### Session Management
- Sessions use GUID identifiers
- Implement automatic cleanup after 10 minutes of inactivity
- Handle Scrum Master disconnection with 5-minute grace period
- Support user reconnection to active sessions

### Real-time Features
- All actions must sync across clients within 1 second
- Implement WebSocket reconnection logic with exponential backoff
- Maintain session state consistency during disconnections

### User Interface Requirements
- Virtual boardroom with oval table visualization
- Fibonacci voting cards: 1, 2, 3, 5, 8, 13, 21, plus "Coffee" option
- Voting status indicators (green: voted, red: not voted)
- Responsive design for desktop, tablet, and mobile

### Voting Process Flow
1. Scrum Master starts voting round
2. Participants select estimate cards
3. Votes remain hidden until reveal
4. Scrum Master reveals all votes simultaneously
5. Statistics displayed (average, distribution, consensus)
6. New round can be initiated

### User Roles
- **Scrum Master**: Full session control (start/reveal votes, kick users, end session)
- **Participants**: Can vote and view session state
- Scrum Master role can be transferred to another participant

## Testing Considerations

When implementing tests, ensure coverage for:
- Real-time synchronization across multiple clients
- Session expiry and cleanup
- User reconnection scenarios
- Scrum Master role transfer
- Vote calculation and statistics
- Maximum capacity limits (3 sessions, 16 users/session)