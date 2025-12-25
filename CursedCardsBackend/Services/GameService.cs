using System.Text.Json;
using CursedCardsBackend.Enums;
using CursedCardsBackend.Managers;
using CursedCardsBackend.Models;

namespace CursedCardsBackend.Services;

public class GameService(
    GameManager gameManager,
    GameNotifier gameNotifier,
    JsonSerializerOptions serializerOptions)
{
    /// <summary>
    /// Four players for now.
    /// </summary>
    private readonly int _minPlayers = 4;
    private readonly string _cursedCardsPath = "cursedCards.json";

    /// <summary>
    /// Initializes a game.
    /// </summary>
    public string InitializeGame()
    {
        var roomId = Guid.NewGuid().ToString("N")[..6].ToUpper();

        // Read all cards from json
        var text = File.ReadAllText(_cursedCardsPath);
        var allCards = JsonSerializer.Deserialize<AllCards>(text, serializerOptions);

        var initialGameState = new GameState
        {
            RoomId = roomId,
            WhiteDeck = allCards?.WhiteCards ?? [],
            BlackDeck = allCards?.BlackCards ?? [],
        };

        gameManager.Write(initialGameState);
        return roomId;
    }

    /// <summary>
    /// Adds a playr to a specific room.
    /// </summary>
    public async Task JoinRoomAsync(string playerName, GameState gameState)
    {
        // Adds a player
        gameState.Players.Add(playerName);

        // Draws a starting hand for the player
        if (!gameState.Hands.ContainsKey(playerName))
        {
            var drawnCards = DrawCards(
                quantity: 10,
                gameState.WhiteDeck);

            gameState.Hands[playerName] = drawnCards;
        }

        // Initialize score at zero for this player
        gameState.Scores.TryAdd(playerName, 0);

        // If czar is not set, choose a random player
        if (string.IsNullOrEmpty(gameState.Czar)
            && gameState.Players.Count >= _minPlayers)
        {
            var random = new Random();
            int index = random.Next(gameState.Players.Count);
            gameState.Czar = gameState.Players[index];
        }

        // Update
        await WriteGameStateAsync(gameState);

        // Notify
        await gameNotifier.LobbyUpdatedAsync(gameState);
    }

    /// <summary>
    /// Starts the game.
    /// </summary>
    public async Task StartGameAsync(GameState gameState)
    {
        gameState.GameStarted = true;
        await WriteGameStateAsync(gameState);

        // Notify
        await gameNotifier.LobbyUpdatedAsync(gameState);
    }

    /// <summary>
    /// Draws a specific amount of cards for a specific player.
    /// </summary>
    public List<string> DrawCards(int quantity, List<string> deck)
    {
        // Draw X random cards
        var random = new Random();
        var drawnCards = new List<string>();

        for (int i = 0; i < quantity; i++)
        {
            int index = random.Next(deck.Count);

            // Adds a card to the player hand and removes it from the deck
            drawnCards.Add(deck[index]);
            deck.RemoveAt(index);
        }

        return drawnCards;
    }

    /// <summary>
    /// Selects the player that won the round.
    /// </summary>
    public async Task SelectWinnerAsync(string roomId, string winnerPlayer, GameState gameState)
    {
        // Check if the player has a score
        if (!gameState.Scores.TryGetValue(winnerPlayer, out int value))
        {
            // If he has not, init score at 0
            value = 0;
            gameState.Scores[winnerPlayer] = value;
        }

        // Update score and game status
        gameState.Scores[winnerPlayer] = value += 1;
        gameState.Czar = winnerPlayer;

        // Reset round status to allow czar starting next round
        gameState.RoundStatus = RoundStatus.Waiting;

        // Reset
        gameState.PlayedCards = [];
        gameState.CurrentBlackCard = null;

        // Update
        await WriteGameStateAsync(gameState);
    }

    /// <summary>
    /// Check if all the players except the czar have played their cards.s
    /// </summary>
    public void IsRoundComplete(GameState gameState)
    {
        // Count how many players must play (all except the czar)
        var playersThatMustPlay = gameState.Players
            .Count(players => players != gameState.Czar);

        // Count how many have already played
        var playersThatPlayed = gameState.PlayedCards.Count;

        // If all non-czar players have played, the round is over
        // And the status changes, now the czar must pick the winner
        if (playersThatPlayed == playersThatMustPlay)
        {
            gameState.RoundStatus = RoundStatus.CzarPicking;
        }
    }

    /// <summary>
    /// Reads the current game state.
    /// </summary>
    public GameState ReadGameState()
        => gameManager.Read();

    /// <summary>
    /// Updates the current game state.
    /// </summary>
    public async Task WriteGameStateAsync(GameState gameState)
    {
        gameManager.Write(gameState);
        await gameNotifier.GameUpdatedAsync(gameState);
    }

    /// <summary>
    /// Checks if a given roomId equals the roomId of the current game.
    /// </summary>
    public bool CheckRoomId(string roomId, string currentRoomId)
        => currentRoomId.Equals(roomId);

    /// <summary>
    /// Checks if a username is taken.
    /// </summary>
    public bool IsUsernameTaken(string playerName, List<string> players)
        => players.Contains(playerName);

    /// <summary>
    /// Checks if there's no cards left.
    /// </summary>
    public bool NoCardsLeft(int cardsCount, int quantity)
        => cardsCount == 0
        || cardsCount < quantity;
}