using CursedCardsBackend.Constants;
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
            app.MapPost(
                CursedCardsEndpoints.DRAW_WHITE_CARD,
                async (
                    string roomId,
                    string player,
                    int quantity,
                    GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Check if room is correct
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<List<string>>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                // Check remaining white cards
                if (gameService.NoCardsLeft(gameState.WhiteDeck.Count, quantity))
                {
                    return new ApiResponse<List<string>>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.NO_MORE_WHITE_CARDS);
                }

                // Draw
                var drawnCards = gameService
                    .DrawCards(
                        quantity,
                        gameState.WhiteDeck);
                gameState.Hands[player].AddRange(drawnCards);

                // Update
                await gameService.WriteGameStateAsync(gameState);
                return new ApiResponse<List<string>>(Response: gameState.Hands[player]);
            });
        }
    }
}
