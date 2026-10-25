using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ChessCore.Tests")]

namespace Chess.Core
{
    public sealed partial class Position
    {
        private static readonly int[] KnightDf = { 1, 2, 2, 1, -1, -2, -2, -1 };
        private static readonly int[] KnightDr = { 2, 1, -1, -2, -2, -1, 1, 2 };
        private static readonly int[] KingDf = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] KingDr = { -1, -1, -1, 0, 0, 1, 1, 1 };
        private static readonly int[] DiagDf = { 1, 1, -1, -1 };
        private static readonly int[] DiagDr = { 1, -1, 1, -1 };
        private static readonly int[] OrthoDf = { 1, -1, 0, 0 };
        private static readonly int[] OrthoDr = { 0, 0, 1, -1 };
        private static readonly PieceType[] PromotionTypes =
            { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };

        public IReadOnlyList<Move> GetLegalMoves()
        {
            var pseudo = new List<Move>(64);
            GeneratePseudoLegal(pseudo);

            var legal = new List<Move>(pseudo.Count);
            Side us = _sideToMove;
            for (int i = 0; i < pseudo.Count; i++)
            {
                Position next = MakeMove(pseudo[i]);
                if (!next.IsSquareAttacked(next.FindKing(us), us.Opposite()))
                    legal.Add(pseudo[i]);
            }
            return legal;
        }

        public bool IsInCheck()
        {
            return IsSquareAttacked(FindKing(_sideToMove), _sideToMove.Opposite());
        }

