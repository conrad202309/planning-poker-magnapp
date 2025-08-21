using MagnaPP.Domain.Enums;

namespace MagnaPP.Api.DTOs;

public class SessionResponse
{
    public Guid SessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public SessionStatus Status { get; set; }
    public List<UserResponse> Users { get; set; } = new();
    public int CurrentRound { get; set; }
    public VotingRoundResponse? CurrentVotingRound { get; set; }
    public Guid? ScrumMasterId { get; set; }
}

public class UserResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsConnected { get; set; }
    public int Position { get; set; }
    public bool HasVoted { get; set; }
}

public class VotingRoundResponse
{
    public int RoundNumber { get; set; }
    public VotingRoundStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? RevealedAt { get; set; }
    public List<VoteResponse>? Votes { get; set; }
    public VotingStatisticsResponse? Statistics { get; set; }
}

public class VoteResponse
{
    public Guid UserId { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class VotingStatisticsResponse
{
    public double Average { get; set; }
    public Dictionary<string, int> Distribution { get; set; } = new();
    public bool HasConsensus { get; set; }
    public int TotalVotes { get; set; }
    public int NumericVotes { get; set; }
}