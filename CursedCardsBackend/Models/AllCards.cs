namespace CursedCardsBackend.Models;

public class AllCards
{
    /// <summary>
    /// All available white cards.
    /// </summary>
    public List<string> WhiteCards { get; set; } = [];

    /// <summary>
    /// All available black cards.
    /// </summary>
    public List<string> BlackCards { get; set; } = [];
}