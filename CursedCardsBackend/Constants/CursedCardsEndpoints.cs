namespace CursedCardsBackend.Constants;

public class CursedCardsEndpoints
{
    /// <summary>
    /// API endpoints.
    /// </summary>
    public const string CREATE_GAME = "/create-game";
    public const string DRAW_BLACK_CARD = "/draw-black/{roomId}";
    public const string DRAW_WHITE_CARD = "/draw-white/{roomId}/{player}/{quantity}";
    public const string GET_GAME_STATE = "/game-state/{roomId}";
    public const string GET_LOBBY_STATE = "/lobby-state/{roomId}";
    public const string JOIN_GAME = "/join-game/{roomId}/{playerName}";
    public const string PLAY_CARDS = "/play-cards/{roomId}/{playerName}";
    public const string SELECT_WINNER = "/select-winner";
    public const string START_GAME = "/start-game/{roomId}/{player}";
    public const string START_ROUND = "/start-round/{roomId}/{player}";

    /// <summary>
    /// Hub endpoint.
    /// </summary>
    public const string GAME_HUB_ENDPOINT = "/gamehub";
}
