using System;
using System.Collections.Generic;
using Chess.Campaign;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class CampaignProgressTests
    {
        private static readonly string[] AllIds = { "kartli", "kakheti", "imereti", "svaneti", "khevsureti" };

        [Fact]
        public void NewCampaign_OnlyFirstRegionAvailable()
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            Assert.Equal(RegionState.Available, p.GetState("kartli"));
            Assert.Equal(RegionState.Locked, p.GetState("kakheti"));
            Assert.Equal(RegionState.Locked, p.GetState("imereti"));
            Assert.Equal(RegionState.Locked, p.GetState("svaneti"));
            Assert.Equal(RegionState.Locked, p.GetState("khevsureti"));
            Assert.Equal(0, p.LiberatedCount);
            Assert.False(p.IsComplete);
            Assert.Empty(p.LiberatedRegionIds);
        }

        [Fact]
        public void FromLiberated_StatesFollowRule()
        {
            CampaignProgress p = CampaignProgress.FromLiberated(new[] { "kakheti" });
            Assert.Equal(RegionState.Available, p.GetState("kartli"));
            Assert.Equal(RegionState.Liberated, p.GetState("kakheti"));
            Assert.Equal(RegionState.Available, p.GetState("imereti"));
            Assert.Equal(RegionState.Locked, p.GetState("svaneti"));
            Assert.Equal(RegionState.Locked, p.GetState("khevsureti"));
            Assert.Equal(1, p.LiberatedCount);
        }

        [Fact]
        public void LiberatedRegionIds_AreInCampaignOrder()
        {
            CampaignProgress p = CampaignProgress.FromLiberated(new[] { "imereti", "kartli" });
            Assert.Equal(new[] { "kartli", "imereti" }, p.LiberatedRegionIds);
        }

        [Fact]
        public void FromLiberated_InvalidInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => CampaignProgress.FromLiberated(null));
            Assert.Throws<ArgumentNullException>(() => CampaignProgress.FromLiberated(new string[] { "kartli", null }));
            Assert.Throws<ArgumentException>(() => CampaignProgress.FromLiberated(new[] { "atlantis" }));
            Assert.Throws<ArgumentException>(() => CampaignProgress.FromLiberated(new[] { "kartli", "kartli" }));
        }

        [Fact]
        public void GetState_NullOrUnknown_Throws()
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            Assert.Throws<ArgumentNullException>(() => p.GetState(null));
            Assert.Throws<ArgumentException>(() => p.GetState("atlantis"));
        }

        [Fact]
        public void Victory_OnAvailable_LiberatesAndOpensNext()
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            BattleSummary s = p.RecordBattle("kartli", BattleOutcome.Victory, true);
            Assert.Equal("გამარჯვება!", s.Title);
            Assert.Equal("ქართლი გათავისუფლდა! კახეთი ახლა ხელმისაწვდომია.", s.Message);
            Assert.False(s.CanRetry);
            Assert.True(s.ProgressChanged);
            Assert.Equal(RegionState.Liberated, p.GetState("kartli"));
            Assert.Equal(RegionState.Available, p.GetState("kakheti"));
            Assert.Equal(1, p.LiberatedCount);
        }

        [Fact]
        public void Victory_WhenNextAlreadyLiberated_NoOpenSentence()
        {
            CampaignProgress p = CampaignProgress.FromLiberated(new[] { "kakheti" });
            BattleSummary s = p.RecordBattle("kartli", BattleOutcome.Victory, true);
            Assert.Equal("ქართლი გათავისუფლდა!", s.Message);
            Assert.True(s.ProgressChanged);
        }

        [Fact]
        public void Victory_OnLastRegion_CompletesCampaign()
        {
            CampaignProgress p = CampaignProgress.FromLiberated(new[] { "kartli", "kakheti", "imereti", "svaneti" });
            BattleSummary s = p.RecordBattle("khevsureti", BattleOutcome.Victory, true);
            Assert.Equal("ხევსურეთი გათავისუფლდა! ყველა რეგიონი გათავისუფლებულია! საქართველო თავისუფალია.", s.Message);
            Assert.True(p.IsComplete);
            Assert.Equal(AllIds, p.LiberatedRegionIds);
        }

        [Fact]
        public void Victory_OnLiberated_NoChange()
        {
            CampaignProgress p = CampaignProgress.FromLiberated(new[] { "kartli" });
            BattleSummary s = p.RecordBattle("kartli", BattleOutcome.Victory, true);
            Assert.Equal("გამარჯვება!", s.Title);
            Assert.Equal("ქართლი კვლავ თავისუფალია.", s.Message);
            Assert.False(s.CanRetry);
            Assert.False(s.ProgressChanged);
            Assert.Equal(1, p.LiberatedCount);
        }

        [Theory]
        [InlineData(BattleOutcome.Defeat, "მარცხი")]
        [InlineData(BattleOutcome.Draw, "ფრე")]
        public void DefeatOrDraw_OnAvailable_NoChange(BattleOutcome outcome, string title)
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            BattleSummary s = p.RecordBattle("kartli", outcome, true);
            Assert.Equal(title, s.Title);
            Assert.Equal("ქართლი ჯერ კიდევ მტრის ხელშია.", s.Message);
            Assert.True(s.CanRetry);
            Assert.False(s.ProgressChanged);
            Assert.Equal(0, p.LiberatedCount);
            Assert.Equal(RegionState.Available, p.GetState("kartli"));
        }

        [Theory]
        [InlineData(BattleOutcome.Defeat, "მარცხი")]
        [InlineData(BattleOutcome.Draw, "ფრე")]
        public void DefeatOrDraw_OnLiberated_RemainsFree(BattleOutcome outcome, string title)
        {
            CampaignProgress p = CampaignProgress.FromLiberated(new[] { "kartli" });
            BattleSummary s = p.RecordBattle("kartli", outcome, true);
            Assert.Equal(title, s.Title);
            Assert.Equal("ქართლი კვლავ თავისუფალია.", s.Message);
            Assert.True(s.CanRetry);
            Assert.False(s.ProgressChanged);
            Assert.Equal(RegionState.Liberated, p.GetState("kartli"));
        }

        [Theory]
        [InlineData(BattleOutcome.Victory)]
        [InlineData(BattleOutcome.Defeat)]
        [InlineData(BattleOutcome.Draw)]
        public void NotCounted_AnyOutcome_NoChange(BattleOutcome outcome)
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            BattleSummary s = p.RecordBattle("kartli", outcome, false);
            Assert.Equal("თამაში დასრულდა", s.Title);
            Assert.Equal("AI მოწინააღმდეგე არ იყო აქტიური, ამიტომ ეს შედეგი არ ითვლება.", s.Message);
            Assert.True(s.CanRetry);
            Assert.False(s.ProgressChanged);
            Assert.Equal(0, p.LiberatedCount);
            Assert.Equal(RegionState.Available, p.GetState("kartli"));
        }

        [Fact]
        public void RecordBattle_LockedRegion_ThrowsInvalidOperation()
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => p.RecordBattle("kakheti", BattleOutcome.Victory, true));
            Assert.Equal("Region 'kakheti' is locked.", ex.Message);
            Assert.Throws<InvalidOperationException>(() => p.RecordBattle("kakheti", BattleOutcome.Victory, false));
            Assert.Equal(0, p.LiberatedCount);
            Assert.Equal(RegionState.Locked, p.GetState("kakheti"));
        }

        [Fact]
        public void RecordBattle_UnknownRegion_ThrowsArgument()
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            Assert.Throws<ArgumentException>(() => p.RecordBattle("atlantis", BattleOutcome.Victory, true));
            Assert.Throws<ArgumentNullException>(() => p.RecordBattle(null, BattleOutcome.Victory, true));
            Assert.Equal(0, p.LiberatedCount);
        }

        [Fact]
        public void LiberatedRegionIds_IsSnapshot()
        {
            CampaignProgress p = CampaignProgress.CreateNew();
            IReadOnlyList<string> before = p.LiberatedRegionIds;
            p.RecordBattle("kartli", BattleOutcome.Victory, true);
            Assert.Empty(before);
            Assert.Single(p.LiberatedRegionIds);
        }
    }
}
