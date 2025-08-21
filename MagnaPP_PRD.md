# Product Requirements Document (PRD)
## MagnaPP Web Application

---

## 1. Introduction & Overview

### 1.1 Product Name
**MagnaPP** (Agile Planning Poker Platform)

### 1.2 Problem Statement
Distributed agile development teams need an effective way to conduct Planning Poker estimation sessions remotely. Current solutions often lack real-time collaboration features or provide poor user experiences that hinder the estimation process.

### 1.3 Solution Overview
MagnaPP is a web-based Planning Poker application that enables multiple users to participate in real-time estimation sessions through a virtual boardroom interface. The application provides seamless collaboration with immediate visual feedback, session management, and reconnection capabilities.

### 1.4 Success Vision
Teams can easily create and join MagnaPP sessions, conduct multiple estimation rounds efficiently, and maintain engagement through an intuitive virtual boardroom experience.

---

## 2. Goals & Objectives

### 2.1 Primary Goals
- Enable real-time collaborative Planning Poker sessions for up to 16 users
- Provide an intuitive virtual boardroom experience that mimics in-person sessions
- Ensure seamless user experience across desktop and mobile devices
- Minimize setup time and technical barriers for users

### 2.2 Success Metrics
- Session completion rate: >95% of started sessions complete at least one voting round
- User retention: Users who join a session stay for the full duration >90% of the time
- Reconnection success: >98% of disconnected users successfully rejoin their session
- Cross-device functionality: Application works seamlessly on desktop and mobile

---

## 3. Target Audience & User Personas

### 3.1 Primary Users
**Agile Development Teams** (3-16 members)
- Scrum Masters, Product Owners, Developers, QA Engineers
- Teams working remotely or in hybrid environments
- Organizations using agile methodologies (Scrum, SAFe, etc.)

### 3.2 User Personas

#### Persona 1: Sarah - Scrum Master
- **Role**: Facilitates MagnaPP sessions
- **Goals**: Run efficient estimation sessions, maintain team engagement, keep discussions focused
- **Pain Points**: Coordinating remote team members, ensuring everyone participates
- **Technical Comfort**: High

#### Persona 2: Mike - Developer
- **Role**: Participates in estimation sessions
- **Goals**: Provide accurate estimates, understand story requirements
- **Pain Points**: Connection issues disrupting flow, unclear voting status
- **Technical Comfort**: High

#### Persona 3: Lisa - Product Owner
- **Role**: Participates in estimation, provides clarification
- **Goals**: Ensure team understands requirements, get reliable estimates
- **Pain Points**: Difficulty seeing who has voted, losing session context
- **Technical Comfort**: Medium

---

## 4. User Stories & Use Cases

### 4.1 Epic 1: User Registration & Session Access

#### User Story 1.1: Initial User Setup
**As a** new user  
**I want to** enter my name/alias and choose an avatar  
**So that** I can be identified in MagnaPP sessions  

**Acceptance Criteria:**
- User must enter a name/alias (minimum 2 characters, maximum 50 characters)
- User can select from available icon library for avatar
- User settings are stored in browser local storage
- Settings persist across browser sessions
- Name validation prevents empty, whitespace-only, or special character names

#### User Story 1.2: Session Creation
**As a** Scrum Master  
**I want to** create a new MagnaPP session  
**So that** my team can join and participate in estimation  

**Acceptance Criteria:**
- User can enter session name (required, 3-100 characters)
- System generates unique GUID for session ID
- System provides shareable link for the session
- User becomes Scrum Master with full session control
- Session is created in application memory
- Session expiry timer starts at 10 minutes of inactivity

#### User Story 1.3: Session Discovery
**As a** user  
**I want to** browse existing active sessions  
**So that** I can join a session without needing a direct link  

**Acceptance Criteria:**
- Display list of active sessions with names only
- Show sessions sorted by creation time (newest first)
- Allow user to select and join any active session
- Update list dynamically as sessions are created/expire
- Handle empty state when no sessions exist

#### User Story 1.4: Direct Session Joining
**As a** team member  
**I want to** join a session using a shared link  
**So that** I can quickly access the MagnaPP session  

**Acceptance Criteria:**
- Link format: `/session/{GUID}`
- If user has saved settings, auto-populate name/avatar
- If no saved settings, prompt for name/alias entry before joining
- Validate session exists and is active
- Handle expired or invalid session links with appropriate error message
- Redirect to session boardroom upon successful join

