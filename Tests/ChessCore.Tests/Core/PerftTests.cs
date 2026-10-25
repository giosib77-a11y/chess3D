using System;
using System.Linq;
using Chess.Core;
using Xunit;

namespace ChessCore.Tests.Core
{
    public class PerftTests
    {
        private const string Kiwipete = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        private const string Pos3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
        private const string Pos4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
        private const string Pos5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

        [Theory]
        [InlineData(Position.StartFen, 1, 20)]
        [InlineData(Position.StartFen, 2, 400)]
        [InlineData(Position.StartFen, 3, 8902)]
        [InlineData(Position.StartFen, 4, 197281)]
        [InlineData(Kiwipete, 1, 48)]
        [InlineData(Kiwipete, 2, 2039)]
        [InlineData(Kiwipete, 3, 97862)]
        [InlineData(Pos3, 1, 14)]
        [InlineData(Pos3, 2, 191)]
        [InlineData(Pos3, 3, 2812)]
        [InlineData(Pos3, 4, 43238)]
        [InlineData(Pos4, 1, 6)]
        [InlineData(Pos4, 2, 264)]
        [InlineData(Pos4, 3, 9467)]
        [InlineData(Pos5, 1, 44)]
        [InlineData(Pos5, 2, 1486)]
        [InlineData(Pos5, 3, 62379)]
        public void Count_MatchesKnownNodeCounts(string fen, int depth, long expected)
        {
            Assert.Equal(expected, Perft.Count(Position.FromFen(fen), depth));
        }

        [Fact]
        public void Count_Depth0_IsOne()
        {
            Assert.Equal(1, Perft.Count(Position.CreateStart(), 0));
        }

        [Fact]
        public void Count_NegativeDepth_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Perft.Count(Position.CreateStart(), -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Perft.Divide(Position.CreateStart(), -1));
        }

        [Fact]
        public void NullPosition_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Perft.Count(null, 1));
            Assert.Throws<ArgumentNullException>(() => Perft.Divide(null, 1));
        }

        [Theory]
        [InlineData(Position.StartFen, 3)]
        [InlineData(Kiwipete, 2)]
        [InlineData(Pos4, 3)]
        public void Divide_SumsToCount_AndKeysAreRootMovesUci(string fen, int depth)
        {
            Position pos = Position.FromFen(fen);
            var divide = Perft.Divide(pos, depth);
            Assert.Equal(Perft.Count(pos, depth), divide.Values.Sum());
            Assert.Equal(
                pos.GetLegalMoves().Select(m => m.ToUci()).OrderBy(s => s),
                divide.Keys.OrderBy(s => s));
        }

        [Fact]
        public void Divide_StartPos_Depth1_EachMoveIsOne()
        {
            var divide = Perft.Divide(Position.CreateStart(), 1);
            Assert.Equal(20, divide.Count);
            Assert.All(divide.Values, v => Assert.Equal(1, v));
        }

        [Fact]
        public void StartPos_Depth5_IsCorrect()
        {
            Assert.Equal(4865609, Perft.Count(Position.CreateStart(), 5));
        }
    }
}
