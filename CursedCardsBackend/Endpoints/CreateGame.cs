using CursedCardsBackend.Constants;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class CreateGame
{
    extension(WebApplication app)
    {
        public void AddCreateGameEndpoint()
        {
            app.MapPost(
                CursedCardsEndpoints.CREATE_GAME,
                (GameService gameService) =>
            {
                var roomId = gameService.InitializeGame();
                return new ApiResponse<string>(Response: roomId);
            });
        }
    }
}
