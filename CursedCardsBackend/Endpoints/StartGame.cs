using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class StartGame
{
    extension(WebApplication app)
    {
        public void AddStartGameEndpoint()
        {
            /// <summary>
            /// Starts the game.
            /// Only the czar can call this endpoint.
            /// </summary>
            app.MapPost(
                CursedCardsEndpoints.START_GAME,
                async (
                    string roomId,
                    string player,
                    GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                if (!string.Equals(gameState.Czar, player))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.YOU_ARE_NOT_CZAR);
                }

                await gameService.StartGameAsync(gameState);

                return new ApiResponse<string>(Response: SuccessMessages.GAME_STARTED);
            });
        }
    }
}