### 4.2 Epic 2: Session Management

#### User Story 2.1: Scrum Master Controls
**As a** Scrum Master  
**I want to** control the MagnaPP session flow  
**So that** I can facilitate effective estimation discussions  

**Acceptance Criteria:**
- Can start a new voting round for all participants
- Can reveal all votes simultaneously
- Can start a new round (resets all votes and voting status)
- Can end the session (returns all users to home screen)
- Can kick users from the session
- Can transfer Scrum Master role to another participant
- All actions are immediately reflected to all session participants

#### User Story 2.2: Session Inactivity Management
**As a** system administrator  
**I want** sessions to expire after inactivity  
**So that** system resources are managed efficiently  

**Acceptance Criteria:**
- Session expires after 10 minutes of no user activity
- Warning timer appears when 2 minutes remain
- All users see the countdown timer
- Expired sessions are removed from memory
- Users in expired sessions are redirected to home screen
- Activity includes: voting, revealing votes, starting new rounds, user joins/leaves

#### User Story 2.3: Scrum Master Disconnection Handling
**As a** session participant  
**I want** the session to wait for Scrum Master reconnection  
**So that** we don't lose session control or progress  

**Acceptance Criteria:**
- When Scrum Master disconnects, session enters "paused" state
- All participants see "Waiting for Scrum Master to return" message
- Session controls are disabled for all users
- Scrum Master can reconnect and resume session within timeout period
- If Scrum Master doesn't return within 5 minutes, longest-connected user becomes new Scrum Master
- Automatic role transfer is announced to all participants

### 4.3 Epic 3: Virtual Boardroom Experience

#### User Story 3.1: Boardroom Visualization
**As a** session participant  
**I want to** see all users arranged around a virtual table  
**So that** I have a familiar meeting room experience  

**Acceptance Criteria:**
- Users displayed as avatars around an oval table
- Maximum 16 user positions around the table
- Avatar shows icon selected by user
- Username displayed below avatar
- Users positioned randomly around table when joining
- Scrum Master has visual indicator (crown icon or similar)
- Responsive layout works on desktop, tablet, and mobile

#### User Story 3.2: Voting Status Indicators
**As a** session participant  
**I want to** see who has voted in the current round  
**So that** I know the voting progress without seeing actual votes  

**Acceptance Criteria:**
- Each user has a visual indicator showing voted/not voted status
- Green indicator for "has voted"
- Red indicator for "not voted" (only during active voting)
- No indicator when not in voting round
- Indicators update in real-time as users submit votes
- My own vote is visible to me but not to others until reveal

### 4.4 Epic 4: Voting Process

#### User Story 4.1: Voting Round Initiation
**As a** Scrum Master  
**I want to** start a voting round  
**So that** all team members can submit their estimates  

**Acceptance Criteria:**
- "Start Voting" button visible only to Scrum Master
- Button disabled during active voting or when results are shown
- Clicking starts voting round for all participants
- All users immediately see voting cards interface
- Audio notification plays for all users when voting starts
- Voting status indicators reset for all users

#### User Story 4.2: Vote Submission
**As a** session participant  
**I want to** select my estimate using voting cards  
**So that** I can participate in the estimation process  

**Acceptance Criteria:**
- Voting cards display Fibonacci sequence: 1, 2, 3, 5, 8, 13, 21
- Additional "Coffee" card for break requests
- Cards are visually distinct and clickable
- Selected card is highlighted/emphasized
- Can change vote until Scrum Master reveals votes
- My vote appears next to my avatar (visible only to me)
- Audio feedback when card is selected
- Vote is immediately saved and status indicator updates

#### User Story 4.3: Vote Revelation
**As a** Scrum Master  
**I want to** reveal all votes simultaneously  
**So that** we can discuss estimates and reach consensus  

**Acceptance Criteria:**
- "Reveal Votes" button visible only to Scrum Master
- Button enabled only when at least one vote is submitted
- All votes revealed simultaneously across all clients
- Vote cards "flip over" next to each user's avatar to show their vote
- Vote statistics panel appears showing average and distribution
- Audio notification plays when votes are revealed
- Voting cards interface is hidden during results display

#### User Story 4.4: Vote Statistics
**As a** session participant  
**I want to** see voting statistics after revelation  
**So that** I can understand the team's estimation consensus  

