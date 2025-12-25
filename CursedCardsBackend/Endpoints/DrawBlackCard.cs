using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class DrawBlackCard
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Allows a player to draw a black card.
        /// </summary>
        public void AddDrawBlackCardEndpoint()
        {
            app.MapPost("/draw-black/{roomId}", async (
                string roomId,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Check if room is correct
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return Results.NotFound(new ApiErrorResponse("Room not found"));
                }

                // Check remaining black cards
                if (gameService.NoCardsLeft(gameState.BlackDeck.Count, 1))
                {
                    return Results.BadRequest(new ApiErrorResponse("No more black cards"));
                }

                // Draw
                gameState.CurrentBlackCard = gameService
                    .DrawCards(
                        quantity: 1,
                        [.. gameState.BlackDeck])[0];

                // Update
                await gameService.WriteGameStateAsync(gameState);
                return Results.Ok(new ApiSuccessResponse<string>(gameState.CurrentBlackCard));
            });
        }
    }
}
