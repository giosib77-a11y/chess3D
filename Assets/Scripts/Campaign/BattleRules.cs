using System;
using Chess.Core;

namespace Chess.Campaign
{
    public enum BattleOutcome { Victory, Defeat, Draw }

    public static class BattleRules
    {
        public static string NotCountedTitle { get { return TextTable.Active.Get(TextId.TitleNotCounted); } }

        public static BattleOutcome GetOutcome(GameResult result, Side humanSide)
        {
            switch (result)
            {
                case GameResult.WhiteWins:
                    return humanSide == Side.White ? BattleOutcome.Victory : BattleOutcome.Defeat;
                case GameResult.BlackWins:
                    return humanSide == Side.Black ? BattleOutcome.Victory : BattleOutcome.Defeat;
                case GameResult.Draw:
                    return BattleOutcome.Draw;
                default:
                    throw new ArgumentException("Game result must be a finished game, got: " + result, nameof(result));
            }
        }

        public static string GetTitle(BattleOutcome outcome)
        {
            switch (outcome)
            {
                case BattleOutcome.Victory: return TextTable.Active.Get(TextId.TitleVictory);
                case BattleOutcome.Defeat: return TextTable.Active.Get(TextId.TitleDefeat);
                case BattleOutcome.Draw: return TextTable.Active.Get(TextId.TitleDraw);
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown battle outcome.");
            }
        }
    }

    public sealed class BattleSummary
    {
        public BattleSummary(string title, string message, bool canRetry, bool progressChanged)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (message == null) throw new ArgumentNullException(nameof(message));
            Title = title;
            Message = message;
            CanRetry = canRetry;
            ProgressChanged = progressChanged;
        }

        public string Title { get; }
        public string Message { get; }
        public bool CanRetry { get; }
        public bool ProgressChanged { get; }
    }
}
