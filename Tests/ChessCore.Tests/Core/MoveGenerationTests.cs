using System.Linq;
using Chess.Core;
using Xunit;

namespace ChessCore.Tests.Core
{
    public class MoveGenerationTests
    {
        private static Move Find(Position pos, string uci)
        {
            return pos.GetLegalMoves().Single(m => m.ToUci() == uci);
        }

        private static bool Has(Position pos, string uci)
        {
            return pos.GetLegalMoves().Any(m => m.ToUci() == uci);
        }

        private static Square Sq(string name) => Square.Parse(name);

        [Fact]
        public void EnPassant_WithHorizontallyPinnedPawn_IsIllegal()
        {
            Position pos = Position.FromFen("8/8/8/KPp4r/8/8/8/7k w - c6 0 1");
            Assert.False(Has(pos, "b5c6"));
            Assert.True(Has(pos, "b5b6"));
        }

        [Fact]
        public void EnPassant_Normal_IsGeneratedAndRemovesCapturedPawn()
        {
            Position pos = Position.FromFen("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1");
            Move m = Find(pos, "e5d6");
            Assert.Equal(MoveFlags.EnPassant, m.Flags);
            Position next = pos.MakeMove(m);
            Assert.Equal("4k3/8/3P4/8/8/8/8/4K3 b - - 0 1", next.ToFen());
        }

        [Fact]
        public void DoublePush_SetsFlagAndEnPassantSquare()
        {
            Position pos = Position.CreateStart();
            Move m = Find(pos, "e2e4");
            Assert.Equal(MoveFlags.DoublePawnPush, m.Flags);
            Assert.Equal("e3", pos.MakeMove(m).EnPassantSquare.Value.ToString());
        }

        [Fact]
        public void DoublePush_SetsEnPassantSquare_EvenWithoutAdjacentEnemyPawn()
        {
            Position next = Position.CreateStart().MakeMove(Find(Position.CreateStart(), "a2a4"));
            Assert.Equal("a3", next.EnPassantSquare.Value.ToString());
        }

        [Fact]
        public void EnPassantSquare_ClearedAfterNextMove()
        {
            Position pos = Position.CreateStart();
            Position afterE4 = pos.MakeMove(Find(pos, "e2e4"));
            Position afterNf6 = afterE4.MakeMove(Find(afterE4, "g8f6"));
            Assert.Null(afterNf6.EnPassantSquare);
        }

        [Fact]
        public void Castling_BothSidesAvailable_FlagsSet()
        {
            Position pos = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            Assert.Equal(MoveFlags.CastleKingside, Find(pos, "e1g1").Flags);
            Assert.Equal(MoveFlags.CastleQueenside, Find(pos, "e1c1").Flags);
        }

        [Fact]
        public void Castling_MovesRookAndClearsRights()
        {
            Position pos = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            Assert.Equal("r3k2r/8/8/8/8/8/8/R4RK1 b kq - 1 1", pos.MakeMove(Find(pos, "e1g1")).ToFen());
            Assert.Equal("r3k2r/8/8/8/8/8/8/2KR3R b kq - 1 1", pos.MakeMove(Find(pos, "e1c1")).ToFen());
        }

        [Fact]
        public void Castling_BlackSide_Works()
        {
            Position pos = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1");
            Assert.Equal("2kr3r/8/8/8/8/8/8/R3K2R w KQ - 1 2", pos.MakeMove(Find(pos, "e8c8")).ToFen());
            Assert.Equal("r4rk1/8/8/8/8/8/8/R3K2R w KQ - 1 2", pos.MakeMove(Find(pos, "e8g8")).ToFen());
        }

        [Fact]
        public void Castling_ImpossibleWhileInCheck()
        {
            Position pos = Position.FromFen("4r1k1/8/8/8/8/8/8/R3K2R w KQ - 0 1");
            Assert.True(pos.IsInCheck());
            Assert.False(Has(pos, "e1g1"));
            Assert.False(Has(pos, "e1c1"));
        }

        [Fact]
        public void Castling_ImpossibleThroughAttackedSquare()
        {
            // Black rook on f8 attacks f1 (kingside path); black rook on d8 attacks d1 (queenside path).
            Position kingside = Position.FromFen("5rk1/8/8/8/8/8/8/R3K2R w KQ - 0 1");
            Assert.False(Has(kingside, "e1g1"));
            Assert.True(Has(kingside, "e1c1"));

            Position queenside = Position.FromFen("3r2k1/8/8/8/8/8/8/R3K2R w KQ - 0 1");
            Assert.False(Has(queenside, "e1c1"));
            Assert.True(Has(queenside, "e1g1"));
        }

