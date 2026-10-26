using System;
using Chess.AI;
using Chess.Campaign;

namespace Chess.Game
{
    /// <summary>Localized difficulty preset names. Lives in Game because Campaign does not know AI.</summary>
    public static class DifficultyText
    {
        public static string Name(DifficultyPreset preset)
        {
            switch (preset)
            {
                case DifficultyPreset.Easy: return TextTable.Active.Get(TextId.DifficultyEasy);
                case DifficultyPreset.Medium: return TextTable.Active.Get(TextId.DifficultyMedium);
                case DifficultyPreset.Hard: return TextTable.Active.Get(TextId.DifficultyHard);
                case DifficultyPreset.Master: return TextTable.Active.Get(TextId.DifficultyMaster);
                default: throw new ArgumentOutOfRangeException(nameof(preset), preset, "Undefined DifficultyPreset.");
            }
        }
    }
}
