using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class SelectWinner
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Selects the winner of a round.
        /// Only the czar can call this endpoint.
        /// </summary>
        public void AddSelectWinnerEndpoint()
        {
            app.MapPost("/select-winner", async (
                string roomId,
                string czarPlayerName,
                string winnerPlayer,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Room check
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return Results.NotFound(new ApiErrorResponse("Room not found"));
                }

                // Czar check: the caller must be the czar
                if (!string.Equals(gameState.Czar, czarPlayerName))
                {
                    return Results.NotFound(new ApiErrorResponse("Only the czar can select the winner"));
                }

                // Player check: must be an existing player
                if (!gameState.Players.Contains(winnerPlayer))
                {
                    return Results.NotFound(new ApiErrorResponse("Player not found"));
                }

                await gameService.SelectWinnerAsync(roomId, winnerPlayer, gameState);
                return Results.Ok(new ApiSuccessResponse<string>("Winner selected"));
            });
        }
    }
}
