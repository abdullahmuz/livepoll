using System.Security.Cryptography;
using LivePoll.Api.Data;
using LivePoll.Api.DTOs;
using LivePoll.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LivePoll.Api.Services;

public class SessionService : ISessionService
{
    // no 0/O or 1/I/L, so codes are easy to read out loud or type from a screen
    private const string CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 6;

    private readonly AppDbContext _context;

    public SessionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SessionResponseDto> CreateAsync(int hostId, CreateSessionDto dto)
    {
        // validate option text (annotations can't look inside list items)
        foreach (var q in dto.Questions)
        {
            var cleaned = q.Options.Select(o => o.Trim()).ToList();

            if (cleaned.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Options cannot be blank.");

            if (cleaned.Distinct(StringComparer.OrdinalIgnoreCase).Count() != cleaned.Count)
                throw new InvalidOperationException("A question cannot have duplicate options.");
        }

        // build the whole object graph in memory first
        var session = new Session
        {
            Title = dto.Title.Trim(),
            HostId = hostId,
            JoinCode = await GenerateUniqueJoinCodeAsync(),
            Questions = dto.Questions.Select((q, index) => new Question
            {
                Text = q.Text.Trim(),
                OrderIndex = index,
                Options = q.Options.Select(o => new Option { Text = o.Trim() }).ToList()
            }).ToList()
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return MapToDto(session);
    }

    public async Task<List<SessionSummaryDto>> GetMineAsync(int hostId)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.HostId == hostId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                Title = s.Title,
                JoinCode = s.JoinCode,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                QuestionCount = s.Questions.Count
            })
            .ToListAsync();
    }

    public async Task<SessionResponseDto?> GetByIdAsync(int hostId, int sessionId)
    {
        var session = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.HostId == hostId);

        return session == null ? null : MapToDto(session);
    }

    public async Task<string?> CloseAsync(int hostId, int sessionId)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.HostId == hostId);

        if (session == null) return null;

        session.IsActive = false;
        await _context.SaveChangesAsync();
        return session.JoinCode;
    }

    public async Task<bool> DeleteAsync(int hostId, int sessionId)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.HostId == hostId);

        if (session == null) return false;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateUniqueJoinCodeAsync()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = string.Create(CodeLength, CodeAlphabet, (span, alphabet) =>
            {
                for (var i = 0; i < span.Length; i++)
                    span[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            });

            if (!await _context.Sessions.AnyAsync(s => s.JoinCode == code))
                return code;
        }

        throw new InvalidOperationException("Could not generate a unique join code. Please try again.");
    }

    private static SessionResponseDto MapToDto(Session s) => new()
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
                Options = q.Options.Select(o => new OptionResponseDto { Id = o.Id, Text = o.Text }).ToList()
            }).ToList()
    };
}