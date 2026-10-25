using System;

namespace Chess.Core
{
    public enum PieceType : byte { None = 0, Pawn = 1, Knight = 2, Bishop = 3, Rook = 4, Queen = 5, King = 6 }

    public readonly struct Piece : IEquatable<Piece>
    {
        public static readonly Piece None = default;

        private readonly PieceType _type;
        private readonly Side _side;

        public Piece(PieceType type, Side side)
        {
            if (type == PieceType.None)
                throw new ArgumentException("Piece type must not be None.", nameof(type));
            if (type < PieceType.Pawn || type > PieceType.King)
                throw new ArgumentOutOfRangeException(nameof(type));
            if (side != Side.White && side != Side.Black)
                throw new ArgumentOutOfRangeException(nameof(side));
            _type = type;
            _side = side;
        }

        public PieceType Type => _type;
        public Side Side => _side;
        public bool IsNone => _type == PieceType.None;

        public char ToFenChar()
        {
            if (IsNone)
                throw new InvalidOperationException("Piece.None has no FEN character.");
            char c;
            switch (_type)
            {
                case PieceType.Pawn: c = 'p'; break;
                case PieceType.Knight: c = 'n'; break;
                case PieceType.Bishop: c = 'b'; break;
                case PieceType.Rook: c = 'r'; break;
                case PieceType.Queen: c = 'q'; break;
                default: c = 'k'; break;
            }
            return _side == Side.White ? char.ToUpperInvariant(c) : c;
        }

        public static bool TryFromFenChar(char c, out Piece piece)
        {
            PieceType type;
            switch (char.ToLowerInvariant(c))
            {
                case 'p': type = PieceType.Pawn; break;
                case 'n': type = PieceType.Knight; break;
                case 'b': type = PieceType.Bishop; break;
                case 'r': type = PieceType.Rook; break;
                case 'q': type = PieceType.Queen; break;
                case 'k': type = PieceType.King; break;
                default:
                    piece = default;
                    return false;
            }
            piece = new Piece(type, char.IsUpper(c) ? Side.White : Side.Black);
            return true;
        }

        public bool Equals(Piece other)
        {
            if (IsNone) return other.IsNone;
            return _type == other._type && _side == other._side;
        }

        public override bool Equals(object obj) => obj is Piece other && Equals(other);

        public override int GetHashCode() => IsNone ? 0 : ((int)_type << 1) | (int)_side;

        public static bool operator ==(Piece left, Piece right) => left.Equals(right);
        public static bool operator !=(Piece left, Piece right) => !left.Equals(right);
    }
}
