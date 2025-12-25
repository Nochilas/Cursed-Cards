using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using Microsoft.AspNetCore.SignalR;

namespace CursedCardsBackend.Services;

public class GameNotifier(IHubContext<GameHub> hub)
{
    /// <summary>
    /// Lobby notification event.
    /// </summary>
    public Task NotifyLobby(GameState state)
        => hub.Clients
            .Group(state.RoomId)
            .SendAsync(
                method: CursedCardsConstants.LOBBY_NOTIFICATION_EVENT,
                new LobbyDTO(state.Players, state.Czar, state.GameStarted));
}