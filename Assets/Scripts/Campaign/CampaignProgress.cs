using System;
using System.Collections.Generic;

namespace Chess.Campaign
{
    public enum RegionState { Locked, Available, Liberated }

    public sealed class CampaignProgress
    {
        private readonly bool[] _liberated = new bool[CampaignRegions.All.Count];

        private CampaignProgress()
        {
        }

        public static CampaignProgress CreateNew()
        {
            return new CampaignProgress();
        }

        public static CampaignProgress FromLiberated(IEnumerable<string> liberatedRegionIds)
        {
            if (liberatedRegionIds == null) throw new ArgumentNullException(nameof(liberatedRegionIds));

            var progress = new CampaignProgress();
            foreach (string id in liberatedRegionIds)
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(liberatedRegionIds), "Region id must not be null.");
                int index = CampaignRegions.IndexOf(id);
                if (index < 0)
                    throw new ArgumentException("Unknown region id: '" + id + "'", nameof(liberatedRegionIds));
                if (progress._liberated[index])
                    throw new ArgumentException("Duplicate region id: '" + id + "'", nameof(liberatedRegionIds));
                progress._liberated[index] = true;
            }
            return progress;
        }

        public IReadOnlyList<string> LiberatedRegionIds
        {
            get
            {
                var ids = new List<string>();
                for (int i = 0; i < _liberated.Length; i++)
                {
                    if (_liberated[i]) ids.Add(CampaignRegions.All[i].Id);
                }
                return ids;
            }
        }

        public int LiberatedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _liberated.Length; i++)
                {
                    if (_liberated[i]) count++;
                }
                return count;
            }
        }

        public bool IsComplete => LiberatedCount == _liberated.Length;

        public RegionState GetState(string regionId)
        {
            CampaignRegions.Get(regionId); // throws for null / unknown
            return StateAt(CampaignRegions.IndexOf(regionId));
        }

        public BattleSummary RecordBattle(string regionId, BattleOutcome outcome, bool counted)
        {
            Region region = CampaignRegions.Get(regionId);
            int index = CampaignRegions.IndexOf(regionId);
            RegionState state = StateAt(index);
            if (state == RegionState.Locked)
                throw new InvalidOperationException("Region '" + regionId + "' is locked.");

            if (!counted)
                return new BattleSummary(BattleRules.NotCountedTitle, CampaignText.NotCountedMessage, true, false);

            string title = BattleRules.GetTitle(outcome);

            if (outcome == BattleOutcome.Victory)
            {
                if (state == RegionState.Liberated)
                    return new BattleSummary(title, region.Name + " remains free.", false, false);

                _liberated[index] = true;
                string message = region.Name + " is liberated!";
                int next = index + 1;
                if (next < _liberated.Length && !_liberated[next])
                    message += " " + CampaignRegions.All[next].Name + " is now open.";
                if (IsComplete)
                    message += " All regions are liberated! Georgia is free.";
                return new BattleSummary(title, message, false, true);
            }

            string text = state == RegionState.Liberated
                ? region.Name + " remains free."
                : region.Name + " is still under enemy control.";
            return new BattleSummary(title, text, true, false);
        }

        private RegionState StateAt(int index)
        {
            if (_liberated[index]) return RegionState.Liberated;
            if (index == 0 || _liberated[index - 1]) return RegionState.Available;
            return RegionState.Locked;
        }
    }
}
