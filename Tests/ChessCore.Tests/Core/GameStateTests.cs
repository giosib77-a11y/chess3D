using System;
using System.Linq;
using Chess.Core;
using Xunit;

namespace ChessCore.Tests.Core
{
    public class GameStateTests
    {
        private static void Play(GameState game, string moves)
        {
            foreach (string uci in moves.Split(' '))
                Assert.True(game.TryMakeUciMove(uci), "move rejected: " + uci);
        }

        [Fact]
        public void NewGame_IsStartPositionInProgress()
        {
            var game = new GameState();
            Assert.Equal(Position.StartFen, game.ToFen());
            Assert.Equal(Side.White, game.SideToMove);
            Assert.Equal(20, game.LegalMoves.Count);
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.Equal(GameResult.None, game.Result);
            Assert.False(game.IsCheck);
            Assert.False(game.IsGameOver);
            Assert.Null(game.LastMove);
            Assert.Empty(game.MoveHistory);
        }

        [Fact]
        public void FoolsMate_IsCheckmateBlackWins()
        {
            var game = new GameState();
            Play(game, "f2f3 e7e5 g2g4 d8h4");
            Assert.Equal(GameStatus.Checkmate, game.Status);
            Assert.Equal(GameResult.BlackWins, game.Result);
            Assert.True(game.IsCheck);
            Assert.True(game.IsGameOver);
            Assert.Empty(game.LegalMoves);
        }

        [Fact]
        public void Stalemate_IsDraw()
        {
            var game = new GameState("7k/8/8/5Q2/8/8/8/K7 w - - 0 1");
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.True(game.TryMakeUciMove("f5f7"));
            Assert.Equal(GameStatus.Stalemate, game.Status);
            Assert.Equal(GameResult.Draw, game.Result);
            Assert.False(game.IsCheck);
            Assert.True(game.IsGameOver);
        }

        [Fact]
        public void FiftyMoveRule_TriggersAtHalfmove100()
        {
            var game = new GameState("4k3/8/8/8/8/8/8/R3K3 w - - 98 80");
            Assert.True(game.TryMakeUciMove("a1a2"));
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.True(game.TryMakeUciMove("e8d8"));
            Assert.Equal(100, game.Position.HalfmoveClock);
            Assert.Equal(GameStatus.FiftyMoveRule, game.Status);
            Assert.Equal(GameResult.Draw, game.Result);
        }

        [Fact]
        public void FiftyMoveRule_FromFenWithHalfmove99_AndQuietMove()
        {
            var game = new GameState("4k3/8/8/8/8/8/8/R3K3 w - - 99 80");
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.True(game.TryMakeUciMove("a1a2"));
            Assert.Equal(GameStatus.FiftyMoveRule, game.Status);
        }

        [Fact]
        public void MateOnHalfmove100_BeatsFiftyMoveRule()
        {
            var game = new GameState("6k1/5ppp/8/8/8/8/8/R3K3 w - - 99 80");
            Assert.True(game.TryMakeUciMove("a1a8"));
            Assert.Equal(100, game.Position.HalfmoveClock);
            Assert.Equal(GameStatus.Checkmate, game.Status);
            Assert.Equal(GameResult.WhiteWins, game.Result);
        }

        [Fact]
        public void ThreefoldRepetition_AfterEighthHalfMove_NotAfterFourth()
        {
            var game = new GameState();
            Play(game, "g1f3 g8f6 f3g1 f6g8");
            Assert.Equal(GameStatus.InProgress, game.Status);
            Play(game, "g1f3 g8f6 f3g1");
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.True(game.TryMakeUciMove("f6g8"));
            Assert.Equal(GameStatus.ThreefoldRepetition, game.Status);
            Assert.Equal(GameResult.Draw, game.Result);
            Assert.Empty(game.LegalMoves);
        }

        [Fact]
        public void UnusableEnPassantSquare_DoesNotChangeRepetitionKey()
        {
            // After e2e4 the FEN has ep square e3, but no black pawn can capture it.
            var game = new GameState();
            Play(game, "e2e4");
            Assert.Contains(" e3 ", game.ToFen());

            Play(game, "g8f6 g1f3 f6g8 f3g1"); // same position again, FEN now has no ep square
            Assert.Null(game.Position.EnPassantSquare);
            Assert.Equal(GameStatus.InProgress, game.Status);

            Play(game, "g8f6 g1f3 f6g8");
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.True(game.TryMakeUciMove("f3g1"));
            Assert.Equal(GameStatus.ThreefoldRepetition, game.Status);
        }

        [Fact]
        public void UsableEnPassantSquare_DistinguishesPositions()
        {
            // Black pawn on d4 can capture e3 after e2e4, so that position differs from the same one without the right.
            var game = new GameState("4k3/8/8/8/3p4/8/4P3/4K3 w - - 0 1");
            Play(game, "e2e4 e8e7 e1e2 e7e8 e2e1 e8e7"); // none of these repeat the ep position
            Assert.Equal(GameStatus.InProgress, game.Status);
        }

        [Fact]
        public void IllegalMove_ReturnsFalseAndStateUnchanged()
        {
            var game = new GameState();
            string before = game.ToFen();
            Assert.False(game.TryMakeUciMove("e2e5"));
            Assert.False(game.TryMakeUciMove("e7e5")); // not this side's piece
            Assert.False(game.TryMakeUciMove("zz"));
            Assert.False(game.TryMakeUciMove(null));
            Assert.False(game.TryMakeMove(default));
            Assert.Equal(before, game.ToFen());
            Assert.Empty(game.MoveHistory);
            Assert.Equal(20, game.LegalMoves.Count);
        }

