using System.Collections.Generic;

namespace Chess.Core
{
    internal static class DrawRules
    {
        // Automatic draw only for: K v K, K+B v K, K+N v K, and K+B(s) v K+B(s) with all bishops on one square color.
        public static bool IsInsufficientMaterial(Position position)
        {
            int knights = 0, bishops = 0;
            int lightBishops = 0, darkBishops = 0;
            for (int i = 0; i < 64; i++)
            {
                Piece p = position.GetPiece(new Square(i));
                switch (p.Type)
                {
                    case PieceType.None:
                    case PieceType.King:
                        break;
                    case PieceType.Knight:
                        knights++;
                        break;
                    case PieceType.Bishop:
                        bishops++;
                        if (((i % 8) + (i / 8)) % 2 == 0) darkBishops++; else lightBishops++;
                        break;
                    default:
                        return false; // pawn, rook or queen
                }
            }

            if (knights == 0)
                return darkBishops == 0 || lightBishops == 0; // no minors, or all bishops share a color
            return knights == 1 && bishops == 0;
        }

        // Placement + side to move + castling + en passant square only if an en passant capture is legal.
        public static string RepetitionKey(Position position, IReadOnlyList<Move> legalMoves)
        {
            bool epLegal = false;
            for (int i = 0; i < legalMoves.Count; i++)
            {
                if ((legalMoves[i].Flags & MoveFlags.EnPassant) != 0)
                {
                    epLegal = true;
                    break;
                }
            }

            string[] fields = position.ToFen().Split(' ');
            return fields[0] + " " + fields[1] + " " + fields[2] + " " + (epLegal ? fields[3] : "-");
        }
    }
}
