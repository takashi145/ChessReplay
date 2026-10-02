using ChessReplay.Models;

namespace ChessReplay.Tests.Models;

public class ChessGameTests
{
    private const string WhiteTestUser = "test-white-user";
    private const string BlackTestUser = "test-black-user";

    private static ChessGame MakeGame(string whiteUsername, string blackUsername) => new(
        Pgn: "1. e4 e5",
        EndTime: DateTimeOffset.UnixEpoch,
        TimeControl: "600",
        TimeClass: "blitz",
        Rated: true,
        White: new Player(whiteUsername, 1500, "win"),
        Black: new Player(blackUsername, 1500, "checkmated"),
        Url: "https://www.chess.com/game/live/1");

    [Theory]
    [InlineData("test-white-user")]
    [InlineData("TEST-WHITE-USER")]
    [InlineData("Test-White-User")]
    public void GetPlayer_MatchesWhiteUsernameCaseInsensitively_ReturnsWhite(string lookupUsername)
    {
        var game = MakeGame(WhiteTestUser, BlackTestUser);

        var player = game.GetPlayer(lookupUsername);

        Assert.Equal(WhiteTestUser, player.Username);
    }

    [Fact]
    public void GetPlayer_UsernameMatchesBlack_ReturnsBlack()
    {
        var game = MakeGame(WhiteTestUser, BlackTestUser);

        var player = game.GetPlayer(BlackTestUser);

        Assert.Equal(BlackTestUser, player.Username);
    }

    [Fact]
    public void GetOpponent_ReturnsOtherPlayer()
    {
        var game = MakeGame(WhiteTestUser, BlackTestUser);

        Assert.Equal(BlackTestUser, game.GetOpponent(WhiteTestUser).Username);
        Assert.Equal(WhiteTestUser, game.GetOpponent(BlackTestUser).Username);
    }
}
