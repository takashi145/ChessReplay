using ChessReplay.Models;

namespace ChessReplay.Tests.Models;

public class GameArchiveTests
{
    [Fact]
    public void TryParse_ValidFormat_ReturnsArchive()
    {
        var success = GameArchive.TryParse("2026-08", out var archive);

        Assert.True(success);
        Assert.Equal(new GameArchive(2026, 8), archive);
    }

    [Theory]
    [InlineData("26-08")] // year not 4 digits
    [InlineData("2026-13")] // month out of range
    [InlineData("2026-00")] // month out of range
    [InlineData("")]
    [InlineData("2026")] // no separator
    [InlineData("yyyy-mm")] // non-numeric
    [InlineData("2026-8-1")] // too many parts
    public void TryParse_InvalidFormat_ReturnsFalse(string value)
    {
        var success = GameArchive.TryParse(value, out var archive);

        Assert.False(success);
        Assert.Null(archive);
    }

    [Fact]
    public void TryParseFromUrl_ValidUrl_ReturnsArchive()
    {
        var success = GameArchive.TryParseFromUrl(
            "https://api.chess.com/pub/player/test-user/games/2026/08", out var archive);

        Assert.True(success);
        Assert.Equal(new GameArchive(2026, 8), archive);
    }

    [Fact]
    public void TryParseFromUrl_TrailingSlash_ReturnsArchive()
    {
        var success = GameArchive.TryParseFromUrl(
            "https://api.chess.com/pub/player/test-user/games/2026/08/", out var archive);

        Assert.True(success);
        Assert.Equal(new GameArchive(2026, 8), archive);
    }

    [Fact]
    public void TryParseFromUrl_TooFewSegments_ReturnsFalse()
    {
        var success = GameArchive.TryParseFromUrl("2026", out var archive);

        Assert.False(success);
        Assert.Null(archive);
    }

    [Fact]
    public void ToString_PadsYearAndMonth()
    {
        var archive = new GameArchive(7, 1);

        Assert.Equal("0007-01", archive.ToString());
    }
}
