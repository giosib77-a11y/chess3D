using Chess.Core;
using Xunit;

namespace ChessCore.Tests.Core
{
    public class DrawRulesTests
    {
        [Theory]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 1")] // K v K
        [InlineData("4k3/8/8/8/8/8/8/2B1K3 w - - 0 1")] // KB v K
        [InlineData("4k3/8/8/8/8/8/8/1N2K3 w - - 0 1")] // KN v K
        [InlineData("4k3/8/8/8/8/8/8/1n2K3 w - - 0 1")] // K v KN
        [InlineData("4kb2/8/8/8/8/8/8/2B1K3 w - - 0 1")] // KB v KB, same color
        [InlineData("4k3/8/8/8/8/4B3/8/2B1K3 w - - 0 1")] // two same-color bishops
        public void InsufficientMaterial_IsDraw(string fen)
        {
            var game = new GameState(fen);
            Assert.Equal(GameStatus.InsufficientMaterial, game.Status);
            Assert.Equal(GameResult.Draw, game.Result);
            Assert.True(game.IsGameOver);
            Assert.Empty(game.LegalMoves);
            Assert.True(DrawRules.IsInsufficientMaterial(Position.FromFen(fen)));
        }

        [Theory]
        [InlineData("4kb2/8/8/8/8/8/8/4KB2 w - - 0 1")] // KB v KB, opposite colors
        [InlineData("4k3/8/8/8/8/8/8/1N1NK3 w - - 0 1")] // KNN v K
        [InlineData("4k3/8/8/8/8/8/4P3/4K3 w - - 0 1")] // KP v K
        [InlineData("4k3/8/8/8/8/8/8/R3K3 w - - 0 1")] // KR v K
        [InlineData("4k3/8/8/8/8/8/8/3QK3 w - - 0 1")] // KQ v K
        [InlineData("4k3/8/8/8/8/8/8/2B1K1N1 w - - 0 1")] // KBN v K
        [InlineData("4kn2/8/8/8/8/8/8/2B1K3 w - - 0 1")] // KB v KN
        [InlineData("4k3/8/8/8/8/8/8/2B1KB2 w - - 0 1")] // two opposite-color bishops
        public void SufficientMaterial_IsInProgress(string fen)
        {
            var game = new GameState(fen);
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.Equal(GameResult.None, game.Result);
            Assert.False(DrawRules.IsInsufficientMaterial(Position.FromFen(fen)));
        }

        [Fact]
        public void CaptureLeavingKingVsKing_BecomesInsufficientMaterial()
        {
            var game = new GameState("4k3/8/8/8/8/8/4r3/4K3 w - - 0 1");
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.True(game.TryMakeUciMove("e1e2"));
            Assert.Equal(GameStatus.InsufficientMaterial, game.Status);
        }
    }
}
