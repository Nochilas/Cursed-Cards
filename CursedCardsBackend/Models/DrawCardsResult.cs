namespace CursedCardsBackend.Services.Models;

public record class DrawCardsResult(
    List<string> PlayerHand,
    List<string> UpdatedDeck);
