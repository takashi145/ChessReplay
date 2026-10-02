using ChessReplay.Chess;

namespace ChessReplay.Tests.Chess;

public class ReplayBuilderTests
{
    [Fact]
    public void Build_SimpleGame_ReturnsOneSnapshotPerPlyPlusInitial()
    {
        var board = PgnParser.Parse("1. e4 e5 2. Nf3 Nc6");

        var snapshots = ReplayBuilder.Build(board);

        Assert.Equal(5, snapshots.Count); // initial position + 4 plies
        Assert.Null(snapshots[0].San);
        Assert.Equal(Side.White, snapshots[0].SideToMove);

        Assert.Equal("e4", snapshots[1].San);
        Assert.Equal(1, snapshots[1].MoveNumber);
        Assert.Equal(Side.Black, snapshots[1].SideToMove);

        Assert.Equal("e5", snapshots[2].San);
        Assert.Equal("Nf3", snapshots[3].San);
        Assert.Equal("Nc6", snapshots[4].San);
    }

    [Fact]
    public void Build_PawnCapture_SetsCapturedPiece()
    {
        var board = PgnParser.Parse("1. e4 d5 2. exd5");

        var snapshots = ReplayBuilder.Build(board);
        var captureSnapshot = snapshots[3]; // after "exd5"

        Assert.Equal("exd5", captureSnapshot.San);
        Assert.NotNull(captureSnapshot.CapturedPiece);
        Assert.Equal(Side.Black, captureSnapshot.CapturedPiece!.Value.Color);
        Assert.Equal(PieceKind.Pawn, captureSnapshot.CapturedPiece!.Value.Kind);
    }

    [Fact]
    public void Build_Checkmate_SetsCheckAndCheckmateFlags()
    {
        var board = PgnParser.Parse("1. e4 e5 2. Bc4 Nc6 3. Qh5 Nf6 4. Qxf7#");

        var snapshots = ReplayBuilder.Build(board);
        var finalSnapshot = snapshots[^1];

        Assert.Equal("Qxf7#", finalSnapshot.San);
        Assert.True(finalSnapshot.IsCheck);
        Assert.True(finalSnapshot.IsCheckmate);
    }

    [Fact]
    public void Build_DoesNotChangeBoardsMoveIndexAfterReturning()
    {
        // Guards the `finally` restore in ReplayBuilder; without it, reusing `board` after Build() would silently show the wrong position.
        var board = PgnParser.Parse("1. e4 e5 2. Nf3 Nc6");
        var originalMoveIndex = board.MoveIndex;

        ReplayBuilder.Build(board);

        Assert.Equal(originalMoveIndex, board.MoveIndex);
    }

    [Fact]
    public void Parse_IllegalMoveSequence_ThrowsPgnParseException()
    {
        // Black cannot play e4 twice in a row.
        Assert.Throws<PgnParseException>(() => PgnParser.Parse("1. e4 e4 2. e4 e4"));
    }
}