**Acceptance Criteria:**
- Display average of numeric votes (excluding Coffee votes)
- Show vote distribution (count of each vote value)
- Indicate consensus when all votes are identical
- Coffee votes counted separately
- Statistics update automatically when votes are revealed
- Visual representation (simple bar chart or similar) for distribution

### 4.5 Epic 5: Connection Management

#### User Story 5.1: User Reconnection
**As a** session participant  
**I want to** rejoin my session after losing connection  
**So that** I can continue participating without disruption  

**Acceptance Criteria:**
- User can rejoin session using same name/avatar
- Rejoined user sees current session state (voting round, results, etc.)
- If voting in progress and user had voted, show their previous vote
- If voting in progress and user hadn't voted, allow them to vote
- Other participants see reconnection notification
- User resumes at their previous position around the table

#### User Story 5.2: Real-time Synchronization
**As a** session participant  
**I want** all session changes to appear immediately  
**So that** I have a seamless collaborative experience  

**Acceptance Criteria:**
- All user actions sync in real-time (<1 second delay)
- Vote submissions immediately update voting indicators
- Scrum Master actions (start voting, reveal votes) appear instantly
- User joins/leaves reflect immediately in boardroom
- Session state maintained consistently across all clients
- Connection status visible to users (connected/reconnecting indicator)

---

## 5. Functional Requirements

### 5.1 User Management
- **FR-1.1**: System shall allow users to set name/alias (2-50 characters, alphanumeric and spaces only)
- **FR-1.2**: System shall provide icon selection for user avatars
- **FR-1.3**: System shall store user preferences in browser storage with 30-day expiration
- **FR-1.4**: System shall validate user input and provide appropriate error messages

### 5.2 Session Management
- **FR-2.1**: System shall generate unique GUID identifiers for each session
- **FR-2.2**: System shall store sessions in application memory only
- **FR-2.3**: System shall expire sessions after 10 minutes of inactivity
- **FR-2.4**: System shall support maximum 16 users per session
- **FR-2.5**: System shall provide session discovery interface showing active sessions
- **FR-2.6**: System shall generate shareable session links in format `/session/{GUID}`

### 5.3 Role Management
- **FR-3.1**: System shall assign Scrum Master role to session creator
- **FR-3.2**: System shall allow Scrum Master to transfer role to any session participant
- **FR-3.3**: System shall allow Scrum Master to remove users from session
- **FR-3.4**: System shall pause session when Scrum Master disconnects
- **FR-3.5**: System shall auto-assign Scrum Master role after 5-minute timeout

### 5.4 Voting System
- **FR-4.1**: System shall provide Fibonacci voting cards: 1, 2, 3, 5, 8, 13, 21, Coffee
- **FR-4.2**: System shall hide individual votes until Scrum Master reveals them
- **FR-4.3**: System shall allow vote changes until revelation
- **FR-4.4**: System shall calculate and display vote statistics (average, distribution)
- **FR-4.5**: System shall support multiple voting rounds per session
- **FR-4.6**: System shall reset voting state when new round starts

### 5.5 Real-time Communication
- **FR-5.1**: System shall provide real-time updates using WebSocket or similar protocol
- **FR-5.2**: System shall synchronize all user actions across clients within 1 second
- **FR-5.3**: System shall handle connection drops and automatic reconnection
- **FR-5.4**: System shall maintain session state during user disconnections

### 5.6 User Interface
- **FR-6.1**: System shall display users as avatars around oval table layout
- **FR-6.2**: System shall show voting status indicators for each user
- **FR-6.3**: System shall provide responsive design for desktop, tablet, and mobile
- **FR-6.4**: System shall display session timer showing time remaining before expiry
- **FR-6.5**: System shall provide audio notifications for voting events

---

## 6. Non-Functional Requirements

### 6.1 Performance
- **NFR-1.1**: Application shall support maximum 3 concurrent sessions
- **NFR-1.2**: Real-time updates shall have <1 second latency
- **NFR-1.3**: Application shall handle 48 concurrent users (3 sessions × 16 users)
- **NFR-1.4**: Session state changes shall propagate to all clients within 1 second

### 6.2 Reliability
- **NFR-2.1**: Application shall have 99% uptime during business hours
- **NFR-2.2**: System shall gracefully handle user disconnections
- **NFR-2.3**: Application shall recover from temporary network issues automatically

