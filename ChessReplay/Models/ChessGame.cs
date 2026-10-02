namespace ChessReplay.Models;

public sealed record ChessGame(
    string Pgn,
    DateTimeOffset EndTime,
    string TimeControl,
    string TimeClass,
    bool Rated,
    Player White,
    Player Black,
    string Url,
    string Rules = "chess")
{
    public bool IsChess960 => string.Equals(Rules, "chess960", StringComparison.OrdinalIgnoreCase);

    public Player GetPlayer(string username) => IsWhite(username) ? White : Black;

    public Player GetOpponent(string username) => IsWhite(username) ? Black : White;

    private bool IsWhite(string username) =>
        string.Equals(White.Username, username, StringComparison.OrdinalIgnoreCase);
}
