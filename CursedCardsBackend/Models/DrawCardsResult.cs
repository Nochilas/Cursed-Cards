namespace CursedCardsBackend.Services.Models;

public record class DrawCardsResult(
    List<string> DrawnCards,
    List<string> UpdatedDeck);
