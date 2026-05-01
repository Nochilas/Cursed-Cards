using System.Text.Json;
using CursedCardsBackend.Models;

namespace CursedCardsBackend.Managers;

public class GameManager(JsonSerializerOptions serializerOptions)
{
    private readonly string _currenGameStatePath = "currentGameState.json";
    private readonly string _empty = "{}";

    /// <summary>
    /// Reads the current game state.
    /// </summary>
    public GameState Read()
    {
        if (!File.Exists(_currenGameStatePath))
        {
            File.WriteAllText(_currenGameStatePath, _empty);
        }

        var text = File.ReadAllText(_currenGameStatePath);
        return JsonSerializer.Deserialize<GameState>(text) ?? new();
    }

    /// <summary>
    /// Updates the current game state.
    /// </summary>
    public void Write(GameState gameState)
    {
        var json = JsonSerializer.Serialize(gameState, serializerOptions);
        File.WriteAllText(_currenGameStatePath, json);
    }
}