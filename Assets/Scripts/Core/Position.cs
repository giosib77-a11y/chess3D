using System;
using System.Globalization;
using System.Text;

namespace Chess.Core
{
    public sealed partial class Position
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        // State is shared with Position.Moves.cs (private members are visible across partial parts).
        private readonly Piece[] _board = new Piece[64];
        private Side _sideToMove;
        private CastlingRights _castlingRights;
        private Square? _enPassantSquare;
        private int _halfmoveClock;
        private int _fullmoveNumber = 1;

        private Position()
        {
        }

        public Side SideToMove => _sideToMove;
        public CastlingRights CastlingRights => _castlingRights;
        public Square? EnPassantSquare => _enPassantSquare;
        public int HalfmoveClock => _halfmoveClock;
        public int FullmoveNumber => _fullmoveNumber;

        public static Position CreateStart() => FromFen(StartFen);

        public Piece GetPiece(Square square) => _board[square.Index];

        public Square FindKing(Side side)
        {
            for (int i = 0; i < 64; i++)
            {
                Piece p = _board[i];
                if (p.Type == PieceType.King && p.Side == side)
                    return new Square(i);
            }
            throw new InvalidOperationException("No king found for " + side + ".");
        }

        public Position Clone()
        {
            var copy = new Position();
            Array.Copy(_board, copy._board, 64);
            copy._sideToMove = _sideToMove;
            copy._castlingRights = _castlingRights;
            copy._enPassantSquare = _enPassantSquare;
            copy._halfmoveClock = _halfmoveClock;
            copy._fullmoveNumber = _fullmoveNumber;
            return copy;
        }

        public static Position FromFen(string fen)
        {
            if (fen == null)
                throw new ArgumentNullException(nameof(fen));

            string[] fields = fen.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 6)
                throw Invalid("expected 6 fields but found " + fields.Length, fen);

            var pos = new Position();
            ParsePlacement(pos, fields[0], fen);

            if (fields[1] == "w") pos._sideToMove = Side.White;
            else if (fields[1] == "b") pos._sideToMove = Side.Black;
            else throw Invalid("side to move must be 'w' or 'b'", fen);

            pos._castlingRights = ParseCastling(pos, fields[2], fen);
            pos._enPassantSquare = ParseEnPassant(pos, pos._sideToMove, fields[3], fen);

