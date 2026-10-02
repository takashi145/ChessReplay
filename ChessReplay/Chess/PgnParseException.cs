namespace ChessReplay.Chess;

public sealed class PgnParseException()
    : Exception("This game could not be replayed because its PGN could not be parsed.");
