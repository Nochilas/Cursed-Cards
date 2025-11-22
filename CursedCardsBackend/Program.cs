using System.Text.Json;
using CursedCardsBackend.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serve static files
app.UseDefaultFiles(); // index.html
app.UseStaticFiles(); // wwwroot

// Constants
const string CURRENT_GAME_STATE_PATH = "currentGameState.json";
const string BAD_CARDS_PATH = "badCards.json";
var SERIALIZER_OPTIONS = new JsonSerializerOptions { WriteIndented = true };

/// <summary>
/// Loads the current game state.
/// </summary>
GameState ReadGameState()
{
    if (!File.Exists(CURRENT_GAME_STATE_PATH))
    {
        File.WriteAllText(CURRENT_GAME_STATE_PATH, "{}");
    }

    var text = File.ReadAllText(CURRENT_GAME_STATE_PATH);
    return JsonSerializer.Deserialize<GameState>(text) ?? new();
}

/// <summary>
/// Updates the game state.
/// </summary>
void WriteGameState(GameState gameState)
{
    var json = JsonSerializer.Serialize(gameState, SERIALIZER_OPTIONS);
    File.WriteAllText(CURRENT_GAME_STATE_PATH, json);
}

/// <summary>
/// Initializes a game.
/// </summary>
app.MapPost("/create-game", () =>
{
    var gameState = ReadGameState();
    var roomId = Guid.NewGuid().ToString("N")[..6].ToUpper();

    // Read all cards from json
    var text = File.ReadAllText(BAD_CARDS_PATH);
    var allCards = JsonSerializer.Deserialize<AllCards>(text);

    var game = new GameState
    {
        RoomId = roomId,
        WhiteDeck = allCards?.WhiteCards ?? [],
        BlackDeck = allCards?.BlackCards ?? [],
    };

    WriteGameState(gameState);

    return Results.Ok(new { roomId });
});

/// <summary>
/// Allows a player to join a game.
/// </summary>
app.MapPost("/join-game/{roomId}/{playerName}", (string roomId, string playerName) =>
{
    var gameState = ReadGameState();

    // Check if roomId is correct
    if (!CheckRoomId(roomId, gameState))
    {
        return Results.NotFound("Room not found");
    }

    // Checks if player is already in
    if (gameState.Players.Contains(playerName))
    {
        return Results.BadRequest("Username already taken");
    }

    // Adds a player, if not already in
    gameState.Players.Add(playerName);

    // Draws a starting hand for the player
    if (!gameState.Hands.ContainsKey(playerName))
    {
        gameState.Hands[playerName] = [.. gameState.WhiteDeck.Take(10)];
        gameState.WhiteDeck.RemoveRange(0, 10);
    }

    // Update
    WriteGameState(gameState);
    return Results.Ok(gameState);
});

/// <summary>
/// Allows a player to draw a number of white cards.
/// </summary>
app.MapPost("/draw-white/{roomId}/{player}/{quantity}", (string roomId, string player, int quantity) =>
{
    var gameState = ReadGameState();

    if (!CheckRoomId(roomId, gameState))
    {
        return Results.NotFound("Room not found");
    }

    if (gameState.WhiteDeck.Count == 0)
    {
        return Results.BadRequest("No more white cards");
    }

    // Draw cards
    var cards = gameState.WhiteDeck.Take(quantity).ToList();
    gameState.WhiteDeck.RemoveRange(0, quantity);

    // Safety check: if a player has no cards, initialize his hand
    if (!gameState.Hands.TryGetValue(player, out List<string>? value))
    {
        value = [];
        gameState.Hands[player] = value;
    }

    value.AddRange(cards);

    // Update
    WriteGameState(gameState);
    return Results.Ok(new { cards });
});

/// <summary>
/// Allows a player to draw a black card.
/// </summary>
app.MapPost("/draw-black/{roomId}", (string roomId) =>
{
    var gameState = ReadGameState();
    if (!CheckRoomId(roomId, gameState))
    {
        return Results.NotFound("Room non trovata");
    }

    if (gameState.BlackDeck.Count == 0)
    {
        return Results.BadRequest("No more black cards");
    }

    // Draw
    gameState.CurrentBlackCard = gameState.BlackDeck[0];
    gameState.BlackDeck.RemoveAt(0);

    // Update
    WriteGameState(gameState);
    return Results.Ok(new { gameState.CurrentBlackCard });
});

app.Run();

/// <summary>
/// Checks the gameState correct roomId.
/// </summary>
static bool CheckRoomId(string roomId, GameState gameState) => gameState.RoomId.Equals(roomId);
