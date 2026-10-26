using System;
using System.Collections.Generic;

namespace Chess.Campaign
{
    public static class CampaignRegions
    {
        private static readonly Region[] Regions =
        {
            Make("kartli", TextId.RegionKartliName, TextId.RegionKartliEra, 800),
            Make("kakheti", TextId.RegionKakhetiName, TextId.RegionKakhetiEra, 1100),
            Make("imereti", TextId.RegionImeretiName, TextId.RegionImeretiEra, 1400),
            Make("svaneti", TextId.RegionSvanetiName, TextId.RegionSvanetiEra, 1800),
            Make("khevsureti", TextId.RegionKhevsuretiName, TextId.RegionKhevsuretiEra, 2200),
        };

        private static Region Make(string id, TextId name, TextId era, int elo)
        {
            return new Region(id, TextTable.Active.Get(name), TextTable.Active.Get(era), elo);
        }

        public static IReadOnlyList<Region> All => Regions;

        public static Region Get(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            int index = IndexOf(id);
            if (index < 0) throw new ArgumentException("Unknown region id: '" + id + "'", nameof(id));
            return Regions[index];
        }

        public static bool TryGet(string id, out Region region)
        {
            int index = IndexOf(id);
            region = index < 0 ? null : Regions[index];
            return region != null;
        }

        public static int IndexOf(string id)
        {
            if (id == null) return -1;
            for (int i = 0; i < Regions.Length; i++)
            {
                if (string.Equals(Regions[i].Id, id, StringComparison.Ordinal)) return i;
            }
            return -1;
        }
    }
}
