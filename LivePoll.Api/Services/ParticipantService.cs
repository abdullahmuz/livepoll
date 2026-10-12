using LivePoll.Api.Data;
using LivePoll.Api.DTOs;
using LivePoll.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LivePoll.Api.Services;

// a specific kind of InvalidOperationException, so the controller can return 409 for it
public class AlreadyVotedException : InvalidOperationException
{
    public AlreadyVotedException() : base("You have already voted on this question.") { }
}

public class ParticipantService : IParticipantService
{
    private readonly AppDbContext _context;

    public ParticipantService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SessionResponseDto?> GetSessionAsync(string joinCode)
    {
        var code = Normalize(joinCode);

        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.JoinCode == code)
            .Select(s => new SessionResponseDto
            {
                Id = s.Id,
                Title = s.Title,
                JoinCode = s.JoinCode,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                Questions = s.Questions
                    .OrderBy(q => q.OrderIndex)
                    .Select(q => new QuestionResponseDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        OrderIndex = q.OrderIndex,
                        Options = q.Options
                            .OrderBy(o => o.Id)
                            .Select(o => new OptionResponseDto { Id = o.Id, Text = o.Text })
                            .ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<QuestionResultsDto?> VoteAsync(string joinCode, VoteDto dto)
    {
        var code = Normalize(joinCode);

        var session = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.JoinCode == code)
            .Select(s => new { s.Id, s.IsActive })
            .FirstOrDefaultAsync();

        if (session == null) return null;

        if (!session.IsActive)
            throw new InvalidOperationException("This poll is closed.");

        // the question must belong to THIS session, and the option to THIS question
        var validChoice = await _context.Options.AnyAsync(o =>
            o.Id == dto.OptionId &&
            o.QuestionId == dto.QuestionId &&
            o.Question.SessionId == session.Id);

        if (!validChoice)
            throw new InvalidOperationException("That question or option doesn't belong to this poll.");

        // friendly fast-path check
        var alreadyVoted = await _context.Votes.AnyAsync(v =>
            v.ParticipantId == dto.ParticipantId && v.QuestionId == dto.QuestionId);

        if (alreadyVoted)
            throw new AlreadyVotedException();

        _context.Votes.Add(new Vote
        {
            ParticipantId = dto.ParticipantId,
            QuestionId = dto.QuestionId,
            OptionId = dto.OptionId
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // two identical votes raced past the check above; the unique index stopped the second
            throw new AlreadyVotedException();
        }

        var results = await QuestionResultsQuery(
                _context.Questions.AsNoTracking().Where(q => q.Id == dto.QuestionId))
            .FirstAsync();

        return WithPercentages(results);
    }

    public async Task<SessionResultsDto?> GetResultsAsync(string joinCode)
    {
        var code = Normalize(joinCode);

        var session = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.JoinCode == code)
            .Select(s => new { s.Id, s.Title, s.IsActive })
            .FirstOrDefaultAsync();

        if (session == null) return null;

        var questions = await QuestionResultsQuery(
                _context.Questions.AsNoTracking().Where(q => q.SessionId == session.Id))
            .ToListAsync();

        return new SessionResultsDto
        {
            Title = session.Title,
            IsActive = session.IsActive,
            Questions = questions.Select(WithPercentages).ToList()
        };
    }

    // one reusable query shape: questions -> options -> vote counts, all counted in SQL
    private static IQueryable<QuestionResultsDto> QuestionResultsQuery(IQueryable<Question> questions) =>
        questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionResultsDto
            {
                QuestionId = q.Id,
                Text = q.Text,
                Options = q.Options
                    .OrderBy(o => o.Id)
                    .Select(o => new OptionResultDto
                    {
                        OptionId = o.Id,
                        Text = o.Text,
                        Votes = o.Votes.Count
                    }).ToList()
            });

    private static QuestionResultsDto WithPercentages(QuestionResultsDto q)
    {
        q.TotalVotes = q.Options.Sum(o => o.Votes);

        foreach (var option in q.Options)
            option.Percentage = q.TotalVotes == 0
                ? 0
                : Math.Round(option.Votes * 100.0 / q.TotalVotes, 1);

        return q;
    }

    private static string Normalize(string joinCode) => joinCode.Trim().ToUpperInvariant();
}