        public bool IsSquareAttacked(Square square, Side bySide)
        {
            int file = square.File, rank = square.Rank;

            // Pawns: a pawn of bySide attacks diagonally forward, so it sits one rank behind the target.
            int pawnRank = bySide == Side.White ? rank - 1 : rank + 1;
            if (pawnRank >= 0 && pawnRank <= 7)
            {
                for (int df = -1; df <= 1; df += 2)
                {
                    int f = file + df;
                    if (f >= 0 && f <= 7 && IsPiece(f, pawnRank, PieceType.Pawn, bySide))
                        return true;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                if (IsPiece(file + KnightDf[i], rank + KnightDr[i], PieceType.Knight, bySide)) return true;
                if (IsPiece(file + KingDf[i], rank + KingDr[i], PieceType.King, bySide)) return true;
            }

            return RayHits(file, rank, DiagDf, DiagDr, PieceType.Bishop, bySide)
                || RayHits(file, rank, OrthoDf, OrthoDr, PieceType.Rook, bySide);
        }

        // Copy-make; no legality check. Behaviour is derived from the board, not from move flags.
        internal Position MakeMove(Move legalMove)
        {
            Position p = Clone();
            int from = legalMove.From.Index, to = legalMove.To.Index;
            Piece piece = _board[from];
            Piece target = _board[to];
            Side us = _sideToMove;
            bool isPawn = piece.Type == PieceType.Pawn;
            bool capture = !target.IsNone;

            p._board[from] = Piece.None;
            p._enPassantSquare = null;

            if (isPawn && !capture && legalMove.From.File != legalMove.To.File)
            {
                // En passant: the captured pawn is behind the target square.
                p._board[us == Side.White ? to - 8 : to + 8] = Piece.None;
                capture = true;
            }

            if (piece.Type == PieceType.King && (to - from == 2 || from - to == 2))
            {
                bool kingside = to > from;
                int rookFrom = kingside ? from + 3 : from - 4;
                int rookTo = kingside ? from + 1 : from - 1;
                p._board[rookTo] = p._board[rookFrom];
                p._board[rookFrom] = Piece.None;
            }

            p._board[to] = legalMove.IsPromotion ? new Piece(legalMove.Promotion, us) : piece;

            if (isPawn && (to - from == 16 || from - to == 16))
                p._enPassantSquare = new Square((from + to) / 2);

            CastlingRights rights = _castlingRights;
            if (piece.Type == PieceType.King)
            {
                rights &= us == Side.White
                    ? ~(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside)
                    : ~(CastlingRights.BlackKingside | CastlingRights.BlackQueenside);
            }
            rights &= ~RightsLostOn(from);
            rights &= ~RightsLostOn(to);
            p._castlingRights = rights;

            p._halfmoveClock = (isPawn || capture) ? 0 : _halfmoveClock + 1;
            if (us == Side.Black)
                p._fullmoveNumber = _fullmoveNumber + 1;
            p._sideToMove = us.Opposite();
            return p;
        }

        private static CastlingRights RightsLostOn(int index)
        {
            switch (index)
            {
                case 0: return CastlingRights.WhiteQueenside;
                case 7: return CastlingRights.WhiteKingside;
                case 56: return CastlingRights.BlackQueenside;
                case 63: return CastlingRights.BlackKingside;
                default: return CastlingRights.None;
            }
        }

        private bool IsPiece(int file, int rank, PieceType type, Side side)
        {
            if (file < 0 || file > 7 || rank < 0 || rank > 7)
                return false;
            Piece p = _board[rank * 8 + file];
            return p.Type == type && p.Side == side;
        }

        // Walks each ray from (file, rank); true if the first piece met is a slider (or queen) of bySide.
        private bool RayHits(int file, int rank, int[] dfs, int[] drs, PieceType slider, Side bySide)
        {
            for (int d = 0; d < dfs.Length; d++)
            {
                int f = file + dfs[d], r = rank + drs[d];
                while (f >= 0 && f <= 7 && r >= 0 && r <= 7)
                {
                    Piece p = _board[r * 8 + f];
                    if (!p.IsNone)
                    {
                        if (p.Side == bySide && (p.Type == slider || p.Type == PieceType.Queen))
                            return true;
                        break;
                    }
                    f += dfs[d];
                    r += drs[d];
                }
            }
            return false;
        }

        private void GeneratePseudoLegal(List<Move> moves)
        {
            Side us = _sideToMove;
            for (int i = 0; i < 64; i++)
            {
                Piece piece = _board[i];
                if (piece.IsNone || piece.Side != us)
                    continue;

                int file = i % 8, rank = i / 8;
                switch (piece.Type)
                {
                    case PieceType.Pawn: GeneratePawn(moves, i, file, rank, us); break;
                    case PieceType.Knight: GenerateSteps(moves, i, file, rank, us, KnightDf, KnightDr); break;
                    case PieceType.King:
                        GenerateSteps(moves, i, file, rank, us, KingDf, KingDr);
                        GenerateCastling(moves, i, us);
                        break;
                    case PieceType.Bishop: GenerateSlides(moves, i, file, rank, us, DiagDf, DiagDr); break;
                    case PieceType.Rook: GenerateSlides(moves, i, file, rank, us, OrthoDf, OrthoDr); break;
                    default:
                        GenerateSlides(moves, i, file, rank, us, DiagDf, DiagDr);
                        GenerateSlides(moves, i, file, rank, us, OrthoDf, OrthoDr);
                        break;
                }
            }
        }

        private void GenerateSteps(List<Move> moves, int from, int file, int rank, Side us, int[] dfs, int[] drs)
        {
            for (int d = 0; d < dfs.Length; d++)
            {
                int f = file + dfs[d], r = rank + drs[d];
                if (f < 0 || f > 7 || r < 0 || r > 7)
                    continue;
                Piece target = _board[r * 8 + f];
                if (target.IsNone)
                    moves.Add(new Move(new Square(from), new Square(r * 8 + f)));
                else if (target.Side != us)
                    moves.Add(new Move(new Square(from), new Square(r * 8 + f), PieceType.None, MoveFlags.Capture));
            }
        }

        private void GenerateSlides(List<Move> moves, int from, int file, int rank, Side us, int[] dfs, int[] drs)
        {
            for (int d = 0; d < dfs.Length; d++)
            {
                int f = file + dfs[d], r = rank + drs[d];
                while (f >= 0 && f <= 7 && r >= 0 && r <= 7)
                {
                    Piece target = _board[r * 8 + f];
                    if (target.IsNone)
                    {
                        moves.Add(new Move(new Square(from), new Square(r * 8 + f)));
                    }
                    else
                    {
                        if (target.Side != us)
                            moves.Add(new Move(new Square(from), new Square(r * 8 + f), PieceType.None, MoveFlags.Capture));
                        break;
                    }
                    f += dfs[d];
                    r += drs[d];
                }
            }
        }

        private void GeneratePawn(List<Move> moves, int from, int file, int rank, Side us)
        {
            int dir = us == Side.White ? 1 : -1;
            int startRank = us == Side.White ? 1 : 6;
            int lastRank = us == Side.White ? 7 : 0;
            int nr = rank + dir;
            Square fromSq = new Square(from);

            if (_board[nr * 8 + file].IsNone)
            {
                AddPawnMove(moves, fromSq, new Square(nr * 8 + file), nr == lastRank, MoveFlags.None);
                if (rank == startRank && _board[(nr + dir) * 8 + file].IsNone)
                    moves.Add(new Move(fromSq, new Square((nr + dir) * 8 + file), PieceType.None, MoveFlags.DoublePawnPush));
            }

            for (int df = -1; df <= 1; df += 2)
            {
                int f = file + df;
                if (f < 0 || f > 7)
                    continue;
                int to = nr * 8 + f;
                Piece target = _board[to];
                if (!target.IsNone && target.Side != us)
                    AddPawnMove(moves, fromSq, new Square(to), nr == lastRank, MoveFlags.Capture);
                else if (target.IsNone && _enPassantSquare.HasValue && _enPassantSquare.Value.Index == to)
                    moves.Add(new Move(fromSq, new Square(to), PieceType.None, MoveFlags.EnPassant));
            }
        }

        private static void AddPawnMove(List<Move> moves, Square from, Square to, bool promotes, MoveFlags flags)
        {
            if (!promotes)
            {
                moves.Add(new Move(from, to, PieceType.None, flags));
                return;
            }
            for (int i = 0; i < PromotionTypes.Length; i++)
                moves.Add(new Move(from, to, PromotionTypes[i], flags | MoveFlags.Promotion));
        }

        private void GenerateCastling(List<Move> moves, int kingIndex, Side us)
        {
            bool white = us == Side.White;
            int home = white ? 4 : 60;
            if (kingIndex != home)
                return;

            CastlingRights kingsideRight = white ? CastlingRights.WhiteKingside : CastlingRights.BlackKingside;
            CastlingRights queensideRight = white ? CastlingRights.WhiteQueenside : CastlingRights.BlackQueenside;
            if ((_castlingRights & (kingsideRight | queensideRight)) == 0)
                return;

            Side them = us.Opposite();
            if (IsSquareAttacked(new Square(home), them))
                return;

            if ((_castlingRights & kingsideRight) != 0
                && _board[home + 1].IsNone && _board[home + 2].IsNone
                && !IsSquareAttacked(new Square(home + 1), them)
                && !IsSquareAttacked(new Square(home + 2), them))
            {
                moves.Add(new Move(new Square(home), new Square(home + 2), PieceType.None, MoveFlags.CastleKingside));
            }

            if ((_castlingRights & queensideRight) != 0
                && _board[home - 1].IsNone && _board[home - 2].IsNone && _board[home - 3].IsNone
                && !IsSquareAttacked(new Square(home - 1), them)
                && !IsSquareAttacked(new Square(home - 2), them))
            {
                moves.Add(new Move(new Square(home), new Square(home - 2), PieceType.None, MoveFlags.CastleQueenside));
            }
        }
    }
}
