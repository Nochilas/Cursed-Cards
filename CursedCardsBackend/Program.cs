using System.Text.Json;
using CursedCardsBackend.Enums;
using CursedCardsBackend.Managers;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    })
    .AddSingleton<GameManager>()
    .AddSingleton<GameService>();

var app = builder.Build();

// Serve static files
app.UseDefaultFiles(); // index.html
app.UseStaticFiles(); // wwwroot

app.MapPost("/create-game", (GameService gameService) =>
{
    var roomId = gameService.InitializeGame();
    return Results.Ok(new ApiSuccessResponse<string>(roomId));
});

/// <summary>
/// Loads the game state.
/// </summary>
app.MapGet("/game-state/{roomId}", (
    string roomId,
    GameService gameService) =>
{
    var gameState = gameService.ReadGameState();
    if (!gameService.CheckRoomId(roomId, gameState.RoomId))
    {
        return Results.NotFound(new ApiErrorResponse("Room not found"));
    }

    return Results.Ok(new ApiSuccessResponse<GameState>(gameState));
});

/// <summary>
/// Allows a player to join a game.
/// </summary>
app.MapPost("/join-game/{roomId}/{playerName}", (
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
    gameService.JoinRoom(playerName, gameState);
    return Results.Ok(new ApiSuccessResponse<string>(playerName));
});

/// <summary>
/// Allows a player to draw a number of white cards.
/// </summary>
app.MapPost("/draw-white/{roomId}/{player}/{quantity}", (
    string roomId,
    string player,
    int quantity,
    GameService gameService) =>
{
    var gameState = gameService.ReadGameState();

    // Check if room is correct
    if (!gameService.CheckRoomId(roomId, gameState.RoomId))
    {
        return Results.NotFound(new ApiErrorResponse("Room not found"));
    }

    // Check remaining white cards
    if (gameService.NoCardsLeft(gameState.WhiteDeck.Count, quantity))
    {
        return Results.BadRequest(new ApiErrorResponse("No more white cards"));
    }

    // Draw
    var drawnCards = gameService
        .DrawCards(
            quantity,
            gameState.WhiteDeck);
    gameState.Hands[player].AddRange(drawnCards);

    // Update
    gameService.WriteGameState(gameState);
    return Results.Ok(new ApiSuccessResponse<List<string>>(gameState.Hands[player]));
});

/// <summary>
/// Allows a player to draw a black card.
/// </summary>
app.MapPost("/draw-black/{roomId}", (
    string roomId,
    GameService gameService) =>
{
    var gameState = gameService.ReadGameState();

    // Check if room is correct
    if (!gameService.CheckRoomId(roomId, gameState.RoomId))
    {
        return Results.NotFound(new ApiErrorResponse("Room not found"));
    }

    // Check remaining black cards
    if (gameService.NoCardsLeft(gameState.BlackDeck.Count, 1))
    {
        return Results.BadRequest(new ApiErrorResponse("No more black cards"));
    }

    // Draw
    gameState.CurrentBlackCard = gameService
        .DrawCards(
            quantity: 1,
            [.. gameState.BlackDeck])[0];

    // Update
    gameService.WriteGameState(gameState);
    return Results.Ok(new ApiSuccessResponse<string>(gameState.CurrentBlackCard));
});

/// <summary>
/// Opens the room lobby.
/// </summary>
app.MapGet("/lobby-state/{roomId}", (
    string roomId,
    GameService gameService) =>
{
    var gameState = gameService.ReadGameState();
    if (!gameService.CheckRoomId(roomId, gameState.RoomId))
        return Results.NotFound(new ApiErrorResponse("Room not found"));

    var lobbyDto = new LobbyDTO(
        gameState.Players,
        gameState.Czar,
        gameState.GameStarted);

    return Results.Ok(new ApiSuccessResponse<LobbyDTO>(lobbyDto));
});

/// <summary>
/// Starts the game.
/// Only the czar can call this endpoint.
/// </summary>
app.MapPost("/start-game/{roomId}/{player}", (
    string roomId,
    string player,
    GameService gameService) =>
{
    var gameState = gameService.ReadGameState();
    if (!gameService.CheckRoomId(roomId, gameState.RoomId))
    {
        return Results.NotFound(new ApiErrorResponse("Room not found"));
    }

    if (!string.Equals(gameState.Czar, player))
    {
        return Results.BadRequest(new ApiErrorResponse("You are not the Czar"));
    }

    gameState.GameStarted = true;
    gameService.WriteGameState(gameState);

    return Results.Ok(new ApiSuccessResponse<string>("Game started"));
});

/// <summary>
/// Starts a round of a game.
/// Only the czar can call this endpoint.
/// </summary>
app.MapPost("/start-round/{roomId}/{player}", (
    string roomId,
    string player,
    GameService gameService
) =>
{
    var gameState = gameService.ReadGameState();
    if (!gameService.CheckRoomId(roomId, gameState.RoomId))
    {
        return Results.NotFound(new ApiErrorResponse("Room not found"));
    }

    if (!string.Equals(gameState.Czar, player))
    {
        return Results.BadRequest(new ApiErrorResponse("You are not the Czar"));
    }

    if (string.IsNullOrWhiteSpace(gameState.CurrentBlackCard))
    {
        return Results.BadRequest(new ApiErrorResponse("No black card drawn"));
    }

    // Reset previous round data
    gameState.PlayedCards.Clear();
    gameState.RoundStatus = RoundStatus.InProgress;

    gameService.WriteGameState(gameState);

    return Results.Ok(new ApiSuccessResponse<string>("Round started"));
});

app.MapPost("/play-cards/{roomId}/{playerName}", (
    string roomId,
    string playerName,
    SelectedCardsRequestDTO selectedCards,
    GameService gameService) =>
{
    var gameState = gameService.ReadGameState();

    // Room check
    if (!gameService.CheckRoomId(roomId, gameState.RoomId))
    {
        return Results.NotFound(new ApiErrorResponse("Room not found"));
    }

    // Status check: the round must be in progress
    if (gameState.RoundStatus != RoundStatus.InProgress)
    {
        return Results.BadRequest(new ApiErrorResponse("No active round"));
    }

    // Player check: must be an existing player
    if (!gameState.Players.Contains(playerName))
    {
        return Results.NotFound(new ApiErrorResponse("Player not found"));
    }

    // Czar check: the czar can't play cards
    if (string.Equals(gameState.Czar, playerName))
    {
        return Results.NotFound(new ApiErrorResponse("Czar cannot play cards"));
    }

    // Check if player has played cards already
    if (gameState.PlayedCards.ContainsKey(playerName))
    {
        return Results.BadRequest(new ApiErrorResponse("Player already played this round"));
    }

    // Check if black card is selected
    if (string.IsNullOrEmpty(gameState.CurrentBlackCard))
    {
        return Results.BadRequest(new { errorMessage = "No black card selected" });
    }

    // Save
    gameState.PlayedCards[playerName] = selectedCards.SelectedCards;

    // Update
    gameService.WriteGameState(gameState);
    return Results.Ok(new ApiSuccessResponse<string>("Cards played successfully"));
});

// Start the app
app.Run();