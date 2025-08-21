namespace MagnaPP.Api.DTOs;

public class SessionListResponse
{
    public List<SessionSummary> Sessions { get; set; } = new();
}

public class SessionSummary
{
    public Guid SessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public int MaxUsers { get; set; } = 16;
    public bool CanJoin => UserCount < MaxUsers;
}