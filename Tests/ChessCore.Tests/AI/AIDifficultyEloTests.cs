using Chess.AI;
using Xunit;

namespace ChessCore.Tests.AI
{
    public class AIDifficultyEloTests
    {
        private static Dictionary<string, UciOption> FullOptions()
        {
            var d = new Dictionary<string, UciOption>();
            d["Skill Level"] = new UciOption("Skill Level", "spin", "20", 0, 20);
            d["UCI_LimitStrength"] = new UciOption("UCI_LimitStrength", "check", "false", null, null);
            d["UCI_Elo"] = new UciOption("UCI_Elo", "spin", "1320", 1320, 3190);
            return d;
        }

        [Theory]
        [InlineData(500, 0, 1, 100, null)]
        [InlineData(800, 0, 1, 100, null)]
        [InlineData(999, 0, 1, 100, null)]
        [InlineData(1000, 3, 2, 100, null)]
        [InlineData(1199, 3, 2, 100, null)]
        [InlineData(1200, 6, 4, 100, null)]
        [InlineData(1319, 6, 4, 100, null)]
        [InlineData(1320, 20, null, 300, 1320)]
        [InlineData(1599, 20, null, 300, 1599)]
        [InlineData(1600, 20, null, 500, 1600)]
        [InlineData(1999, 20, null, 500, 1999)]
        [InlineData(2000, 20, null, 1000, 2000)]
        [InlineData(3500, 20, null, 1000, 3500)]
        public void FromElo_Bands(int elo, int skill, int? depth, int moveTime, int? expectedElo)
        {
            var d = AIDifficulty.FromElo(elo);
            Assert.Equal(skill, d.SkillLevel);
            Assert.Equal(depth, d.Depth);
            Assert.Equal(moveTime, d.MoveTimeMs);
            Assert.Equal(expectedElo, d.Elo);
        }

        [Theory]
        [InlineData(499)]
        [InlineData(3501)]
        [InlineData(int.MinValue)]
        public void FromElo_OutOfRange_Throws(int elo)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AIDifficulty.FromElo(elo));
        }

        [Theory]
        [InlineData(800, DifficultyPreset.Easy)]
        [InlineData(1400, DifficultyPreset.Medium)]
        [InlineData(1800, DifficultyPreset.Hard)]
        [InlineData(2200, DifficultyPreset.Master)]
        public void FromElo_MatchesPresets(int elo, DifficultyPreset preset)
        {
            var a = AIDifficulty.FromElo(elo);
            var b = AIDifficulty.FromPreset(preset);
            Assert.Equal(b.SkillLevel, a.SkillLevel);
            Assert.Equal(b.Depth, a.Depth);
            Assert.Equal(b.MoveTimeMs, a.MoveTimeMs);
            Assert.Equal(b.Elo, a.Elo);
            Assert.Equal(b.LimitStrength, a.LimitStrength);
        }

        [Fact]
        public void FromElo_Below1320_SendsSkillAndDepth()
        {
            var d = AIDifficulty.FromElo(1100);
            var cmds = UciProtocol.BuildDifficultyCommands(d, FullOptions());
            Assert.Equal(new[]
            {
                "setoption name Skill Level value 3",
                "setoption name UCI_LimitStrength value false"
            }, cmds);
            Assert.Equal("go depth 2", UciProtocol.BuildGo(d));
        }

        [Fact]
        public void FromElo_AtOrAbove1320_SendsUciElo()
        {
            var d = AIDifficulty.FromElo(1800);
            var cmds = UciProtocol.BuildDifficultyCommands(d, FullOptions());
            Assert.Equal(new[]
            {
                "setoption name Skill Level value 20",
                "setoption name UCI_LimitStrength value true",
                "setoption name UCI_Elo value 1800"
            }, cmds);
            Assert.Equal("go movetime 500", UciProtocol.BuildGo(d));
        }

        [Fact]
        public void MinUciElo_Is1320()
        {
            Assert.Equal(1320, AIDifficulty.MinUciElo);
        }
    }
}
