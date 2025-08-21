using MagnaPP.Domain.Enums;

namespace MagnaPP.Domain.Entities;

public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Participant;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public bool IsConnected { get; set; } = true;
    public string? ConnectionId { get; set; }
    public int Position { get; set; }

    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    public void SetAsDisconnected()
    {
        IsConnected = false;
        ConnectionId = null;
    }

    public void SetAsConnected(string connectionId)
    {
        IsConnected = true;
        ConnectionId = connectionId;
        UpdateActivity();
    }
}