using System;
using System.Collections.Generic;

namespace Chess.Campaign
{
    public static class CampaignRegions
    {
        private static readonly Region[] Regions =
        {
            new Region("kartli", "Kartli", "5th century - King Vakhtang Gorgasali", 800),
            new Region("kakheti", "Kakheti", "16th century - King Levan", 1100),
            new Region("imereti", "Imereti", "17th century - King Alexander III", 1400),
            new Region("svaneti", "Svaneti", "12th century - King David the Builder", 1800),
            new Region("khevsureti", "Khevsureti", "18th century - King Erekle II", 2200),
        };

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
