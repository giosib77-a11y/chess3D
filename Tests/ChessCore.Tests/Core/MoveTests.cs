using System;
using Chess.Core;
using Xunit;

namespace ChessCore.Tests.Core
{
    public class MoveTests
    {
        [Theory]
        [InlineData("e2e4", PieceType.None)]
        [InlineData("e7e8q", PieceType.Queen)]
        [InlineData("a2a1n", PieceType.Knight)]
        [InlineData("h7h8r", PieceType.Rook)]
        [InlineData("b7b8b", PieceType.Bishop)]
        public void Uci_RoundTrips(string uci, PieceType promotion)
        {
            Assert.True(Move.TryParseUci(uci, out Move move));
            Assert.Equal(promotion, move.Promotion);
            Assert.Equal(promotion != PieceType.None, move.IsPromotion);
            Assert.Equal(MoveFlags.None, move.Flags);
            Assert.Equal(uci, move.ToUci());
        }

        [Theory]
        [InlineData("e2e9")]
        [InlineData("e7e8k")]
        [InlineData("e7e8p")]
        [InlineData("e7e8Q")]
        [InlineData("e2")]
        [InlineData("e2e4qq")]
        [InlineData("E2E4")]
        [InlineData("")]
        [InlineData(null)]
        public void InvalidUci_ReturnsFalse(string uci)
        {
            Assert.False(Move.TryParseUci(uci, out Move move));
            Assert.Equal(default(Move), move);
        }

        [Fact]
        public void Equality_IgnoresFlags()
        {
            Square e5 = Square.Parse("e5"), d6 = Square.Parse("d6");
            var plain = new Move(e5, d6);
            var capture = new Move(e5, d6, PieceType.None, MoveFlags.Capture | MoveFlags.EnPassant);
            Assert.Equal(plain, capture);
            Assert.True(plain == capture);
            Assert.Equal(plain.GetHashCode(), capture.GetHashCode());
            Assert.True(capture.IsCapture);
            Assert.False(plain.IsCapture);
        }

        [Fact]
        public void Equality_UsesFromToPromotion()
        {
            Square a7 = Square.Parse("a7"), a8 = Square.Parse("a8");
            Assert.NotEqual(new Move(a7, a8, PieceType.Queen), new Move(a7, a8, PieceType.Knight));
            Assert.NotEqual(new Move(a7, a8, PieceType.Queen), new Move(a7, a8));
            Assert.NotEqual(new Move(a7, a8), new Move(a8, a7));
        }

        [Fact]
        public void ParsedUci_EqualsMoveWithFlags()
        {
            Move.TryParseUci("e1g1", out Move parsed);
            var generated = new Move(Square.Parse("e1"), Square.Parse("g1"), PieceType.None, MoveFlags.CastleKingside);
            Assert.Equal(generated, parsed);
            Assert.True(generated.IsCastle);
            Assert.Equal("e1g1", generated.ToUci());
        }

        [Theory]
        [InlineData(PieceType.Pawn)]
        [InlineData(PieceType.King)]
        public void Constructor_InvalidPromotion_Throws(PieceType promotion)
        {
            Assert.Throws<ArgumentException>(() => new Move(new Square(0), new Square(1), promotion));
        }

        [Fact]
        public void PromotionFlag_IsAccessible()
        {
            var m = new Move(Square.Parse("e7"), Square.Parse("e8"), PieceType.Queen, MoveFlags.Promotion);
            Assert.True(m.IsPromotion);
            Assert.False(m.IsCastle);
        }
    }
}
