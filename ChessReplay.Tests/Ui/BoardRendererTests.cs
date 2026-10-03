using ChessReplay.Chess;
using ChessReplay.Ui;

namespace ChessReplay.Tests.Ui;

public class BoardRendererTests
{
    private static BoardSnapshot MakeSnapshot(
        (int File, int Rank)? fromSquare = null, (int File, int Rank)? toSquare = null)
    {
        var board = new BoardPiece?[8, 8];
        board[4, 0] = new BoardPiece(Side.White, PieceKind.King); // e1
        board[4, 7] = new BoardPiece(Side.Black, PieceKind.King); // e8
        board[0, 6] = new BoardPiece(Side.Black, PieceKind.Pawn); // a7

        return new BoardSnapshot
        {
            Board = board,
            MoveNumber = 0,
            SideToMove = Side.White,
            FromSquare = fromSquare,
            ToSquare = toSquare,
        };
    }

    [Fact]
    public void Render_ContainsGlyphsForPlacedPieces()
    {
        var output = BoardRenderer.Render(MakeSnapshot(), flipped: false);

        Assert.Contains("[white]♔[/]", output);
        Assert.Contains("[orange1]♔[/]", output);
        Assert.Contains("[orange1]♙[/]", output);
    }

    [Fact]
    public void Render_NeverUsesFilledGlyphs()
    {
        // U+265F (filled pawn) is drawn as an emoji by some terminals, ignoring the color.
        var output = BoardRenderer.Render(MakeSnapshot(), flipped: false);

        foreach (var glyph in "♚♛♜♝♞♟")
            Assert.DoesNotContain(glyph, output);
    }

    [Fact]
    public void Render_NotFlipped_FileLabelsRunAToH()
    {
        var output = BoardRenderer.Render(MakeSnapshot(), flipped: false);

        Assert.Contains("a b c d e f g h", output);
    }

    [Fact]
    public void Render_Flipped_FileLabelsRunHToA()
    {
        var output = BoardRenderer.Render(MakeSnapshot(), flipped: true);

        Assert.Contains("h g f e d c b a", output);
    }

    [Fact]
    public void Render_FromSquare_IsHighlighted()
    {
        var output = BoardRenderer.Render(MakeSnapshot(fromSquare: (4, 0)), flipped: false);

        Assert.Contains("[white on grey37]♔[/]", output);
    }

    [Fact]
    public void Render_ToSquare_IsHighlighted()
    {
        var output = BoardRenderer.Render(MakeSnapshot(toSquare: (4, 7)), flipped: false);

        Assert.Contains("[orange1 on grey37]♔[/]", output);
    }
}
