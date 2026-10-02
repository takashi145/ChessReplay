using System.Text;
using ChessReplay.Chess;

namespace ChessReplay.Ui;

public static class BoardRenderer
{
    private static readonly Dictionary<(Side, PieceKind), char> Glyphs = new()
    {
        [(Side.White, PieceKind.King)] = '♔',
        [(Side.White, PieceKind.Queen)] = '♕',
        [(Side.White, PieceKind.Rook)] = '♖',
        [(Side.White, PieceKind.Bishop)] = '♗',
        [(Side.White, PieceKind.Knight)] = '♘',
        [(Side.White, PieceKind.Pawn)] = '♙',
        [(Side.Black, PieceKind.King)] = '♚',
        [(Side.Black, PieceKind.Queen)] = '♛',
        [(Side.Black, PieceKind.Rook)] = '♜',
        [(Side.Black, PieceKind.Bishop)] = '♝',
        [(Side.Black, PieceKind.Knight)] = '♞',
        [(Side.Black, PieceKind.Pawn)] = '♟',
    };

    // Returns Spectre.Console markup for the 8x8 board only (caller adds header/footer).
    public static string Render(BoardSnapshot snapshot, bool flipped)
    {
        var files = FileLabels(flipped);
        var builder = new StringBuilder();

        builder.Append("    ").Append(files).Append('\n');
        builder.Append("  ┌─────────────────┐\n");

        foreach (var rank in Ranks(flipped))
        {
            builder.Append(rank + 1).Append(" │ ");

            foreach (var file in Files(flipped))
            {
                var piece = snapshot.Board[file, rank];
                var glyph = piece is null ? ' ' : Glyphs[(piece.Value.Color, piece.Value.Kind)];
                var highlighted = snapshot.FromSquare == (file, rank) || snapshot.ToSquare == (file, rank);

                builder.Append(highlighted ? $"[black on yellow]{glyph}[/]" : glyph.ToString());
                builder.Append(' ');
            }

            builder.Append("│\n");
        }

        builder.Append("  └─────────────────┘\n");
        builder.Append("    ").Append(files);

        return builder.ToString();
    }

    private static IEnumerable<int> Ranks(bool flipped) =>
        flipped ? Enumerable.Range(0, 8) : Enumerable.Range(0, 8).Reverse();

    private static IEnumerable<int> Files(bool flipped) =>
        flipped ? Enumerable.Range(0, 8).Reverse() : Enumerable.Range(0, 8);

    private static string FileLabels(bool flipped) =>
        string.Join(' ', Files(flipped).Select(f => (char)('a' + f)));
}
