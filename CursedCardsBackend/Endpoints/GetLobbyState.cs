using CursedCardsBackend.Constants;
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
            app.MapGet(
                CursedCardsEndpoints.GET_LOBBY_STATE,
                (string roomId, GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<LobbyDTO>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                return new ApiResponse<LobbyDTO>(
                    Response: new(
                        gameState.Players,
                        gameState.Czar,
                        gameState.GameStarted));
            });
        }
    }
}
