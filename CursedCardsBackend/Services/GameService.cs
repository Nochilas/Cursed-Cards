using System.Text.Json;
using CursedCardsBackend.Managers;
using CursedCardsBackend.Models;
using CursedCardsBackend.Services.Models;

namespace CursedCardsBackend.Services;

public class GameService(GameManager gameManager, JsonSerializerOptions serializerOptions)
{
    private readonly string _badCardsPath = "badCards.json";

    /// <summary>
    /// Initializes a game.
    /// </summary>
    public string InitializeGame()
    {
        var roomId = Guid.NewGuid().ToString("N")[..6].ToUpper();

        // Read all cards from json
        var text = File.ReadAllText(_badCardsPath);
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
    public void JoinRoom(string playerName, GameState gameState)
    {
        // Adds a player
        gameState.Players.Add(playerName);

        // Draws a starting hand for the player
        if (!gameState.Hands.ContainsKey(playerName))
        {
            var drawCardsResult = DrawCards(
                quantity: 10,
                gameState.WhiteDeck);

            gameState.Hands[playerName].AddRange(drawCardsResult.DrawnCards);
            gameState.WhiteDeck = drawCardsResult.UpdatedDeck;
        }

        // Update
        gameManager.Write(gameState);
    }

    /// <summary>
    /// Draws a specific amount of cards for a specific player.
    /// </summary>
    public DrawCardsResult DrawCards(
        int quantity,
        List<string> deck)
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

        return new(drawnCards, deck);
    }

    /// <summary>
    /// Reads the current game state.
    /// </summary>
    public GameState ReadGameState()
        => gameManager.Read();

    /// <summary>
    /// Updates the current game state.
    /// </summary>
    public void WriteGameState(GameState gameState)
        => gameManager.Write(gameState);

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