using CursedCardsBackend.Enums;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class PlayCards
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Submits the played cards.
        /// Only players can call this endpoint.
        /// </summary>
        public void AddPlayCardsEndpoint()
        {
            app.MapPost("/play-cards/{roomId}/{playerName}", (
                string roomId,
                string playerName,
                SelectedCardsRequestDTO selectedCards,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Room check
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return Results.NotFound(new ApiErrorResponse("Room not found"));
                }

                // Status check: the round must be in progress
                if (gameState.RoundStatus != RoundStatus.InProgress)
                {
                    return Results.BadRequest(new ApiErrorResponse("No active round"));
                }

                // Player check: must be an existing player
                if (!gameState.Players.Contains(playerName))
                {
                    return Results.NotFound(new ApiErrorResponse("Player not found"));
                }

                // Czar check: the czar can't play cards
                if (string.Equals(gameState.Czar, playerName))
                {
                    return Results.NotFound(new ApiErrorResponse("Czar cannot play cards"));
                }

                // Check if player has played cards already
                if (gameState.PlayedCards.ContainsKey(playerName))
                {
                    return Results.BadRequest(new ApiErrorResponse("Player already played this round"));
                }

                // Check if black card is selected
                if (string.IsNullOrEmpty(gameState.CurrentBlackCard))
                {
                    return Results.BadRequest(new { errorMessage = "No black card selected" });
                }

                // Update the played cards
                gameState.PlayedCards[playerName] = selectedCards.SelectedCards;

                // Remove played cards from player hand
                gameState.Hands[playerName] = [.. gameState.Hands[playerName]
                    .Where(card => !selectedCards.SelectedCards.Contains(card))];

                // Check if the round is complete (all players have played their cards)
                gameService.IsRoundComplete(gameState);

                // Update
                gameService.WriteGameState(gameState);
                return Results.Ok(new ApiSuccessResponse<string>("Cards played successfully"));
            });
        }
    }
}
