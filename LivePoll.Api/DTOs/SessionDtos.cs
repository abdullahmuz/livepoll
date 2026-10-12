using System.ComponentModel.DataAnnotations;

namespace LivePoll.Api.DTOs;

// ---------- incoming ----------
public class CreateQuestionDto
{
    [Required(ErrorMessage = "Question text is required.")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "Question text must be 3 to 300 characters.")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Each question needs a list of options.")]
    [MinLength(2, ErrorMessage = "Each question needs at least 2 options.")]
    [MaxLength(10, ErrorMessage = "A question can have at most 10 options.")]
    public List<string> Options { get; set; } = new();
}

public class CreateSessionDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be 3 to 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A session needs questions.")]
    [MinLength(1, ErrorMessage = "A session needs at least one question.")]
    [MaxLength(20, ErrorMessage = "A session can have at most 20 questions.")]
    public List<CreateQuestionDto> Questions { get; set; } = new();
}

// ---------- outgoing ----------
public class OptionResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class QuestionResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public List<OptionResponseDto> Options { get; set; } = new();
}

public class SessionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuestionResponseDto> Questions { get; set; } = new();
}

// lighter version for the "my sessions" list
public class SessionSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
}