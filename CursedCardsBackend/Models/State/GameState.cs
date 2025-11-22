using CursedCardsBackend.Enums;

namespace CursedCardsBackend.Models;

public class GameState
{
    /// <summary>
    /// This property is only needed to join a game.
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// Current players in this game.
    /// </summary>
    public List<string> Players { get; set; } = [];

    /// <summary>
    /// All the white cards of the game.
    /// </summary>
    public List<string> WhiteDeck { get; set; } = [];

    /// <summary>
    /// All the black cards of the game.
    /// </summary>
    public List<string> BlackDeck { get; set; } = [];

    /// <summary>
    /// All the player hands of the game.
    /// </summary>
    public Dictionary<string, List<string>> Hands { get; set; } = [];

    /// <summary>
    /// Indicates the current black card.
    /// </summary>
    public string? CurrentBlackCard { get; set; }

    /// <summary>
    /// The player that's currently the game czar.
    /// </summary>
    public string? Czar { get; set; }

    /// <summary>
    /// Indicates if the game has started.
    /// </summary>
    public bool GameStarted { get; set; }

    /// <summary>
    /// Indicates the status of the current round.
    /// </summary>
    public RoundStatus RoundStatus { get; set; }

    /// <summary>
    /// Cards played in this round.
    /// </summary>
    public Dictionary<string, List<string>> PlayedCards { get; set; } = [];

    /// <summary>
    /// Score of the game.
    /// </summary>
    public Dictionary<string, int> Scores { get; set; } = [];
}