        [Fact]
        public void AfterGameOver_AnyMoveIsRejected()
        {
            var game = new GameState();
            Play(game, "f2f3 e7e5 g2g4 d8h4");
            string before = game.ToFen();
            Assert.False(game.TryMakeUciMove("a2a3"));
            Assert.False(game.TryResolveUciMove("a2a3", out _));
            Assert.Empty(game.LegalMoves);
            Assert.Empty(game.GetLegalMovesFrom(Square.Parse("a2")));
            Assert.Equal(before, game.ToFen());
        }

        [Fact]
        public void PromotionWithoutPromotionPiece_IsRejected()
        {
            var game = new GameState("8/4P2k/8/8/8/8/8/K7 w - - 0 1");
            string before = game.ToFen();
            Assert.False(game.TryMakeUciMove("e7e8"));
            Assert.False(game.TryMakeMove(new Move(Square.Parse("e7"), Square.Parse("e8"))));
            Assert.Equal(before, game.ToFen());
        }

        [Fact]
        public void PromotionWithPiece_Works()
        {
            var game = new GameState("8/4P2k/8/8/8/8/8/K7 w - - 0 1");
            Assert.True(game.TryMakeUciMove("e7e8q"));
            Assert.Equal(new Piece(PieceType.Queen, Side.White), game.Position.GetPiece(Square.Parse("e8")));
            Assert.True(game.LastMove.Value.IsPromotion);
            Assert.False(game.TryMakeUciMove("e8e9"));
        }

        [Fact]
        public void ResolveUci_ReturnsFlags()
        {
            var castle = new GameState("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            Assert.True(castle.TryResolveUciMove("e1g1", out Move ks));
            Assert.Equal(MoveFlags.CastleKingside, ks.Flags);
            Assert.True(castle.TryResolveUciMove("e1c1", out Move qs));
            Assert.Equal(MoveFlags.CastleQueenside, qs.Flags);

            var ep = new GameState("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1");
            Assert.True(ep.TryResolveUciMove("e5d6", out Move epMove));
            Assert.Equal(MoveFlags.EnPassant, epMove.Flags);

            Assert.False(castle.TryResolveUciMove("e1e5", out _));
            Assert.False(castle.TryResolveUciMove("garbage", out _));
        }

        [Fact]
        public void MoveHistoryAndLastMove_ContainFlags()
        {
            var game = new GameState("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            Play(game, "e1g1 e8c8");
            Assert.Equal(2, game.MoveHistory.Count);
            Assert.Equal(MoveFlags.CastleKingside, game.MoveHistory[0].Flags);
            Assert.Equal(MoveFlags.CastleQueenside, game.LastMove.Value.Flags);
            Assert.Equal("e8c8", game.LastMove.Value.ToUci());
        }

        [Fact]
        public void TryMakeMove_AcceptsFlaglessMove()
        {
            var game = new GameState();
            Assert.True(game.TryMakeMove(new Move(Square.Parse("e2"), Square.Parse("e4"))));
            Assert.Equal(MoveFlags.DoublePawnPush, game.LastMove.Value.Flags);
        }

        [Fact]
        public void GetLegalMovesFrom_ReturnsOnlyThatSquare()
        {
            var game = new GameState();
            var fromG1 = game.GetLegalMovesFrom(Square.Parse("g1"));
            Assert.Equal(2, fromG1.Count);
            Assert.All(fromG1, m => Assert.Equal(Square.Parse("g1"), m.From));
            Assert.Empty(game.GetLegalMovesFrom(Square.Parse("e5")));
            Assert.Empty(game.GetLegalMovesFrom(Square.Parse("g8"))); // opponent's piece
        }

        [Fact]
        public void IsCheck_TrueWhenInCheck()
        {
            var game = new GameState("4k3/8/8/8/8/8/8/4K2R w - - 0 1");
            Assert.True(game.TryMakeUciMove("h1h8"));
            Assert.True(game.IsCheck);
            Assert.Equal(GameStatus.InProgress, game.Status);
        }

        [Fact]
        public void Fen_SideNotToMoveInCheck_ThrowsFormatException()
        {
            const string fen = "4k3/8/8/8/8/8/4R3/4K3 w - - 0 1";
            var ex = Assert.Throws<FormatException>(() => new GameState(fen));
            Assert.Contains("side not to move is in check", ex.Message);
            Assert.StartsWith("Invalid FEN: ", ex.Message);
            Assert.EndsWith("FEN: '" + fen + "'", ex.Message);
        }

        [Fact]
        public void Fen_NullAndMalformed_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new GameState(null));
            Assert.Throws<FormatException>(() => new GameState("not a fen"));
            Assert.Throws<FormatException>(() => new GameState("8/8/8/8/8/8/8/8 w - - 0 1"));
        }

        [Fact]
        public void Fen_CheckmatePosition_IsDetectedOnConstruction()
        {
            var game = new GameState("rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3");
            Assert.Equal(GameStatus.Checkmate, game.Status);
            Assert.Equal(GameResult.BlackWins, game.Result);
        }
    }
}
