using System;

namespace Chess.Core
{
    [Flags]
    public enum MoveFlags : byte
    {
        None = 0,
        Capture = 1,
        DoublePawnPush = 2,
        EnPassant = 4,
        CastleKingside = 8,
        CastleQueenside = 16,
        Promotion = 32
    }

    public readonly struct Move : IEquatable<Move>
    {
        private readonly Square _from;
        private readonly Square _to;
        private readonly PieceType _promotion;
        private readonly MoveFlags _flags;

        public Move(Square from, Square to, PieceType promotion = PieceType.None, MoveFlags flags = MoveFlags.None)
        {
            if (promotion != PieceType.None && promotion != PieceType.Knight && promotion != PieceType.Bishop
                && promotion != PieceType.Rook && promotion != PieceType.Queen)
                throw new ArgumentException("Invalid promotion piece: " + promotion, nameof(promotion));
            _from = from;
            _to = to;
            _promotion = promotion;
            _flags = flags;
        }

        public Square From => _from;
        public Square To => _to;
        public PieceType Promotion => _promotion;
        public MoveFlags Flags => _flags;
        public bool IsPromotion => _promotion != PieceType.None;
        public bool IsCapture => (_flags & (MoveFlags.Capture | MoveFlags.EnPassant)) != 0;
        public bool IsCastle => (_flags & (MoveFlags.CastleKingside | MoveFlags.CastleQueenside)) != 0;

        public string ToUci()
        {
            string s = _from.ToString() + _to.ToString();
            switch (_promotion)
            {
                case PieceType.Knight: return s + "n";
                case PieceType.Bishop: return s + "b";
                case PieceType.Rook: return s + "r";
                case PieceType.Queen: return s + "q";
                default: return s;
            }
        }

        public static bool TryParseUci(string uci, out Move move)
        {
            move = default;
            if (uci == null || (uci.Length != 4 && uci.Length != 5))
                return false;
            if (!Square.TryParse(uci.Substring(0, 2), out Square from)
                || !Square.TryParse(uci.Substring(2, 2), out Square to))
                return false;

            PieceType promotion = PieceType.None;
            if (uci.Length == 5)
            {
                switch (uci[4])
                {
                    case 'n': promotion = PieceType.Knight; break;
                    case 'b': promotion = PieceType.Bishop; break;
                    case 'r': promotion = PieceType.Rook; break;
                    case 'q': promotion = PieceType.Queen; break;
                    default: return false;
                }
            }
            move = new Move(from, to, promotion);
            return true;
        }

        // Flags are intentionally ignored so a parsed UCI move equals the generated legal move.
        public bool Equals(Move other) => _from == other._from && _to == other._to && _promotion == other._promotion;
        public override bool Equals(object obj) => obj is Move other && Equals(other);

        public override int GetHashCode()
        {
            return (_from.Index << 9) | (_to.Index << 3) | (int)_promotion;
        }

        public override string ToString() => ToUci();

        public static bool operator ==(Move left, Move right) => left.Equals(right);
        public static bool operator !=(Move left, Move right) => !left.Equals(right);
    }
}
