# Technology Stack Decisions

## Final Technology Stack

### Backend
- **Framework**: .NET Core 8 Web API
- **Real-time Communication**: SignalR
- **Data Storage**: IMemoryCache (in-memory only)
- **Authentication**: None (anonymous sessions)
- **Hosting**: Corporate web server

### Frontend
- **Framework**: Angular 17+ with TypeScript
- **UI Framework**: Angular Material + Bootstrap 5
- **Real-time Client**: @microsoft/signalr
- **State Management**: Angular Services + RxJS
- **Build Tool**: Angular CLI

### Architecture Decisions

#### Why .NET Core + SignalR?
- Native real-time WebSocket support with automatic fallbacks
- Excellent performance for 48 concurrent users
- Built-in memory caching for session storage
- Strong typing and enterprise-grade reliability
- Easy deployment to corporate environments

#### Why Angular + TypeScript?
- Robust enterprise framework with excellent tooling
- Built-in TypeScript support for type safety
- RxJS integration perfect for real-time data streams
- Powerful reactive forms for voting interface
- Strong dependency injection for service architecture

#### Why Angular Material + Bootstrap?
- Angular Material: Official components with built-in accessibility
- Bootstrap: Proven responsive grid system and utilities
- Combined: Consistent design system with maximum flexibility
- Mobile-first responsive design capabilities

### Project Structure
```
MagnaPP/
├── backend/                 # .NET Core Web API
│   ├── MagnaPP.Api/        # Main API project
│   ├── MagnaPP.Core/       # Business logic
│   └── MagnaPP.Hubs/       # SignalR hubs
├── frontend/               # Angular application
│   ├── src/app/           # Angular components
│   └── src/environments/  # Environment configs
└── docs/                  # Documentation
```

### Development Workflow
- Backend: `dotnet run` for API server
- Frontend: `ng serve` for development server
- SignalR endpoint: `/hub/session` for real-time communication
- API base: `/api/` for REST endpoints