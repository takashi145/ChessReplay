namespace ChessReplay.Models;

public enum GameResult
{
    Win,
    Loss,
    Draw,
}

public static class GameOutcome
{
    private static readonly HashSet<string> DrawRawResults = new(StringComparer.OrdinalIgnoreCase)
    {
        "agreed", "repetition", "stalemate", "insufficient", "50move", "timevsinsufficient",
    };

    // Chess.com marks the winner's own result as "win"; the terminal reason
    // (checkmate, timeout, ...) is only present on the losing/drawing side.
    public static (GameResult Result, string Reason) Describe(Player self, Player opponent)
    {
        var isWin = self.RawResult.Equals("win", StringComparison.OrdinalIgnoreCase);
        var result = isWin ? GameResult.Win
            : DrawRawResults.Contains(self.RawResult) ? GameResult.Draw
            : GameResult.Loss;

        var reasonSource = isWin ? opponent.RawResult : self.RawResult;
        return (result, DescribeReason(reasonSource));
    }

    private static string DescribeReason(string rawResult) => rawResult.ToLowerInvariant() switch
    {
        "checkmated" => "Checkmate",
        "timeout" => "Timeout",
        "resigned" => "Resignation",
        "repetition" => "Repetition",
        "stalemate" => "Stalemate",
        "insufficient" => "Insufficient Material",
        "50move" => "50-move Rule",
        "agreed" => "Agreement",
        "abandoned" => "Abandoned",
        "timevsinsufficient" => "Timeout vs Insufficient Material",
        "win" => "Win",
        _ => rawResult,
    };
}
