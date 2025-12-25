using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;
using Microsoft.AspNetCore.SignalR;

public class GameHub(GameService gameService) : Hub
{
public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        var gameState = gameService.ReadGameState();
        if (gameState.RoomId != roomId)
        {
            return;
        }

        // Immediately send status to joined client
        await Clients.Caller
            .SendAsync(
                method: CursedCardsConstants.UPDATED_LOBBY_EVENT,
                new LobbyDTO(gameState.Players, gameState.Czar, gameState.GameStarted));
    }

    public async Task LeaveRoom(string roomId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
}
