using System;

namespace Chess.Core
{
    [Flags]
    public enum CastlingRights : byte
    {
        None = 0,
        WhiteKingside = 1,
        WhiteQueenside = 2,
        BlackKingside = 4,
        BlackQueenside = 8,
        All = 15
    }
}
