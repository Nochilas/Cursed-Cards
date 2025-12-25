using CursedCardsBackend.Constants;
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
            app.MapPost(
                CursedCardsEndpoints.DRAW_BLACK_CARD,
                async (string roomId, GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Check if room is correct
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                // Check remaining black cards
                if (gameService.NoCardsLeft(gameState.BlackDeck.Count, 1))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.NO_MORE_BLACK_CARDS);
                }

                // Draw
                gameState.CurrentBlackCard = gameService
                    .DrawCards(
                        quantity: 1,
                        [.. gameState.BlackDeck])[0];

                // Update
                await gameService.WriteGameStateAsync(gameState);
                return new ApiResponse<string>(Response: gameState.CurrentBlackCard);
            });
        }
    }
}
