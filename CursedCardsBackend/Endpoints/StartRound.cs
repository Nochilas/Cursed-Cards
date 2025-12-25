using CursedCardsBackend.Constants;
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
            app.MapPost(
                CursedCardsEndpoints.START_ROUND,
                async (
                    string roomId,
                    string player,
                    GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                if (!string.Equals(gameState.Czar, player))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.YOU_ARE_NOT_CZAR);
                }

                if (string.IsNullOrWhiteSpace(gameState.CurrentBlackCard))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.NO_BLACK_CARD_SELECTED);
                }

                if (gameState.RoundStatus == RoundStatus.InProgress)
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROUND_IN_PROGRESS);
                }

                // Reset previous round data
                gameState.PlayedCards.Clear();
                gameState.RoundStatus = RoundStatus.InProgress;

                await gameService.WriteGameStateAsync(gameState);

                return new ApiResponse<string>(Response: SuccessMessages.ROUND_STARTED);
            });
        }
    }
}
