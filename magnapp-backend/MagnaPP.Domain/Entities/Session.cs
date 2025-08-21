using MagnaPP.Domain.Enums;

namespace MagnaPP.Domain.Entities;

public class Session
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public List<User> Users { get; set; } = new();
    public List<VotingRound> VotingRounds { get; set; } = new();
    public int CurrentRound { get; set; } = 0;
    public Guid? ScrumMasterId { get; set; }

    public const int MaxUsers = 16;
    public static readonly TimeSpan InactivityTimeout = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ScrumMasterReconnectTimeout = TimeSpan.FromMinutes(5);

    public User? ScrumMaster => Users.FirstOrDefault(u => u.UserId == ScrumMasterId);

    public bool IsExpired => DateTime.UtcNow - LastActivity > InactivityTimeout;

    public bool CanJoin => Users.Count < MaxUsers && Status == SessionStatus.Active;

    public VotingRound? CurrentVotingRound => VotingRounds.FirstOrDefault(r => r.RoundNumber == CurrentRound);

    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    public bool AddUser(User user)
    {
        if (!CanJoin || Users.Any(u => u.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (!Users.Any())
        {
            user.Role = UserRole.ScrumMaster;
            ScrumMasterId = user.UserId;
        }

        user.Position = GetNextAvailablePosition();
        Users.Add(user);
        UpdateActivity();
        return true;
    }

    public bool RemoveUser(Guid userId)
    {
        var user = Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return false;

        Users.Remove(user);

        if (user.UserId == ScrumMasterId)
        {
            AssignNewScrumMaster();
        }

        UpdateActivity();
        return true;
    }

    public void StartVotingRound()
    {
        if (ScrumMasterId == null || Status != SessionStatus.Active) return;

        CurrentRound++;
        var round = new VotingRound { RoundNumber = CurrentRound };
        round.StartRound();
        VotingRounds.Add(round);
        UpdateActivity();
    }

    public void RevealVotes()
    {
        var round = CurrentVotingRound;
        if (round?.Status == VotingRoundStatus.InProgress)
        {
            round.RevealVotes();
            UpdateActivity();
        }
    }

    public void SubmitVote(Guid userId, string voteValue)
    {
        var round = CurrentVotingRound;
        if (round?.Status == VotingRoundStatus.InProgress)
        {
            round.SubmitVote(userId, voteValue);
            UpdateActivity();
        }
    }

    public void PauseSession()
    {
        Status = SessionStatus.Paused;
        UpdateActivity();
    }

    public void ResumeSession()
    {
        Status = SessionStatus.Active;
        UpdateActivity();
    }

    public void EndSession()
    {
        Status = SessionStatus.Ended;
        UpdateActivity();
    }

    public bool TransferScrumMasterRole(Guid newScrumMasterId)
    {
        var newScrumMaster = Users.FirstOrDefault(u => u.UserId == newScrumMasterId);
        if (newScrumMaster == null) return false;

        if (ScrumMaster != null)
        {
            ScrumMaster.Role = UserRole.Participant;
        }

        newScrumMaster.Role = UserRole.ScrumMaster;
        ScrumMasterId = newScrumMasterId;
        UpdateActivity();
        return true;
    }

    private void AssignNewScrumMaster()
    {
        var longestConnectedUser = Users
            .Where(u => u.IsConnected)
            .OrderBy(u => u.JoinedAt)
            .FirstOrDefault();

        if (longestConnectedUser != null)
        {
            longestConnectedUser.Role = UserRole.ScrumMaster;
            ScrumMasterId = longestConnectedUser.UserId;
        }
        else
        {
            ScrumMasterId = null;
            PauseSession();
        }
    }

    private int GetNextAvailablePosition()
    {
        var occupiedPositions = Users.Select(u => u.Position).ToHashSet();
        for (int i = 1; i <= MaxUsers; i++)
        {
            if (!occupiedPositions.Contains(i))
                return i;
        }
        return Users.Count + 1;
    }
}