        [Fact]
        public void Castling_ImpossibleOntoAttackedSquare()
        {
            Position kingside = Position.FromFen("6rk/8/8/8/8/8/8/R3K2R w KQ - 0 1");
            Assert.False(Has(kingside, "e1g1"));

            Position queenside = Position.FromFen("2r4k/8/8/8/8/8/8/R3K2R w KQ - 0 1");
            Assert.False(Has(queenside, "e1c1"));
        }

        [Fact]
        public void Castling_QueensideAllowedWhenOnlyB1Attacked()
        {
            // b1 is crossed by the rook only, so attacking it does not prevent castling.
            Position pos = Position.FromFen("1r5k/8/8/8/8/8/8/R3K2R w KQ - 0 1");
            Assert.True(Has(pos, "e1c1"));
        }

        [Fact]
        public void Castling_BlockedByPieceBetween()
        {
            Position pos = Position.FromFen("4k3/8/8/8/8/8/8/RN2K1NR w KQ - 0 1");
            Assert.False(Has(pos, "e1g1"));
            Assert.False(Has(pos, "e1c1"));
        }

        [Fact]
        public void Castling_WithoutRights_NotGenerated()
        {
            Position pos = Position.FromFen("4k3/8/8/8/8/8/8/R3K2R w - - 0 1");
            Assert.False(Has(pos, "e1g1"));
            Assert.False(Has(pos, "e1c1"));
        }

        [Fact]
        public void RookCapturedOnHomeSquare_RemovesMatchingRight()
        {
            Position pos = Position.FromFen("r3k2r/8/8/8/8/8/6b1/R3K2R b KQkq - 0 1");
            Position next = pos.MakeMove(Find(pos, "g2h1"));
            Assert.Equal("r3k2r/8/8/8/8/8/8/R3K2b w Qkq - 0 2", next.ToFen());
        }

        [Fact]
        public void RookCapturedOnA8_RemovesBlackQueenside()
        {
            Position pos = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            Position next = pos.MakeMove(Find(pos, "a1a8"));
            Assert.Equal("R3k2r/8/8/8/8/8/8/4K2R b Kk - 0 1", next.ToFen());
        }

        [Fact]
        public void RookMovingFromHomeSquare_RemovesMatchingRight()
        {
            Position pos = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            Assert.Equal("r3k2r/8/8/8/8/8/8/1R2K2R b Kkq - 1 1", pos.MakeMove(Find(pos, "a1b1")).ToFen());
        }

        [Fact]
        public void KingMove_ClearsBothRightsOfThatSide()
        {
            Position pos = Position.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            Assert.Equal("r3k2r/8/8/8/8/8/8/R2K3R b kq - 1 1", pos.MakeMove(Find(pos, "e1d1")).ToFen());
        }

        [Fact]
        public void Promotion_GeneratesFourMovesWithFlag()
        {
            Position pos = Position.FromFen("8/P6k/8/8/8/8/8/K7 w - - 0 1");
            var promos = pos.GetLegalMoves().Where(m => m.From == Sq("a7")).ToList();
            Assert.Equal(4, promos.Count);
            Assert.All(promos, m => Assert.Equal(MoveFlags.Promotion, m.Flags));
            Assert.Equal(
                new[] { PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen },
                promos.Select(m => m.Promotion).OrderBy(t => t).ToArray());
        }

        [Fact]
        public void PromotionCapture_HasCaptureAndPromotionFlags()
        {
            Position pos = Position.FromFen("1n5k/P7/8/8/8/8/8/K7 w - - 0 1");
            var caps = pos.GetLegalMoves().Where(m => m.ToUci().StartsWith("a7b8")).ToList();
            Assert.Equal(4, caps.Count);
            Assert.All(caps, m => Assert.Equal(MoveFlags.Capture | MoveFlags.Promotion, m.Flags));
        }

        [Fact]
        public void Promotion_ReplacesPawnOnBoard()
        {
            Position pos = Position.FromFen("8/P6k/8/8/8/8/8/K7 w - - 0 1");
            Position next = pos.MakeMove(Find(pos, "a7a8n"));
            Assert.Equal("N7/7k/8/8/8/8/8/K7 b - - 0 1", next.ToFen());
        }

        [Fact]
        public void IsInCheck_DetectsAndRejects()
        {
            Assert.False(Position.CreateStart().IsInCheck());
            Assert.True(Position.FromFen("4k3/8/8/8/8/8/8/4R2K b - - 0 1").IsInCheck());
            Assert.True(Position.FromFen("4k3/3P4/8/8/8/8/8/4K3 b - - 0 1").IsInCheck());
            // Blocked rook does not give check.
            Assert.False(Position.FromFen("4k3/4p3/8/8/8/8/8/4R2K b - - 0 1").IsInCheck());
        }

