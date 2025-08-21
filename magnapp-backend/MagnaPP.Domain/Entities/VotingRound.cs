using MagnaPP.Domain.Enums;

namespace MagnaPP.Domain.Entities;

public class VotingRound
{
    public int RoundNumber { get; set; }
    public VotingRoundStatus Status { get; set; } = VotingRoundStatus.NotStarted;
    public DateTime StartedAt { get; set; }
    public DateTime? RevealedAt { get; set; }
    public List<Vote> Votes { get; set; } = new();

    public void StartRound()
    {
        Status = VotingRoundStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        Votes.Clear();
    }

    public void RevealVotes()
    {
        Status = VotingRoundStatus.Revealed;
        RevealedAt = DateTime.UtcNow;
    }

    public void SubmitVote(Guid userId, string voteValue)
    {
        if (Status != VotingRoundStatus.InProgress) return;
        
        var existingVote = Votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote != null)
        {
            existingVote.Value = voteValue;
            existingVote.SubmittedAt = DateTime.UtcNow;
        }
        else
        {
            Votes.Add(new Vote { UserId = userId, Value = voteValue });
        }
    }

    public bool HasUserVoted(Guid userId) => Votes.Any(v => v.UserId == userId);

    public Dictionary<string, int> GetVoteDistribution()
    {
        return Votes.GroupBy(v => v.Value).ToDictionary(g => g.Key, g => g.Count());
    }

    public double GetAverageVote()
    {
        var numericVotes = Votes.Where(v => v.IsNumericVote()).ToList();
        return numericVotes.Any() ? numericVotes.Average(v => v.GetNumericValue()) : 0;
    }

    public bool HasConsensus()
    {
        var numericVotes = Votes.Where(v => v.IsNumericVote()).Select(v => v.Value).Distinct().ToList();
        return numericVotes.Count == 1;
    }
}