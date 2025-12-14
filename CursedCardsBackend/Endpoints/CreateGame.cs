using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

namespace CursedCardsBackend.Endpoints;

public static class CreateGame
{
    extension(WebApplication app)
    {
        public void AddCreateGameEndpoint()
        {
            app.MapPost("/create-game", (GameService gameService) =>
            {
                var roomId = gameService.InitializeGame();
                return Results.Ok(new ApiSuccessResponse<string>(roomId));
            });
        }
    }
}
