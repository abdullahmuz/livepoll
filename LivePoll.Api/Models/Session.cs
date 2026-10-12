namespace LivePoll.Api.Models;

public class Session
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int HostId { get; set; }
    public User Host { get; set; } = null!;

    public List<Question> Questions { get; set; } = new();
}