using System;
using Chess.Campaign;
using Chess.Core;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class BattleRulesTests
    {
        [Theory]
        [InlineData(GameResult.WhiteWins, Side.White, BattleOutcome.Victory)]
        [InlineData(GameResult.WhiteWins, Side.Black, BattleOutcome.Defeat)]
        [InlineData(GameResult.BlackWins, Side.White, BattleOutcome.Defeat)]
        [InlineData(GameResult.BlackWins, Side.Black, BattleOutcome.Victory)]
        [InlineData(GameResult.Draw, Side.White, BattleOutcome.Draw)]
        [InlineData(GameResult.Draw, Side.Black, BattleOutcome.Draw)]
        public void GetOutcome_MapsResultAndSide(GameResult result, Side side, BattleOutcome expected)
        {
            Assert.Equal(expected, BattleRules.GetOutcome(result, side));
        }

        [Fact]
        public void GetOutcome_None_Throws()
        {
            Assert.Throws<ArgumentException>(() => BattleRules.GetOutcome(GameResult.None, Side.White));
            Assert.Throws<ArgumentException>(() => BattleRules.GetOutcome((GameResult)99, Side.Black));
        }

        [Fact]
        public void GetTitle_AllOutcomes()
        {
            Assert.Equal("Victory!", BattleRules.GetTitle(BattleOutcome.Victory));
            Assert.Equal("Defeat", BattleRules.GetTitle(BattleOutcome.Defeat));
            Assert.Equal("Draw", BattleRules.GetTitle(BattleOutcome.Draw));
            Assert.Throws<ArgumentOutOfRangeException>(() => BattleRules.GetTitle((BattleOutcome)99));
        }

        [Fact]
        public void NotCountedTitle_IsGameOver()
        {
            Assert.Equal("Game over", BattleRules.NotCountedTitle);
        }
    }
}
