using System;
using Chess.Core;
using Xunit;

namespace ChessCore.Tests.Core
{
    public class FenTests
    {
        [Theory]
        [InlineData(Position.StartFen)]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")]
        [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1")]
        [InlineData("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1")]
        [InlineData("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8")]
        [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
        [InlineData("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 25")]
        public void RoundTrip(string fen)
        {
            Assert.Equal(fen, Position.FromFen(fen).ToFen());
        }

        [Fact]
        public void CreateStart_HasExpectedState()
        {
            Position p = Position.CreateStart();
            Assert.Equal(Position.StartFen, p.ToFen());
            Assert.Equal(Side.White, p.SideToMove);
            Assert.Equal(CastlingRights.All, p.CastlingRights);
            Assert.Null(p.EnPassantSquare);
            Assert.Equal(0, p.HalfmoveClock);
            Assert.Equal(1, p.FullmoveNumber);
            Assert.Equal(new Piece(PieceType.Rook, Side.White), p.GetPiece(Square.Parse("a1")));
            Assert.Equal(new Piece(PieceType.Queen, Side.Black), p.GetPiece(Square.Parse("d8")));
            Assert.True(p.GetPiece(Square.Parse("e4")).IsNone);
            Assert.Equal(Square.Parse("e1"), p.FindKing(Side.White));
            Assert.Equal(Square.Parse("e8"), p.FindKing(Side.Black));
        }

        [Fact]
        public void EnPassantAndCounters_AreParsed()
        {
            Position p = Position.FromFen("4k3/8/8/3pP3/8/8/8/4K3 w - d6 7 25");
            Assert.Equal(Square.Parse("d6"), p.EnPassantSquare);
            Assert.Equal(7, p.HalfmoveClock);
            Assert.Equal(25, p.FullmoveNumber);
            Assert.Equal(CastlingRights.None, p.CastlingRights);
        }

        [Fact]
        public void ExtraSpacesAndWhitespace_AreAccepted_ToFenNormalizes()
        {
            Position p = Position.FromFen("  " + Position.StartFen.Replace(" ", "   ") + "  ");
            Assert.Equal(Position.StartFen, p.ToFen());
        }

        [Fact]
        public void CastlingString_IsNormalizedOrder()
        {
            Position p = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w qkQK - 0 1");
            Assert.Equal("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", p.ToFen());
            Position q = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w qK - 0 1");
            Assert.Equal("r3k2r/8/8/8/8/8/8/R3K2R w Kq - 0 1", q.ToFen());
            Assert.Equal(CastlingRights.WhiteKingside | CastlingRights.BlackQueenside, q.CastlingRights);
        }

        [Fact]
        public void Clone_IsIndependentCopyWithSameState()
        {
            Position p = Position.FromFen("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
            Position c = p.Clone();
            Assert.NotSame(p, c);
            Assert.Equal(p.ToFen(), c.ToFen());
        }

        [Fact]
        public void FindKing_ReturnsKingSquares()
        {
            Position p = Position.FromFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
            Assert.Equal(Square.Parse("a5"), p.FindKing(Side.White));
            Assert.Equal(Square.Parse("h4"), p.FindKing(Side.Black));
        }

        [Fact]
        public void NullFen_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => Position.FromFen(null));
        }

        [Theory]
        // wrong field count
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - -")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 1 extra")]
        // rank length wrong
        [InlineData("4k3/8/8/8/8/8/8/3K3 w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/5K3 w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K2P w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/44K w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8//4K3 w - - 0 1")]
        // wrong rank count
        [InlineData("4k3/8/8/8/8/8/4K3 w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/8/4K3 w - - 0 1")]
        // unknown char
        [InlineData("4k3/8/8/8/8/8/8/4K2x w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K30 w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K2- w - - 0 1")]
        // no king / two kings
        [InlineData("8/8/8/8/8/8/8/4K3 w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/8 w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4KK2 w - - 0 1")]
        [InlineData("4k2k/8/8/8/8/8/8/4K3 w - - 0 1")]
        // pawn on rank 1 / 8
        [InlineData("P3k3/8/8/8/8/8/8/4K3 w - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K2p w - - 0 1")]
        // bad side to move
        [InlineData("4k3/8/8/8/8/8/8/4K3 x - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 W - - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 white - - 0 1")]
        // bad castling string
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w KK - 0 1")]
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w KQkqK - 0 1")]
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w KX - 0 1")]
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w -K - 0 1")]
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w -- 0 1")]
        // castling right without king or rook on home squares
        [InlineData("4k3/8/8/8/8/8/8/4K3 w K - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/R3K3 w K - 0 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K2R w Q - 0 1")]
        [InlineData("5k1r/8/8/8/8/8/8/4K3 w k - 0 1")]
        [InlineData("r3k3/8/8/8/8/8/8/4K3 w k - 0 1")]
        [InlineData("r2k3r/8/8/8/8/8/8/R2K3R w KQkq - 0 1")]
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2r w K - 0 1")]
        // en passant square on wrong rank / invalid
        [InlineData("4k3/8/8/3pP3/8/8/8/4K3 w - d3 0 1")]
        [InlineData("4k3/8/8/8/3Pp3/8/8/4K3 b - e6 0 1")]
        [InlineData("4k3/8/8/3pP3/8/8/8/4K3 w - D6 0 1")]
        [InlineData("4k3/8/8/3pP3/8/8/8/4K3 w - z9 0 1")]
        [InlineData("4k3/8/8/3pP3/8/8/8/4K3 w - d 0 1")]
        // negative halfmove, fullmove 0, non-integer counters
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - -1 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 0")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 -3")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - x 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 y")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 1.5 1")]
        [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 99999999999 1")]
        public void InvalidFen_ThrowsFormatException(string fen)
        {
            var ex = Assert.Throws<FormatException>(() => Position.FromFen(fen));
            Assert.StartsWith("Invalid FEN: ", ex.Message);
            Assert.Contains(fen, ex.Message);
        }
    }
}
