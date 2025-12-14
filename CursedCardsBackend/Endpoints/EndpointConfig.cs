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
            app.AddCreateGameEndpoint();        // Player creates a new game
            app.AddJoinGameEndpoint();          // Player joins an existing game
            app.AddGetLobbyStateEndpoint();     // Returns lobby state for UI refresh
            app.AddStartGameEndpoint();         // Czar starts a new game
            app.AddGetGameStateEndpoint();      // Returns game state for UI refresh
            app.AddDrawWhiteCardsEndpoint();    // Player draws a number of white cards
            app.AddDrawBlackCardEndpoint();     // Player draws a black card
            app.AddStartRoundEndpoint();        // Czar starts a new round
            app.AddPlayCardsEndpoint();         // Player plays a card
            app.AddSelectWinnerEndpoint();      // Czar selects a winner for the current round
        }
    } 
}
