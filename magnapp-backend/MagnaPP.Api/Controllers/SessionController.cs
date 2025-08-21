using Microsoft.AspNetCore.Mvc;
using MagnaPP.Api.DTOs;
using MagnaPP.Domain.Entities;
using MagnaPP.Domain.Enums;
using MagnaPP.Infrastructure.Services;

namespace MagnaPP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<SessionResponse>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            if (!await _sessionService.CanCreateNewSessionAsync())
            {
                return BadRequest(new { error = "Maximum number of concurrent sessions reached (3)" });
            }

            var creator = new User
            {
                Name = request.UserName,
                Avatar = request.Avatar,
                Role = UserRole.ScrumMaster
            };

            var session = await _sessionService.CreateSessionAsync(request.Name, creator);
            var response = MapToSessionResponse(session, creator.UserId);

            _logger.LogInformation("Session created: {SessionId} by {UserName}", session.SessionId, request.UserName);
            
            return CreatedAtAction(nameof(GetSession), new { id = session.SessionId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, new { error = "Failed to create session" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionResponse>> GetSession(Guid id, [FromQuery] Guid? userId = null)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id);
            if (session == null)
            {
                return NotFound(new { error = "Session not found or expired" });
            }

            var response = MapToSessionResponse(session, userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", id);
            return StatusCode(500, new { error = "Failed to retrieve session" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<SessionListResponse>> GetActiveSessions()
    {
        try
        {
            var sessions = await _sessionService.GetActiveSessionsAsync();
            var response = new SessionListResponse
            {
                Sessions = sessions.Select(s => new SessionSummary
                {
                    SessionId = s.SessionId,
                    Name = s.Name,
                    CreatedAt = s.CreatedAt,
                    UserCount = s.Users.Count,
                    MaxUsers = Session.MaxUsers
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions");
            return StatusCode(500, new { error = "Failed to retrieve sessions" });
        }
    }

    [HttpPost("{id:guid}/join")]
    public async Task<ActionResult<SessionResponse>> JoinSession(Guid id, [FromBody] JoinSessionRequest request)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id);
            if (session == null)
            {
                return NotFound(new { error = "Session not found or expired" });
            }

            if (!session.CanJoin)
            {
                return BadRequest(new { error = "Session is full or not accepting new users" });
            }

            if (session.Users.Any(u => u.Name.Equals(request.UserName, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { error = "Username is already taken in this session" });
            }

            var user = new User
            {
                Name = request.UserName,
                Avatar = request.Avatar,
                Role = UserRole.Participant
            };

            var success = await _sessionService.AddUserToSessionAsync(id, user);
            if (!success)
            {
                return BadRequest(new { error = "Failed to join session" });
            }

            // Get updated session
            session = await _sessionService.GetSessionAsync(id);
            var response = MapToSessionResponse(session!, user.UserId);

            _logger.LogInformation("User {UserName} joined session {SessionId}", request.UserName, id);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining session {SessionId}", id);
            return StatusCode(500, new { error = "Failed to join session" });
        }
    }

    [HttpPost("{id:guid}/leave")]
    public async Task<ActionResult> LeaveSession(Guid id, [FromBody] LeaveSessionRequest request)
    {
        try
        {
            var success = await _sessionService.RemoveUserFromSessionAsync(id, request.UserId);
            if (!success)
            {
                return NotFound(new { error = "User not found in session" });
            }

            _logger.LogInformation("User {UserId} left session {SessionId}", request.UserId, id);
            
            return Ok(new { message = "Successfully left session" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving session {SessionId}", id);
            return StatusCode(500, new { error = "Failed to leave session" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> EndSession(Guid id, [FromBody] EndSessionRequest request)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id);
            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Only Scrum Master can end session
            if (session.ScrumMasterId != request.UserId)
            {
                return Forbid();
            }

            session.EndSession();
            await _sessionService.UpdateSessionAsync(session);
            await _sessionService.DeleteSessionAsync(id);

            _logger.LogInformation("Session {SessionId} ended by {UserId}", id, request.UserId);
            
            return Ok(new { message = "Session ended successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session {SessionId}", id);
            return StatusCode(500, new { error = "Failed to end session" });
        }
    }

    private static SessionResponse MapToSessionResponse(Session session, Guid? currentUserId)
    {
        var currentRound = session.CurrentVotingRound;
        
        return new SessionResponse
        {
            SessionId = session.SessionId,
            Name = session.Name,
            CreatedAt = session.CreatedAt,
            Status = session.Status,
            CurrentRound = session.CurrentRound,
            ScrumMasterId = session.ScrumMasterId,
            Users = session.Users.Select(u => new UserResponse
            {
                UserId = u.UserId,
                Name = u.Name,
                Avatar = u.Avatar,
                Role = u.Role,
                JoinedAt = u.JoinedAt,
                IsConnected = u.IsConnected,
                Position = u.Position,
                HasVoted = currentRound?.HasUserVoted(u.UserId) ?? false
            }).ToList(),
            CurrentVotingRound = currentRound != null ? new VotingRoundResponse
            {
                RoundNumber = currentRound.RoundNumber,
                Status = currentRound.Status,
                StartedAt = currentRound.StartedAt,
                RevealedAt = currentRound.RevealedAt,
                Votes = currentRound.Status == VotingRoundStatus.Revealed ? 
                    currentRound.Votes.Select(v => new VoteResponse
                    {
                        UserId = v.UserId,
                        Value = v.Value,
                        SubmittedAt = v.SubmittedAt
                    }).ToList() : null,
                Statistics = currentRound.Status == VotingRoundStatus.Revealed ?
                    new VotingStatisticsResponse
                    {
                        Average = currentRound.GetAverageVote(),
                        Distribution = currentRound.GetVoteDistribution(),
                        HasConsensus = currentRound.HasConsensus(),
                        TotalVotes = currentRound.Votes.Count,
                        NumericVotes = currentRound.Votes.Count(v => v.IsNumericVote())
                    } : null
            } : null
        };
    }
}

public class LeaveSessionRequest
{
    public Guid UserId { get; set; }
}

public class EndSessionRequest
{
    public Guid UserId { get; set; }
}