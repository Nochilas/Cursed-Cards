using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class DrawWhiteCards
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Allows a player to draw a number of white cards.
        /// </summary>
        public void AddDrawWhiteCardsEndpoint()
        {
            app.MapPost("/draw-white/{roomId}/{player}/{quantity}", async (
                string roomId,
                string player,
                int quantity,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Check if room is correct
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return Results.NotFound(new ApiErrorResponse("Room not found"));
                }

                // Check remaining white cards
                if (gameService.NoCardsLeft(gameState.WhiteDeck.Count, quantity))
                {
                    return Results.BadRequest(new ApiErrorResponse("No more white cards"));
                }

                // Draw
                var drawnCards = gameService
                    .DrawCards(
                        quantity,
                        gameState.WhiteDeck);
                gameState.Hands[player].AddRange(drawnCards);

                // Update
                await gameService.WriteGameStateAsync(gameState);
                return Results.Ok(new ApiSuccessResponse<List<string>>(gameState.Hands[player]));
            });
        }
    }
}
