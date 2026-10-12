namespace LivePoll.Api.Models;

public class Option
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public List<Vote> Votes { get; set; } = new();
}