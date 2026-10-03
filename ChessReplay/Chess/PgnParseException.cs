namespace ChessReplay.Chess;

public sealed class PgnParseException(
    string message = "This game could not be replayed because its PGN could not be parsed.")
    : Exception(message);
