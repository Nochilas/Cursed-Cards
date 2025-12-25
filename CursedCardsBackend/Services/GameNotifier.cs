using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using Microsoft.AspNetCore.SignalR;

namespace CursedCardsBackend.Services;

public class GameNotifier(IHubContext<GameHub> hub)
{
    /// <summary>
    /// Lobby notification event.
    /// </summary>
    public Task LobbyUpdatedAsync(GameState state)
        => hub.Clients
            .Group(state.RoomId)
            .SendAsync(
                method: CursedCardsConstants.UPDATED_LOBBY_EVENT,
                new LobbyDTO(state.Players, state.Czar, state.GameStarted));
    
    // TODO
    // public Task GameStateUpdated(GameState state)
    //     => hub.Clients
    //         .Group(state.RoomId)
    //         .SendAsync("GameStateUpdated", stateDTO);
}