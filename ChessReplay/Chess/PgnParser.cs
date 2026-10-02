using Chess;

namespace ChessReplay.Chess;

public static class PgnParser
{
    public static ChessBoard Parse(string pgn)
    {
        try
        {
            if (ChessBoard.TryLoadFromPgn(pgn, out var board))
                return board;
        }
        catch
        {
        }

        throw new PgnParseException();
    }
}
