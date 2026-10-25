using Chess.AI;
using Xunit;

namespace ChessCore.Tests.AI
{
    public class AIDifficultyTests
    {
        [Theory]
        [InlineData(DifficultyPreset.Easy, 0, 1, 100, null)]
        [InlineData(DifficultyPreset.Medium, 20, null, 300, 1400)]
        [InlineData(DifficultyPreset.Hard, 20, null, 500, 1800)]
        [InlineData(DifficultyPreset.Master, 20, null, 1000, 2200)]
        public void FromPreset_ReturnsTableValues(DifficultyPreset preset, int skill, int? depth, int moveTime, int? elo)
        {
            var d = AIDifficulty.FromPreset(preset);
            Assert.Equal(skill, d.SkillLevel);
            Assert.Equal(depth, d.Depth);
            Assert.Equal(moveTime, d.MoveTimeMs);
            Assert.Equal(elo, d.Elo);
            Assert.Equal(elo.HasValue, d.LimitStrength);
        }

        [Fact]
        public void FromPreset_UnknownValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AIDifficulty.FromPreset((DifficultyPreset)99));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(21)]
        public void SkillLevel_OutOfRange_Throws(int skill)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AIDifficulty(skill, null, 100, null));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(31)]
        public void Depth_OutOfRange_Throws(int depth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AIDifficulty(0, depth, 100, null));
        }

        [Theory]
        [InlineData(49)]
        [InlineData(60001)]
        public void MoveTime_OutOfRange_Throws(int ms)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AIDifficulty(0, null, ms, null));
        }

        [Theory]
        [InlineData(499)]
        [InlineData(3501)]
        public void Elo_OutOfRange_Throws(int elo)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AIDifficulty(0, null, 100, elo));
        }

        [Fact]
        public void ValidBoundaries_Accepted()
        {
            var lo = new AIDifficulty(0, 1, 50, 500);
            var hi = new AIDifficulty(20, 30, 60000, 3500);
            Assert.Equal(500, lo.Elo);
            Assert.Equal(30, hi.Depth);
            Assert.Equal(60000, hi.MoveTimeMs);
            Assert.True(hi.LimitStrength);
        }
    }
}
