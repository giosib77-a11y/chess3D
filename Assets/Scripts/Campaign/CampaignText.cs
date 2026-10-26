using System;

namespace Chess.Campaign
{
    public static class CampaignText
    {
        public static string SaveFailedWarning { get { return TextTable.Active.Get(TextId.SaveFailedWarning); } }
        public static string CorruptedSaveWarning { get { return TextTable.Active.Get(TextId.CorruptedSaveWarning); } }
        public static string NotCountedMessage { get { return TextTable.Active.Get(TextId.NotCountedMessage); } }

        public static string StateName(RegionState state)
        {
            switch (state)
            {
                case RegionState.Locked: return TextTable.Active.Get(TextId.StateLocked);
                case RegionState.Available: return TextTable.Active.Get(TextId.StateAvailable);
                case RegionState.Liberated: return TextTable.Active.Get(TextId.StateLiberated);
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown region state.");
            }
        }

        public static string RegionLabel(Region region, RegionState state)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            return TextTable.Active.Format(TextId.RegionLabelFormat, region.Name, region.Era, region.Elo, StateName(state));
        }

        public static string MapStatus(CampaignProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (progress.IsComplete) return TextTable.Active.Get(TextId.AllLiberated);
            return TextTable.Active.Format(TextId.MapStatusFormat, progress.LiberatedCount, CampaignRegions.All.Count);
        }

        public static string BattleInfo(Region region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            return TextTable.Active.Format(TextId.BattleInfoFormat, region.Name, region.Era, region.Elo);
        }
    }
}
