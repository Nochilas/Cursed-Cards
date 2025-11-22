namespace CursedCardsBackend.Models;

public record class LobbyDTO(
    List<string> Players,
    string? Czar,
    bool GameStarted);