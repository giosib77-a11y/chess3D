using System;

namespace Chess.Campaign
{
    public sealed class Region
    {
        public const int MinElo = 500;
        public const int MaxElo = 3500;

        public Region(string id, string name, string era, int elo)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (era == null) throw new ArgumentNullException(nameof(era));

            if (id.Length == 0)
                throw new ArgumentException("Region id must not be empty.", nameof(id));
            for (int i = 0; i < id.Length; i++)
            {
                if (id[i] < 'a' || id[i] > 'z')
                    throw new ArgumentException("Region id must contain only 'a'..'z': '" + id + "'.", nameof(id));
            }
            if (name.Length == 0)
                throw new ArgumentException("Region name must not be empty.", nameof(name));
            if (era.Length == 0)
                throw new ArgumentException("Region era must not be empty.", nameof(era));
            if (elo < MinElo || elo > MaxElo)
                throw new ArgumentOutOfRangeException(nameof(elo), elo, "Elo must be in " + MinElo + ".." + MaxElo + ".");

            Id = id;
            Name = name;
            Era = era;
            Elo = elo;
        }

        public string Id { get; }
        public string Name { get; }
        public string Era { get; }
        public int Elo { get; }
    }
}
