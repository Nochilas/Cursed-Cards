using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class GetGameState
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Loads the game state.
        /// </summary>
        public void AddGetGameStateEndpoint()
        {
            app.MapGet(
                CursedCardsEndpoints.GET_GAME_STATE,
                (string roomId, GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<GameState>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                return new ApiResponse<GameState>(Response: gameState);
            });
        }
    }
}
