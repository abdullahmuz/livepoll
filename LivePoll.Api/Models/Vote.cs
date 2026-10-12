namespace LivePoll.Api.Models;

public class Vote
{
    public int Id { get; set; }
    public string ParticipantId { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int OptionId { get; set; }
    public Option Option { get; set; } = null!;
}