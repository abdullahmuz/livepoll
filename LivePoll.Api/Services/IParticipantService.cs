using LivePoll.Api.DTOs;

namespace LivePoll.Api.Services;

public interface IParticipantService
{
    Task<SessionResponseDto?> GetSessionAsync(string joinCode);
    Task<QuestionResultsDto?> VoteAsync(string joinCode, VoteDto dto);
    Task<SessionResultsDto?> GetResultsAsync(string joinCode);
}