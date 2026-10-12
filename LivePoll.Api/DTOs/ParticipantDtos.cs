using System.ComponentModel.DataAnnotations;

namespace LivePoll.Api.DTOs;

public class VoteDto
{
    [Required(ErrorMessage = "Participant id is required.")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "Participant id must be 8 to 64 characters.")]
    public string ParticipantId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "QuestionId is required.")]
    public int QuestionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "OptionId is required.")]
    public int OptionId { get; set; }
}

public class OptionResultDto
{
    public int OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
    public double Percentage { get; set; }
}

public class QuestionResultsDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public List<OptionResultDto> Options { get; set; } = new();
}

public class SessionResultsDto
{
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<QuestionResultsDto> Questions { get; set; } = new();
}