using CursedCardsBackend.Enums;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class StartRound
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Starts a round of a game.
        /// Only the czar can call this endpoint.
        /// </summary>
        public void AddStartRoundEndpoint()
        {
            app.MapPost("/start-round/{roomId}/{player}", async (
                string roomId,
                string player,
                GameService gameService
            ) =>
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

                if (string.IsNullOrWhiteSpace(gameState.CurrentBlackCard))
                {
                    return Results.BadRequest(new ApiErrorResponse("No black card drawn"));
                }

                if (gameState.RoundStatus == RoundStatus.InProgress)
                {
                    return Results.BadRequest(new ApiErrorResponse("Round already in progress"));
                }

                // Reset previous round data
                gameState.PlayedCards.Clear();
                gameState.RoundStatus = RoundStatus.InProgress;

                await gameService.WriteGameStateAsync(gameState);

                return Results.Ok(new ApiSuccessResponse<string>("Round started"));
            });
        }
    }
}