        [Fact]
        public void IsSquareAttacked_Pieces()
        {
            Position pos = Position.FromFen("4k3/8/8/3N4/8/8/8/R3K3 w - - 0 1");
            Assert.True(pos.IsSquareAttacked(Sq("e7"), Side.White));   // knight
            Assert.True(pos.IsSquareAttacked(Sq("a8"), Side.White));   // rook along file
            Assert.False(pos.IsSquareAttacked(Sq("d5"), Side.Black));
            Assert.True(pos.IsSquareAttacked(Sq("d7"), Side.Black));   // king
            Assert.False(pos.IsSquareAttacked(Sq("h8"), Side.White));
        }

        [Fact]
        public void IsSquareAttacked_PawnDirectionDependsOnSide()
        {
            Position pos = Position.FromFen("4k3/8/8/8/3p4/8/4P3/4K3 w - - 0 1");
            Assert.True(pos.IsSquareAttacked(Sq("d3"), Side.White));
            Assert.False(pos.IsSquareAttacked(Sq("d3"), Side.Black));
            Assert.False(pos.IsSquareAttacked(Sq("c3"), Side.White));
            Assert.True(pos.IsSquareAttacked(Sq("c3"), Side.Black));
            Assert.True(pos.IsSquareAttacked(Sq("e3"), Side.Black));
            Assert.False(pos.IsSquareAttacked(Sq("c5"), Side.Black));
        }

        [Fact]
        public void MakeMove_HalfmoveAndFullmoveCounters()
        {
            Position pos = Position.FromFen("4k3/8/8/8/8/8/4P3/4K1N1 w - - 5 10");
            Position afterKnight = pos.MakeMove(Find(pos, "g1f3"));
            Assert.Equal(6, afterKnight.HalfmoveClock);
            Assert.Equal(10, afterKnight.FullmoveNumber);
            Assert.Equal(Side.Black, afterKnight.SideToMove);

            Position afterKing = afterKnight.MakeMove(Find(afterKnight, "e8e7"));
            Assert.Equal(7, afterKing.HalfmoveClock);
            Assert.Equal(11, afterKing.FullmoveNumber);

            Position afterPawn = pos.MakeMove(Find(pos, "e2e3"));
            Assert.Equal(0, afterPawn.HalfmoveClock);
        }

        [Fact]
        public void MakeMove_Capture_ResetsHalfmoveClock()
        {
            Position pos = Position.FromFen("4k3/8/8/3p4/8/8/8/3RK3 w - - 7 20");
            Position next = pos.MakeMove(Find(pos, "d1d5"));
            Assert.Equal(0, next.HalfmoveClock);
            Assert.Equal(20, next.FullmoveNumber);
        }

        [Fact]
        public void MakeMove_DoesNotMutateOriginal()
        {
            Position pos = Position.CreateStart();
            pos.MakeMove(Find(pos, "e2e4"));
            Assert.Equal(Position.StartFen, pos.ToFen());
        }

        [Fact]
        public void GetLegalMoves_CheckmateAndStalemate_AreEmpty()
        {
            Assert.Empty(Position.FromFen("7k/6Q1/6K1/8/8/8/8/8 b - - 0 1").GetLegalMoves());
            Assert.Empty(Position.FromFen("k7/2Q5/1K6/8/8/8/8/8 b - - 0 1").GetLegalMoves());
        }

        [Fact]
        public void GetLegalMoves_InCheck_OnlyResolvingMoves()
        {
            // White king e1 checked by rook e8; only king moves (d1/d2/f1/f2) or none blocking.
            Position pos = Position.FromFen("4r1k1/8/8/8/8/8/8/4K3 w - - 0 1");
            var uci = pos.GetLegalMoves().Select(m => m.ToUci()).OrderBy(s => s).ToArray();
            Assert.Equal(new[] { "e1d1", "e1d2", "e1f1", "e1f2" }, uci);
        }

        [Fact]
        public void GetLegalMoves_CaptureFlagSet()
        {
            Position pos = Position.FromFen("4k3/8/8/3p4/8/8/8/3RK3 w - - 0 1");
            Assert.Equal(MoveFlags.Capture, Find(pos, "d1d5").Flags);
            Assert.Equal(MoveFlags.None, Find(pos, "d1d2").Flags);
        }
    }
}
