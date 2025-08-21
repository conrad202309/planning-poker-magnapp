namespace MagnaPP.Domain.Entities;

public class Vote
{
    public Guid UserId { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public bool IsNumericVote() => int.TryParse(Value, out _);
    
    public int GetNumericValue() => IsNumericVote() ? int.Parse(Value) : 0;
}