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
        public async Task AddJoinGameEndpointAsync()
        {
            app.MapPost("/join-game/{roomId}/{playerName}", async (
                string roomId,
                string playerName,
                GameService gameService) =>
            {
                var gameState = gameService.ReadGameState();

                // Check if room is correct
                if (!gameService.CheckRoomId(roomId, gameState.RoomId))
                {
                    return Results.NotFound(new ApiErrorResponse("Room not found"));
                }

                // Check if the player is already in, or if someone already uses this username
                if (gameService.IsUsernameTaken(playerName, gameState.Players))
                {
                    return Results.BadRequest(new ApiErrorResponse("Username already taken"));
                }

                // Join
                await gameService.JoinRoomAsync(playerName, gameState);
                return Results.Ok(new ApiSuccessResponse<string>(playerName));
            });
        }
    }
}
