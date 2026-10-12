using LivePoll.Api.DTOs;
using LivePoll.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace LivePoll.Api.Hubs;

public class PollHub : Hub
{
    private readonly IParticipantService _participantService;

    public PollHub(IParticipantService participantService)
    {
        _participantService = participantService;
    }

    // one place that decides what a group is called, so hub and controllers always agree
    public static string GroupFor(string joinCode) => joinCode.Trim().ToUpperInvariant();

    // clients call this right after connecting
    public async Task<SessionResultsDto> JoinSession(string joinCode)
    {
        var results = await _participantService.GetResultsAsync(joinCode);

        if (results == null)
            throw new HubException("No session found with that code.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(joinCode));

        // returned straight to the caller, so late joiners get the current state immediately
        return results;
    }

    public Task LeaveSession(string joinCode) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupFor(joinCode));
}