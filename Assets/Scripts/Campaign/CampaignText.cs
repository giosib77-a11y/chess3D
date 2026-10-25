using System;

namespace Chess.Campaign
{
    public static class CampaignText
    {
        public const string SaveFailedWarning = "Warning: progress could not be saved.";
        public const string CorruptedSaveWarning =
            "The saved campaign was damaged and could not be loaded. It was kept as campaign.corrupt.json. Start a new campaign.";
        public const string NotCountedMessage = "The AI opponent was not active, so this result does not count.";

        public static string StateName(RegionState state)
        {
            switch (state)
            {
                case RegionState.Locked: return "Locked";
                case RegionState.Available: return "Available";
                case RegionState.Liberated: return "Liberated";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown region state.");
            }
        }

        public static string RegionLabel(Region region, RegionState state)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            return region.Name + "\n" + region.Era + "\nElo " + region.Elo + "\n" + StateName(state);
        }

        public static string MapStatus(CampaignProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (progress.IsComplete) return "All regions are liberated! Georgia is free.";
            return "Liberated " + progress.LiberatedCount + " of " + CampaignRegions.All.Count
                + ". Choose a region to fight for.";
        }

        public static string BattleInfo(Region region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            return region.Name + " - " + region.Era + " - enemy Elo " + region.Elo;
        }
    }
}
