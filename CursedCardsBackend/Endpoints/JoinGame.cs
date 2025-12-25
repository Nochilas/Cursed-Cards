using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class JoinGame
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Allows a player to join a game.
        /// </summary>
        public void AddJoinGameEndpoint()
        {
            app.MapPost(
                CursedCardsEndpoints.JOIN_GAME,
                async (
                    string roomId,
                    string playerName,
                    GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Check if room is correct
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.ROOM_NOT_FOUND);
                }

                // Check if the player is already in, or if someone already uses this username
                if (gameService.IsUsernameTaken(playerName, gameState.Players))
                {
                    return new ApiResponse<string>(
                        HasError: true,
                        ErrorMessage: ErrorMessages.USERNAME_ALREADY_TAKEN);
                }

                // Join
                await gameService.JoinRoomAsync(playerName, gameState);
                return new ApiResponse<string>(Response: playerName);
            });
        }
    }
}
