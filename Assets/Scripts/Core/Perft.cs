using System;
using System.Collections.Generic;

namespace Chess.Core
{
    public static class Perft
    {
        public static long Count(Position position, int depth)
        {
            Validate(position, depth);
            return CountInternal(position, depth);
        }

        public static IReadOnlyDictionary<string, long> Divide(Position position, int depth)
        {
            Validate(position, depth);
            var result = new Dictionary<string, long>();
            if (depth == 0)
                return result;

            IReadOnlyList<Move> moves = position.GetLegalMoves();
            for (int i = 0; i < moves.Count; i++)
                result[moves[i].ToUci()] = CountInternal(position.MakeMove(moves[i]), depth - 1);
            return result;
        }

        private static void Validate(Position position, int depth)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            if (depth < 0)
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "Depth must be >= 0.");
        }

        private static long CountInternal(Position position, int depth)
        {
            if (depth == 0)
                return 1;

            IReadOnlyList<Move> moves = position.GetLegalMoves();
            if (depth == 1)
                return moves.Count; // bulk counting

            long total = 0;
            for (int i = 0; i < moves.Count; i++)
                total += CountInternal(position.MakeMove(moves[i]), depth - 1);
            return total;
        }
    }
}
