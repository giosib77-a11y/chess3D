using System;

namespace Chess.AI
{
    public enum DifficultyPreset
    {
        Easy,
        Medium,
        Hard,
        Master
    }

    public sealed class AIDifficulty
    {
        public AIDifficulty(int skillLevel, int? depth, int moveTimeMs, int? elo)
        {
            if (skillLevel < 0 || skillLevel > 20)
                throw new ArgumentOutOfRangeException(nameof(skillLevel), skillLevel, "Skill level must be 0..20.");
            if (depth.HasValue && (depth.Value < 1 || depth.Value > 30))
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "Depth must be 1..30.");
            if (moveTimeMs < 50 || moveTimeMs > 60000)
                throw new ArgumentOutOfRangeException(nameof(moveTimeMs), moveTimeMs, "Move time must be 50..60000 ms.");
            if (elo.HasValue && (elo.Value < 500 || elo.Value > 3500))
                throw new ArgumentOutOfRangeException(nameof(elo), elo, "Elo must be 500..3500.");

            SkillLevel = skillLevel;
            Depth = depth;
            MoveTimeMs = moveTimeMs;
            Elo = elo;
        }

        public int SkillLevel { get; }
        public int? Depth { get; }
        public int MoveTimeMs { get; }
        public int? Elo { get; }
        public bool LimitStrength => Elo.HasValue;

        public static AIDifficulty FromPreset(DifficultyPreset preset)
        {
            switch (preset)
            {
                case DifficultyPreset.Easy: return new AIDifficulty(0, 1, 100, null);
                case DifficultyPreset.Medium: return new AIDifficulty(20, null, 300, 1400);
                case DifficultyPreset.Hard: return new AIDifficulty(20, null, 500, 1800);
                case DifficultyPreset.Master: return new AIDifficulty(20, null, 1000, 2200);
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unknown difficulty preset.");
            }
        }
    }
}
