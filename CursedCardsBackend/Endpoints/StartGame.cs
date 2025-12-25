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
            app.MapPost("/start-game/{roomId}/{player}", async (
                string roomId,
                string player,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return Results.NotFound(new ApiErrorResponse("Room not found"));
                }

                if (!string.Equals(gameState.Czar, player))
                {
                    return Results.BadRequest(new ApiErrorResponse("You are not the Czar"));
                }

                await gameService.StartGameAsync(gameState);

                return Results.Ok(new ApiSuccessResponse<string>("Game started"));
            });
        }
    }
}
