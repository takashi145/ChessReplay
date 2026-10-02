using Chess;

namespace ChessReplay.Chess;

public static class ReplayBuilder
{
    public static IReadOnlyList<BoardSnapshot> Build(ChessBoard board)
    {
        var originalMoveIndex = board.MoveIndex;

        try
        {
            var moves = board.ExecutedMoves;
            var snapshots = new List<BoardSnapshot>(moves.Count + 1);

            board.MoveIndex = -1;
            snapshots.Add(new BoardSnapshot
            {
                Board = CaptureBoard(board),
                MoveNumber = 0,
                SideToMove = ToSide(board.Turn),
            });

            for (var i = 0; i < moves.Count; i++)
            {
                board.MoveIndex = i;
                var move = moves[i];

                snapshots.Add(new BoardSnapshot
                {
                    Board = CaptureBoard(board),
                    MoveNumber = i + 1,
                    SideToMove = ToSide(board.Turn),
                    San = move.San,
                    FromSquare = (move.OriginalPosition.X, move.OriginalPosition.Y),
                    ToSquare = (move.NewPosition.X, move.NewPosition.Y),
                    CapturedPiece = move.CapturedPiece is { } captured ? ToBoardPiece(captured) : null,
                    IsCheck = move.IsCheck,
                    IsCheckmate = move.IsMate,
                });
            }

            return snapshots;
        }
        finally
        {
            board.MoveIndex = originalMoveIndex;
        }
    }

    private static BoardPiece?[,] CaptureBoard(ChessBoard board)
    {
        var grid = new BoardPiece?[8, 8];

        for (var file = 0; file < 8; file++)
        {
            for (var rank = 0; rank < 8; rank++)
            {
                var piece = board[file, rank];
                grid[file, rank] = piece is null ? null : ToBoardPiece(piece);
            }
        }

        return grid;
    }

    private static BoardPiece ToBoardPiece(Piece piece) => new(ToSide(piece.Color), ToKind(piece.Type));

    private static Side ToSide(PieceColor color) => color == PieceColor.White ? Side.White : Side.Black;

    private static PieceKind ToKind(PieceType type)
    {
        if (type == PieceType.Pawn) return PieceKind.Pawn;
        if (type == PieceType.Knight) return PieceKind.Knight;
        if (type == PieceType.Bishop) return PieceKind.Bishop;
        if (type == PieceType.Rook) return PieceKind.Rook;
        if (type == PieceType.Queen) return PieceKind.Queen;
        if (type == PieceType.King) return PieceKind.King;
        throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown piece type.");
    }
}
