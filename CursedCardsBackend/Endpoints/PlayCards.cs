using CursedCardsBackend.Constants;
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
            app.MapPost(
                CursedCardsEndpoints.PLAY_CARDS,
                async (
                    string roomId,
                    string playerName,
                    SelectedCardsRequestDTO selectedCards,
                    GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Room check
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                // Status check: the round must be in progress
                if (gameState.RoundStatus != RoundStatus.InProgress)
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.NO_ACTIVE_ROUND);
                }

                // Player check: must be an existing player
                if (!gameState.Players.Contains(playerName))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.PLAYER_NOT_FOUND);
                }

                // Czar check: the czar can't play cards
                if (string.Equals(gameState.Czar, playerName))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.CZAR_CANNOT_PLAY_CARDS);
                }

                // Check if player has played cards already
                if (gameState.PlayedCards.ContainsKey(playerName))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.PLAYER_ALREADY_PLAYED);
                }

                // Check if black card is selected
                if (string.IsNullOrEmpty(gameState.CurrentBlackCard))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.NO_BLACK_CARD_SELECTED);
                }

                // Update the played cards
                gameState.PlayedCards[playerName] = selectedCards.SelectedCards;

                // Remove played cards from player hand
                gameState.Hands[playerName] = [.. gameState.Hands[playerName]
                    .Where(card => !selectedCards.SelectedCards.Contains(card))];

                // Check if the round is complete (all players have played their cards)
                gameService.IsRoundComplete(gameState);

                // Update
                await gameService.WriteGameStateAsync(gameState);
                return new ApiResponse<string>(Response: SuccessMessages.CARDS_PLAYED_SUCCESSFULLY);
            });
        }
    }
}
