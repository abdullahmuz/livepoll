using LivePoll.Api.DTOs;

namespace LivePoll.Api.Services;

public interface ISessionService
{
    Task<SessionResponseDto> CreateAsync(int hostId, CreateSessionDto dto);
    Task<List<SessionSummaryDto>> GetMineAsync(int hostId);
    Task<SessionResponseDto?> GetByIdAsync(int hostId, int sessionId);
    Task<string?> CloseAsync(int hostId, int sessionId);   // returns the join code, or null if not found
    Task<bool> DeleteAsync(int hostId, int sessionId);
}