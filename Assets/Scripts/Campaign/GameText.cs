using System;
using Chess.Core;

namespace Chess.Campaign
{
    // HUD / promotion / quick game texts, all via TextTable.Active.
    public static class GameText
    {
        public static string HelpLine { get { return TextTable.Active.Get(TextId.HelpLine); } }

        public static string Status(GameStatus status, Side sideToMove, bool isCheck, bool aiThinking)
        {
            TextTable t = TextTable.Active;
            switch (status)
            {
                case GameStatus.Checkmate:
                    return t.Get(sideToMove.Opposite() == Side.White ? TextId.CheckmateWhiteWins : TextId.CheckmateBlackWins);
                case GameStatus.Stalemate:
                    return t.Get(TextId.Stalemate);
                case GameStatus.FiftyMoveRule:
                    return t.Get(TextId.DrawFiftyMove);
                case GameStatus.ThreefoldRepetition:
                    return t.Get(TextId.DrawThreefold);
                case GameStatus.InsufficientMaterial:
                    return t.Get(TextId.DrawInsufficient);
                case GameStatus.InProgress:
                    if (aiThinking) return t.Get(TextId.AiThinking);
                    string move = t.Get(SideToMoveId(sideToMove));
                    return isCheck ? t.Format(TextId.CheckFormat, move) : move;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, "Undefined GameStatus.");
            }
        }

        public static string SideName(Side side)
        {
            switch (side)
            {
                case Side.White: return TextTable.Active.Get(TextId.SideWhite);
                case Side.Black: return TextTable.Active.Get(TextId.SideBlack);
                default: throw new ArgumentOutOfRangeException(nameof(side), side, "Undefined Side.");
            }
        }

        public static string PieceName(PieceType type)
        {
            switch (type)
            {
                case PieceType.Queen: return TextTable.Active.Get(TextId.PieceQueen);
                case PieceType.Rook: return TextTable.Active.Get(TextId.PieceRook);
                case PieceType.Bishop: return TextTable.Active.Get(TextId.PieceBishop);
                case PieceType.Knight: return TextTable.Active.Get(TextId.PieceKnight);
                default: throw new ArgumentOutOfRangeException(nameof(type), type, "Not a promotion piece.");
            }
        }

        public static string PromotionTitle(Side side)
        {
            return TextTable.Active.Format(TextId.PromotionTitleFormat, SideName(side));
        }

        public static string QuickColorLabel(Side side)
        {
            return TextTable.Active.Format(TextId.QuickColorFormat, SideName(side));
        }

        public static string QuickDifficultyLabel(string difficultyName)
        {
            if (difficultyName == null) throw new ArgumentNullException(nameof(difficultyName));
            return TextTable.Active.Format(TextId.QuickDifficultyFormat, difficultyName);
        }

        public static string QuickGameInfo(string difficultyName, Side humanSide)
        {
            if (difficultyName == null) throw new ArgumentNullException(nameof(difficultyName));
            return TextTable.Active.Format(TextId.QuickGameInfoFormat, difficultyName, SideName(humanSide));
        }

        public static string StockfishNotFound(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return TextTable.Active.Format(TextId.StockfishNotFoundFormat, path);
        }

        public static string AiError(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return TextTable.Active.Format(TextId.AiErrorFormat, message.TrimEnd('.'));
        }

        public static string WithDebugFen(string status)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));
            return TextTable.Active.Format(TextId.DebugFenFormat, status);
        }

        private static TextId SideToMoveId(Side side)
        {
            switch (side)
            {
                case Side.White: return TextId.WhiteToMove;
                case Side.Black: return TextId.BlackToMove;
                default: throw new ArgumentOutOfRangeException(nameof(side), side, "Undefined Side.");
            }
        }
    }
}
