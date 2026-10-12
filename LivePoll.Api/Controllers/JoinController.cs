using LivePoll.Api.DTOs;
using LivePoll.Api.Models;
using LivePoll.Api.Services;
using LivePoll.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LivePoll.Api.Controllers;

[ApiController]
[Route("api/join")]
public class JoinController : ControllerBase
{
    private readonly IParticipantService _participantService;
    private readonly IHubContext<PollHub> _hubContext;

    public JoinController(IParticipantService participantService, IHubContext<PollHub> hubContext)
    {
        _participantService = participantService;
        _hubContext = hubContext;
    }

    // GET api/join/X7K2P9
    [HttpGet("{code}")]
    public async Task<ActionResult<ApiResponse<SessionResponseDto>>> GetSession(string code)
    {
        var session = await _participantService.GetSessionAsync(code);

        if (session == null)
            return NotFound(ApiResponse<SessionResponseDto>.Fail("No session found with that code."));

        return Ok(ApiResponse<SessionResponseDto>.Ok(session, "Session found"));
    }

    // POST api/join/X7K2P9/vote
    [HttpPost("{code}/vote")]
    public async Task<ActionResult<ApiResponse<QuestionResultsDto>>> Vote(string code, VoteDto dto)
    {
        try
        {
            var results = await _participantService.VoteAsync(code, dto);

            if (results == null)
                return NotFound(ApiResponse<QuestionResultsDto>.Fail("No session found with that code."));

            // push the fresh numbers to everyone watching this poll
            await _hubContext.Clients.Group(PollHub.GroupFor(code))
                .SendAsync("QuestionResultsUpdated", results);

            return Ok(ApiResponse<QuestionResultsDto>.Ok(results, "Vote recorded"));
        }
        catch (AlreadyVotedException ex)
        {
            return Conflict(ApiResponse<QuestionResultsDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<QuestionResultsDto>.Fail(ex.Message));
        }
    }

    // GET api/join/X7K2P9/results
    [HttpGet("{code}/results")]
    public async Task<ActionResult<ApiResponse<SessionResultsDto>>> GetResults(string code)
    {
        var results = await _participantService.GetResultsAsync(code);

        if (results == null)
            return NotFound(ApiResponse<SessionResultsDto>.Fail("No session found with that code."));

        return Ok(ApiResponse<SessionResultsDto>.Ok(results, "Results retrieved"));
    }
}