            if (!int.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int half) || half < 0)
                throw Invalid("halfmove clock must be a non-negative integer", fen);
            if (!int.TryParse(fields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out int full) || full < 1)
                throw Invalid("fullmove number must be an integer >= 1", fen);
            pos._halfmoveClock = half;
            pos._fullmoveNumber = full;
            return pos;
        }

        public string ToFen()
        {
            var sb = new StringBuilder(80);
            for (int rank = 7; rank >= 0; rank--)
            {
                int empty = 0;
                for (int file = 0; file < 8; file++)
                {
                    Piece p = _board[rank * 8 + file];
                    if (p.IsNone)
                    {
                        empty++;
                        continue;
                    }
                    if (empty > 0)
                    {
                        sb.Append((char)('0' + empty));
                        empty = 0;
                    }
                    sb.Append(p.ToFenChar());
                }
                if (empty > 0) sb.Append((char)('0' + empty));
                if (rank > 0) sb.Append('/');
            }

            sb.Append(' ').Append(_sideToMove == Side.White ? 'w' : 'b').Append(' ');

            if (_castlingRights == CastlingRights.None)
            {
                sb.Append('-');
            }
            else
            {
                if ((_castlingRights & CastlingRights.WhiteKingside) != 0) sb.Append('K');
                if ((_castlingRights & CastlingRights.WhiteQueenside) != 0) sb.Append('Q');
                if ((_castlingRights & CastlingRights.BlackKingside) != 0) sb.Append('k');
                if ((_castlingRights & CastlingRights.BlackQueenside) != 0) sb.Append('q');
            }

            sb.Append(' ').Append(_enPassantSquare.HasValue ? _enPassantSquare.Value.ToString() : "-");
            sb.Append(' ').Append(_halfmoveClock.ToString(CultureInfo.InvariantCulture));
            sb.Append(' ').Append(_fullmoveNumber.ToString(CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        private static FormatException Invalid(string reason, string fen)
        {
            return new FormatException("Invalid FEN: " + reason + ". FEN: '" + fen + "'");
        }

        private static void ParsePlacement(Position pos, string placement, string fen)
        {
            string[] ranks = placement.Split('/');
            if (ranks.Length != 8)
                throw Invalid("expected 8 ranks but found " + ranks.Length, fen);

            int whiteKings = 0, blackKings = 0;
            for (int r = 0; r < 8; r++)
            {
                int rank = 7 - r;
                int file = 0;
                foreach (char c in ranks[r])
                {
                    if (c >= '1' && c <= '8')
                    {
                        file += c - '0';
                        if (file > 8)
                            throw Invalid("rank " + (rank + 1) + " has more than 8 squares", fen);
                        continue;
                    }
                    if (!Piece.TryFromFenChar(c, out Piece piece))
                        throw Invalid("unknown character '" + c + "'", fen);
                    if (file >= 8)
                        throw Invalid("rank " + (rank + 1) + " has more than 8 squares", fen);
                    if (piece.Type == PieceType.Pawn && (rank == 0 || rank == 7))
                        throw Invalid("pawn on rank " + (rank + 1), fen);
                    if (piece.Type == PieceType.King)
                    {
                        if (piece.Side == Side.White) whiteKings++; else blackKings++;
                    }
                    pos._board[rank * 8 + file] = piece;
                    file++;
                }
                if (file != 8)
                    throw Invalid("rank " + (rank + 1) + " has " + file + " squares instead of 8", fen);
            }

            if (whiteKings != 1 || blackKings != 1)
                throw Invalid("each side must have exactly one king", fen);
        }

        private static CastlingRights ParseCastling(Position pos, string text, string fen)
        {
            if (text == "-")
                return CastlingRights.None;

            CastlingRights rights = CastlingRights.None;
            foreach (char c in text)
            {
                CastlingRights flag;
                Side side;
                int kingSq, rookSq;
                switch (c)
                {
                    case 'K': flag = CastlingRights.WhiteKingside; side = Side.White; kingSq = 4; rookSq = 7; break;
                    case 'Q': flag = CastlingRights.WhiteQueenside; side = Side.White; kingSq = 4; rookSq = 0; break;
                    case 'k': flag = CastlingRights.BlackKingside; side = Side.Black; kingSq = 60; rookSq = 63; break;
                    case 'q': flag = CastlingRights.BlackQueenside; side = Side.Black; kingSq = 60; rookSq = 56; break;
                    default: throw Invalid("invalid castling character '" + c + "'", fen);
                }
                if ((rights & flag) != 0)
                    throw Invalid("duplicate castling right '" + c + "'", fen);
                if (pos._board[kingSq] != new Piece(PieceType.King, side)
                    || pos._board[rookSq] != new Piece(PieceType.Rook, side))
                    throw Invalid("castling right '" + c + "' requires king and rook on their home squares", fen);
                rights |= flag;
            }
            return rights;
        }

        private static Square? ParseEnPassant(Position pos, Side sideToMove, string text, string fen)
        {
            if (text == "-")
                return null;
            if (!Square.TryParse(text, out Square sq))
                throw Invalid("invalid en passant square '" + text + "'", fen);
            int expectedRank = sideToMove == Side.White ? 5 : 2;
            if (sq.Rank != expectedRank)
                throw Invalid("en passant square '" + text + "' is on the wrong rank", fen);

            // The opponent just double-pushed: ep square and the square behind it are empty,
            // and the pushed pawn stands in front of the ep square.
            int step = sideToMove == Side.White ? 8 : -8;
            int behind = sq.Index + step;
            int front = sq.Index - step;
            Side pusher = sideToMove == Side.White ? Side.Black : Side.White;
            if (!pos._board[sq.Index].IsNone
                || !pos._board[behind].IsNone
                || pos._board[front] != new Piece(PieceType.Pawn, pusher))
                throw Invalid("en passant square '" + text + "' is inconsistent with the position", fen);
            return sq;
        }
    }
}
