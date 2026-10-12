using System.Security.Claims;
using LivePoll.Api.DTOs;
using LivePoll.Api.Models;
using LivePoll.Api.Services;
using LivePoll.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LivePoll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IHubContext<PollHub> _hubContext;

    public SessionsController(ISessionService sessionService, IHubContext<PollHub> hubContext)
    {
        _sessionService = sessionService;
        _hubContext = hubContext;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SessionResponseDto>>> Create(CreateSessionDto dto)
    {
        try
        {
            var session = await _sessionService.CreateAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = session.Id },
                ApiResponse<SessionResponseDto>.Ok(session, "Session created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SessionResponseDto>.Fail(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SessionSummaryDto>>>> GetMine()
    {
        var sessions = await _sessionService.GetMineAsync(GetUserId());
        return Ok(ApiResponse<List<SessionSummaryDto>>.Ok(sessions, "Sessions retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SessionResponseDto>>> GetById(int id)
    {
        var session = await _sessionService.GetByIdAsync(GetUserId(), id);

        if (session == null)
            return NotFound(ApiResponse<SessionResponseDto>.Fail($"Session {id} was not found."));

        return Ok(ApiResponse<SessionResponseDto>.Ok(session, "Session retrieved successfully"));
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<ApiResponse<object>>> Close(int id)
    {
        var joinCode = await _sessionService.CloseAsync(GetUserId(), id);

        if (joinCode == null)
            return NotFound(ApiResponse<object>.Fail($"Session {id} was not found."));

        await _hubContext.Clients.Group(PollHub.GroupFor(joinCode)).SendAsync("SessionClosed");

        return Ok(ApiResponse<object>.Ok(new { }, "Session closed"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _sessionService.DeleteAsync(GetUserId(), id);

        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Session {id} was not found."));

        return Ok(ApiResponse<object>.Ok(new { }, "Session deleted"));
    }
}