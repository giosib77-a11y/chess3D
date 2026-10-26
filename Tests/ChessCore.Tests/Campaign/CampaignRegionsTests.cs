using System;
using System.Linq;
using Chess.Campaign;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class CampaignRegionsTests
    {
        [Fact]
        public void All_HasFiveRegionsInCampaignOrder()
        {
            string[] ids = CampaignRegions.All.Select(r => r.Id).ToArray();
            Assert.Equal(new[] { "kartli", "kakheti", "imereti", "svaneti", "khevsureti" }, ids);
        }

        [Fact]
        public void All_MatchContractTable()
        {
            var expected = new[]
            {
                ("kartli", "ქართლი", "V საუკუნე - მეფე ვახტანგ გორგასალი", 800),
                ("kakheti", "კახეთი", "XVI საუკუნე - მეფე ლევანი", 1100),
                ("imereti", "იმერეთი", "XVII საუკუნე - მეფე ალექსანდრე III", 1400),
                ("svaneti", "სვანეთი", "XII საუკუნე - მეფე დავით აღმაშენებელი", 1800),
                ("khevsureti", "ხევსურეთი", "XVIII საუკუნე - მეფე ერეკლე II", 2200),
            };

            Assert.Equal(expected.Length, CampaignRegions.All.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Region r = CampaignRegions.All[i];
                Assert.Equal(expected[i].Item1, r.Id);
                Assert.Equal(expected[i].Item2, r.Name);
                Assert.Equal(expected[i].Item3, r.Era);
                Assert.Equal(expected[i].Item4, r.Elo);
            }
        }

        [Fact]
        public void Get_UnknownOrNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => CampaignRegions.Get(null));
            ArgumentException ex = Assert.Throws<ArgumentException>(() => CampaignRegions.Get("atlantis"));
            Assert.Contains("Unknown region id: 'atlantis'", ex.Message);
            Assert.Throws<ArgumentException>(() => CampaignRegions.Get("Kartli"));
            Assert.Throws<ArgumentException>(() => CampaignRegions.Get(""));
        }

        [Fact]
        public void TryGet_And_IndexOf()
        {
            Assert.True(CampaignRegions.TryGet("imereti", out Region region));
            Assert.Same(CampaignRegions.Get("imereti"), region);
            Assert.Equal(2, CampaignRegions.IndexOf("imereti"));
            Assert.Equal(0, CampaignRegions.IndexOf("kartli"));
            Assert.Equal(4, CampaignRegions.IndexOf("khevsureti"));

            Assert.False(CampaignRegions.TryGet(null, out Region r1));
            Assert.Null(r1);
            Assert.False(CampaignRegions.TryGet("nowhere", out Region r2));
            Assert.Null(r2);
            Assert.Equal(-1, CampaignRegions.IndexOf(null));
            Assert.Equal(-1, CampaignRegions.IndexOf("nowhere"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Kartli")]
        [InlineData("kartli1")]
        [InlineData("kar tli")]
        public void Region_InvalidId_Throws(string id)
        {
            Assert.Throws<ArgumentException>(() => new Region(id, "Name", "Era", 800));
        }

        [Fact]
        public void Region_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new Region(null, "Name", "Era", 800));
            Assert.Throws<ArgumentNullException>(() => new Region("a", null, "Era", 800));
            Assert.Throws<ArgumentNullException>(() => new Region("a", "Name", null, 800));
            Assert.Throws<ArgumentException>(() => new Region("a", "", "Era", 800));
            Assert.Throws<ArgumentException>(() => new Region("a", "Name", "", 800));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Region("a", "Name", "Era", 499));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Region("a", "Name", "Era", 3501));

            Assert.Equal(500, new Region("a", "Name", "Era", 500).Elo);
            Assert.Equal(3500, new Region("a", "Name", "Era", 3500).Elo);
        }
    }
}
