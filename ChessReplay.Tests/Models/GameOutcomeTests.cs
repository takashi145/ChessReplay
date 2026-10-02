using ChessReplay.Models;

namespace ChessReplay.Tests.Models;

public class GameOutcomeTests
{
    private static Player MakePlayer(string rawResult) => new("player", 1500, rawResult);

    [Fact]
    public void Describe_SelfWon_ReturnsWinWithOpponentsReason()
    {
        // Chess.com always reports the winner's own RawResult as "win", so the
        // actual reason (checkmate, timeout, ...) has to be read off the opponent.
        var self = MakePlayer("win");
        var opponent = MakePlayer("checkmated");

        var (result, reason) = GameOutcome.Describe(self, opponent);

        Assert.Equal(GameResult.Win, result);
        Assert.Equal("Checkmate", reason);
    }

    [Theory]
    [InlineData("agreed")]
    [InlineData("repetition")]
    [InlineData("stalemate")]
    [InlineData("insufficient")]
    [InlineData("50move")]
    [InlineData("timevsinsufficient")]
    public void Describe_SelfRawResultIsDrawVariant_ReturnsDraw(string drawRawResult)
    {
        var self = MakePlayer(drawRawResult);
        var opponent = MakePlayer(drawRawResult);

        var (result, _) = GameOutcome.Describe(self, opponent);

        Assert.Equal(GameResult.Draw, result);
    }

    [Fact]
    public void Describe_SelfLost_ReturnsLossWithOwnReason()
    {
        var self = MakePlayer("timeout");
        var opponent = MakePlayer("win");

        var (result, reason) = GameOutcome.Describe(self, opponent);

        Assert.Equal(GameResult.Loss, result);
        Assert.Equal("Timeout", reason);
    }

    [Theory]
    [InlineData("checkmated", "Checkmate")]
    [InlineData("timeout", "Timeout")]
    [InlineData("resigned", "Resignation")]
    [InlineData("repetition", "Repetition")]
    [InlineData("stalemate", "Stalemate")]
    [InlineData("insufficient", "Insufficient Material")]
    [InlineData("50move", "50-move Rule")]
    [InlineData("agreed", "Agreement")]
    [InlineData("abandoned", "Abandoned")]
    [InlineData("timevsinsufficient", "Timeout vs Insufficient Material")]
    [InlineData("win", "Win")]
    public void Describe_KnownRawResult_MapsToExpectedReason(string rawResult, string expectedReason)
    {
        var self = MakePlayer(rawResult);
        var opponent = MakePlayer("win");

        var (_, reason) = GameOutcome.Describe(self, opponent);

        Assert.Equal(expectedReason, reason);
    }

    [Fact]
    public void Describe_UnknownRawResult_ReturnsRawResultAsIs()
    {
        var self = MakePlayer("some_future_reason");
        var opponent = MakePlayer("win");

        var (_, reason) = GameOutcome.Describe(self, opponent);

        Assert.Equal("some_future_reason", reason);
    }
}