### 6.3 Usability
- **NFR-3.1**: Application shall be intuitive enough for first-time users without training
- **NFR-3.2**: Critical user actions shall be completed in ≤3 clicks
- **NFR-3.3**: Application shall provide clear visual feedback for all user actions
- **NFR-3.4**: Error messages shall be user-friendly and actionable

### 6.4 Compatibility
- **NFR-4.1**: Application shall support Chrome, Edge, Firefox, Safari (current versions)
- **NFR-4.2**: Application shall work on desktop, tablet, and mobile devices
- **NFR-4.3**: Application shall function without requiring plugins or extensions

### 6.5 Security
- **NFR-5.1**: Application shall validate all user inputs to prevent injection attacks
- **NFR-5.2**: Session data shall be contained within application memory only
- **NFR-5.3**: No persistent storage of user data beyond browser cookies

---

## 7. Technical Requirements

### 7.1 System Components
- **Backend**: Web API service with RESTful endpoints
- **Frontend**: Web application with responsive design
- **Real-time Communication**: WebSocket-based real-time updates
- **Data Storage**: In-memory session storage (no persistent database)
- **Hosting**: Web server in corporate environment

### 7.2 Architecture Requirements
- **TR-1.1**: RESTful API design for HTTP endpoints
- **TR-1.2**: Real-time communication channels for live updates
- **TR-1.3**: Stateless backend with session data in memory
- **TR-1.4**: Single Page Application (SPA) architecture

### 7.3 Data Requirements
- **TR-2.1**: Session data stored in application memory with automatic cleanup
- **TR-2.2**: User preferences stored in browser cookies only
- **TR-2.3**: No persistent data storage required
- **TR-2.4**: Session timeout configurable (default 10 minutes)

---

## 8. User Experience Requirements

### 8.1 User Interface Design
- Clean, modern interface with consistent design system
- Responsive grid layout and utilities
- Oval table visualization for virtual boardroom
- Intuitive voting card interface with visual feedback
- Clear status indicators and progress feedback

### 8.2 User Journey Flow
1. **Entry**: User enters name/avatar or uses saved settings
2. **Session Selection**: User creates new session or joins existing
3. **Boardroom**: User sees virtual table with all participants
4. **Voting**: Scrum Master starts round, users submit votes
5. **Results**: Scrum Master reveals votes, team sees statistics
6. **Iteration**: Multiple rounds as needed
7. **Conclusion**: Scrum Master ends session

### 8.3 Accessibility Requirements
- Keyboard navigation support
- Screen reader compatibility for essential functions
- High contrast mode compatibility
- Light/Dark mode option
- Mobile touch interface optimization

---

## 9. Success Metrics & KPIs

### 9.1 Usage Metrics
- Number of sessions created per week
- Average session duration
- Average number of voting rounds per session
- User retention rate within sessions

### 9.2 Performance Metrics
- Page load time <2 seconds
- Real-time update latency <1 second
- Connection success rate >98%
- Session completion rate >95%

### 9.3 Quality Metrics
- User-reported issues per session <0.1
- Successful reconnection rate >95%
- Cross-browser compatibility issues <1%

---

## 10. Risk Assessment & Mitigation

### 10.1 Technical Risks
- **Risk**: WebSocket connection instability
- **Mitigation**: Implement robust reconnection logic with exponential backoff

- **Risk**: Memory leaks from session storage
- **Mitigation**: Implement automatic cleanup and monitoring

### 10.2 User Experience Risks
- **Risk**: Poor mobile experience
- **Mitigation**: Mobile-first responsive design and testing

- **Risk**: Confusing voting interface
- **Mitigation**: User testing and iterative UI improvements

---

## 11. Development Phases & Priorities

### Phase 1: Core Foundation (MVP)
- User registration and session creation
- Basic boardroom visualization
- Simple voting functionality
- Session management

### Phase 2: Enhanced Experience
- Real-time synchronization improvements
- Advanced UI/UX features
- Mobile optimization
- Audio notifications

### Phase 3: Polish & Optimization
- Performance optimization
- Advanced statistics
- Error handling improvements
- Accessibility features

---

This PRD provides comprehensive detail for development planning while maintaining clarity for implementation. Each user story includes specific acceptance criteria that can be directly translated into development tasks and test cases for the MagnaPP application.