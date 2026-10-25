using System;
using Chess.Campaign;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class CampaignTextTests
    {
        [Fact]
        public void StateName_AllStates()
        {
            Assert.Equal("Locked", CampaignText.StateName(RegionState.Locked));
            Assert.Equal("Available", CampaignText.StateName(RegionState.Available));
            Assert.Equal("Liberated", CampaignText.StateName(RegionState.Liberated));
            Assert.Throws<ArgumentOutOfRangeException>(() => CampaignText.StateName((RegionState)99));
        }

        [Fact]
        public void RegionLabel_Format()
        {
            Assert.Equal(
                "Kartli\n5th century - King Vakhtang Gorgasali\nElo 800\nAvailable",
                CampaignText.RegionLabel(CampaignRegions.Get("kartli"), RegionState.Available));
            Assert.Throws<ArgumentNullException>(() => CampaignText.RegionLabel(null, RegionState.Locked));
        }

        [Fact]
        public void MapStatus_NewAndComplete()
        {
            Assert.Equal(
                "Liberated 0 of 5. Choose a region to fight for.",
                CampaignText.MapStatus(CampaignProgress.CreateNew()));

            CampaignProgress full = CampaignProgress.FromLiberated(
                new[] { "kartli", "kakheti", "imereti", "svaneti", "khevsureti" });
            Assert.Equal("All regions are liberated! Georgia is free.", CampaignText.MapStatus(full));

            CampaignProgress two = CampaignProgress.FromLiberated(new[] { "kartli", "kakheti" });
            Assert.Equal("Liberated 2 of 5. Choose a region to fight for.", CampaignText.MapStatus(two));
            Assert.Throws<ArgumentNullException>(() => CampaignText.MapStatus(null));
        }

        [Fact]
        public void BattleInfo_Format()
        {
            Assert.Equal(
                "Kartli - 5th century - King Vakhtang Gorgasali - enemy Elo 800",
                CampaignText.BattleInfo(CampaignRegions.Get("kartli")));
            Assert.Throws<ArgumentNullException>(() => CampaignText.BattleInfo(null));
        }

        [Fact]
        public void Constants_ExactText()
        {
            Assert.Equal("Warning: progress could not be saved.", CampaignText.SaveFailedWarning);
            Assert.Equal(
                "The saved campaign was damaged and could not be loaded. It was kept as campaign.corrupt.json. Start a new campaign.",
                CampaignText.CorruptedSaveWarning);
            Assert.Equal(
                "The AI opponent was not active, so this result does not count.",
                CampaignText.NotCountedMessage);
        }
    }
}
