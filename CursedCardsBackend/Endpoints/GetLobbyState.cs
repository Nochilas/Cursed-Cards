using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class GetLobbyState
{
    extension(WebApplication app)
    {
        public void AddGetLobbyStateEndpoint()
        {
            /// <summary>
            /// Opens the room lobby.
            /// </summary>
            app.MapGet("/lobby-state/{roomId}", (
                string roomId,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                    return Results.NotFound(new ApiErrorResponse("Room not found"));

                var lobbyDto = new LobbyDTO(
                    gameState.Players,
                    gameState.Czar,
                    gameState.GameStarted);

                return Results.Ok(new ApiSuccessResponse<LobbyDTO>(lobbyDto));
            });
        }
    }
}
