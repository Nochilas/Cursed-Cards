namespace CursedCardsBackend.Endpoints;

public static class EndpointConfig
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Registers all endpoints
        /// </summary>
        public void ConfigureEndpoints()
        {
            // Player creates a new game
            app.AddCreateGameEndpoint();

            // Player joins an existing game
            app.AddJoinGameEndpoint();

            // Returns lobby state for UI refresh
            app.AddGetLobbyStateEndpoint();

            // Czar starts a new game
            app.AddStartGameEndpoint();

            // Returns game state for UI refresh
            app.AddGetGameStateEndpoint();

            // Player draws a number of white cards
            app.AddDrawWhiteCardsEndpoint();

            // Player draws a black card
            app.AddDrawBlackCardEndpoint();

            // Czar starts a new round
            app.AddStartRoundEndpoint();

            // Player plays a card
            app.AddPlayCardsEndpoint();

            // Czar selects a winner for the current round
            app.AddSelectWinnerEndpoint();
        }
    } 
}
