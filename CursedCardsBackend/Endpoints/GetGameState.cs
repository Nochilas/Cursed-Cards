using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class GetGameState
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Loads the game state.
        /// </summary>
        public void AddGetGameStateEndpoint()
        {
            app.MapGet("/game-state/{roomId}", (
                string roomId,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return Results.NotFound(new ApiErrorResponse("Room not found"));
                }

                return Results.Ok(new ApiSuccessResponse<GameState>(gameState));
            });
        }
    }
}
