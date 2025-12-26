using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using Microsoft.AspNetCore.SignalR;

namespace CursedCardsBackend.Services;

public class GameNotifier(IHubContext<GameHub> hub)
{
    /// <summary>
    /// Lobby notification event.
    /// </summary>
    public async Task LobbyUpdatedAsync(GameState state)
        => await hub.Clients
            .Group(state.RoomId)
            .SendAsync(
                method: Events.UPDATED_LOBBY_EVENT,
                new LobbyDTO(state.Players, state.Czar, state.GameStarted));

    /// <summary>
    /// Game state changed notification event.
    /// </summary>
    public async Task GameUpdatedAsync(GameState state)
        => await hub.Clients
            .Group(state.RoomId)
            .SendAsync(Events.UPDATED_GAME_EVENT, state);

    /// <summary>
    /// Notifies the chosen winner.
    /// </summary>
    public async Task WinnerChosenAsync(string roomId, string winnerPlayer)
        => await hub.Clients
            .Group(roomId)
            .SendAsync(Events.WINNER_CHOSEN, winnerPlayer);
}