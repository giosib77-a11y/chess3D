using System;

namespace Chess.Core
{
    public readonly struct Square : IEquatable<Square>
    {
        private readonly byte _index;

        public Square(int index)
        {
            if (index < 0 || index > 63)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Square index must be 0..63.");
            _index = (byte)index;
        }

        public static Square FromFileRank(int file, int rank)
        {
            if (file < 0 || file > 7)
                throw new ArgumentOutOfRangeException(nameof(file), file, "File must be 0..7.");
            if (rank < 0 || rank > 7)
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Rank must be 0..7.");
            return new Square(rank * 8 + file);
        }

        public int Index => _index;
        public int File => _index % 8;
        public int Rank => _index / 8;

        public static Square Parse(string name)
        {
            if (!TryParse(name, out Square square))
                throw new FormatException("Invalid square: '" + name + "'");
            return square;
        }

        public static bool TryParse(string name, out Square square)
        {
            if (name != null && name.Length == 2
                && name[0] >= 'a' && name[0] <= 'h'
                && name[1] >= '1' && name[1] <= '8')
            {
                square = new Square((name[1] - '1') * 8 + (name[0] - 'a'));
                return true;
            }
            square = default;
            return false;
        }

        public override string ToString()
        {
            return new string(new[] { (char)('a' + File), (char)('1' + Rank) });
        }

        public bool Equals(Square other) => _index == other._index;
        public override bool Equals(object obj) => obj is Square other && Equals(other);
        public override int GetHashCode() => _index;

        public static bool operator ==(Square left, Square right) => left._index == right._index;
        public static bool operator !=(Square left, Square right) => left._index != right._index;
    }
}
