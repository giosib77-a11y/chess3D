using System;
using Chess.Core;
using Xunit;

namespace ChessCore.Tests.Core
{
    public class SquareTests
    {
        [Fact]
        public void ParseToString_RoundTripsAll64Squares()
        {
            for (int i = 0; i < 64; i++)
            {
                var sq = new Square(i);
                string name = sq.ToString();
                Assert.Equal(sq, Square.Parse(name));
                Assert.True(Square.TryParse(name, out Square parsed));
                Assert.Equal(i, parsed.Index);
            }
        }

        [Fact]
        public void IndexFileRank_AreMapped()
        {
            Assert.Equal(0, Square.Parse("a1").Index);
            Assert.Equal(63, Square.Parse("h8").Index);
            Assert.Equal(7, Square.Parse("h1").Index);
            Assert.Equal(8, Square.Parse("a2").Index);
            Square e4 = Square.Parse("e4");
            Assert.Equal(4, e4.File);
            Assert.Equal(3, e4.Rank);
            Assert.Equal(e4, Square.FromFileRank(4, 3));
            Assert.Equal("a1", default(Square).ToString());
        }

        [Theory]
        [InlineData("i1")]
        [InlineData("a9")]
        [InlineData("a0")]
        [InlineData("E4")]
        [InlineData("e")]
        [InlineData("e44")]
        [InlineData(" e4")]
        [InlineData("")]
        [InlineData(null)]
        public void Invalid_ParseThrowsFormatException_TryParseFalse(string name)
        {
            // Parse(null) throws FormatException (not ArgumentNullException), same as other invalid input.
            var ex = Assert.Throws<FormatException>(() => Square.Parse(name));
            Assert.Equal("Invalid square: '" + name + "'", ex.Message);
            Assert.False(Square.TryParse(name, out Square sq));
            Assert.Equal(0, sq.Index);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(64)]
        public void Constructor_OutOfRange_Throws(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Square(index));
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(8, 0)]
        [InlineData(0, -1)]
        [InlineData(0, 8)]
        public void FromFileRank_OutOfRange_Throws(int file, int rank)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Square.FromFileRank(file, rank));
        }

        [Fact]
        public void Equality_Works()
        {
            Assert.True(new Square(10) == new Square(10));
            Assert.True(new Square(10) != new Square(11));
            Assert.Equal(new Square(10).GetHashCode(), new Square(10).GetHashCode());
        }
    }
}
