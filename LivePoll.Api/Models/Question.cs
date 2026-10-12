namespace LivePoll.Api.Models;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public List<Option> Options { get; set; } = new();
}