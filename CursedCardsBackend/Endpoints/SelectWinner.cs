using CursedCardsBackend.Constants;
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
            app.MapPost(
                CursedCardsEndpoints.SELECT_WINNER,
                async (
                    string roomId,
                    string czarPlayerName,
                    string winnerPlayer,
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

                // Czar check: the caller must be the czar
                if (!string.Equals(gameState.Czar, czarPlayerName))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ONLY_CZAR_SELECTS_WINNER);
                }

                // Player check: must be an existing player
                if (!gameState.Players.Contains(winnerPlayer))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.PLAYER_NOT_FOUND);
                }

                await gameService.SelectWinnerAsync(winnerPlayer, gameState);
                return new ApiResponse<string>(Response: SuccessMessages.WINNER_SELECTED);
            });
        }
    }
}
