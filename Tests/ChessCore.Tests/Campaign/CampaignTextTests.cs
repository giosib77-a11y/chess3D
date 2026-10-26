using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Campaign;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class CampaignTextTests
    {
        [Fact]
        public void StateName_AllStates()
        {
            Assert.Equal("ჩაკეტილია", CampaignText.StateName(RegionState.Locked));
            Assert.Equal("ხელმისაწვდომია", CampaignText.StateName(RegionState.Available));
            Assert.Equal("გათავისუფლებულია", CampaignText.StateName(RegionState.Liberated));
            Assert.Throws<ArgumentOutOfRangeException>(() => CampaignText.StateName((RegionState)99));
        }

        [Fact]
        public void RegionLabel_Format()
        {
            Assert.Equal(
                "ქართლი\nV საუკუნე - მეფე ვახტანგ გორგასალი\nElo 800\nხელმისაწვდომია",
                CampaignText.RegionLabel(CampaignRegions.Get("kartli"), RegionState.Available));
            Assert.Throws<ArgumentNullException>(() => CampaignText.RegionLabel(null, RegionState.Locked));
        }

        [Fact]
        public void MapStatus_NewAndComplete()
        {
            Assert.Equal(
                "გათავისუფლებულია 0 რეგიონი 5-დან. აირჩიე რეგიონი ბრძოლისთვის.",
                CampaignText.MapStatus(CampaignProgress.CreateNew()));

            CampaignProgress full = CampaignProgress.FromLiberated(
                new[] { "kartli", "kakheti", "imereti", "svaneti", "khevsureti" });
            Assert.Equal("ყველა რეგიონი გათავისუფლებულია! საქართველო თავისუფალია.", CampaignText.MapStatus(full));

            CampaignProgress two = CampaignProgress.FromLiberated(new[] { "kartli", "kakheti" });
            Assert.Equal(
                "გათავისუფლებულია 2 რეგიონი 5-დან. აირჩიე რეგიონი ბრძოლისთვის.",
                CampaignText.MapStatus(two));
            Assert.Throws<ArgumentNullException>(() => CampaignText.MapStatus(null));
        }

        [Fact]
        public void BattleInfo_Format()
        {
            Assert.Equal(
                "ქართლი - V საუკუნე - მეფე ვახტანგ გორგასალი - მოწინააღმდეგის Elo 800",
                CampaignText.BattleInfo(CampaignRegions.Get("kartli")));
            Assert.Throws<ArgumentNullException>(() => CampaignText.BattleInfo(null));
        }

        [Fact]
        public void Properties_ExactText()
        {
            Assert.Equal("გაფრთხილება: პროგრესი ვერ შეინახა.", CampaignText.SaveFailedWarning);
            Assert.Equal(
                "შენახული კამპანია დაზიანებულია და ვერ ჩაიტვირთა. ფაილი შენახულია სახელით campaign.corrupt.json. დაიწყე ახალი კამპანია.",
                CampaignText.CorruptedSaveWarning);
            Assert.Equal(
                "AI მოწინააღმდეგე არ იყო აქტიური, ამიტომ ეს შედეგი არ ითვლება.",
                CampaignText.NotCountedMessage);
        }

        [Fact]
        public void AllFormattedOutputs_UseSupportedCharsOnly()
        {
            var texts = new List<string>();
            RegionState[] states = { RegionState.Locked, RegionState.Available, RegionState.Liberated };

            foreach (Region region in CampaignRegions.All)
            {
                foreach (RegionState state in states) texts.Add(CampaignText.RegionLabel(region, state));
                texts.Add(CampaignText.BattleInfo(region));
            }

            string[] ids = CampaignRegions.All.Select(r => r.Id).ToArray();
            for (int n = 0; n <= ids.Length; n++)
                texts.Add(CampaignText.MapStatus(CampaignProgress.FromLiberated(ids.Take(n))));

            texts.Add(CampaignText.SaveFailedWarning);
            texts.Add(CampaignText.CorruptedSaveWarning);
            texts.Add(CampaignText.NotCountedMessage);
            texts.Add(BattleRules.NotCountedTitle);

            foreach (BattleOutcome outcome in new[] { BattleOutcome.Victory, BattleOutcome.Defeat, BattleOutcome.Draw })
            {
                texts.Add(BattleRules.GetTitle(outcome));
                for (int i = 0; i < ids.Length; i++)
                {
                    // Available: all previous liberated; Liberated: this one liberated as well.
                    foreach (bool liberated in new[] { false, true })
                    {
                        IEnumerable<string> start = ids.Take(i);
                        if (liberated) start = start.Concat(new[] { ids[i] });
                        BattleSummary s = CampaignProgress.FromLiberated(start).RecordBattle(ids[i], outcome, true);
                        texts.Add(s.Title);
                        texts.Add(s.Message);
                    }
                }
                BattleSummary nc = CampaignProgress.CreateNew().RecordBattle(ids[0], outcome, false);
                texts.Add(nc.Title);
                texts.Add(nc.Message);
            }

            Assert.NotEmpty(texts);
            foreach (string text in texts)
            {
                Assert.False(string.IsNullOrEmpty(text));
                foreach (char c in text)
                    Assert.True(TextTable.IsSupportedChar(c), "Unsupported char U+" + ((int)c).ToString("X4") + " in: " + text);
            }
        }
    }
}
