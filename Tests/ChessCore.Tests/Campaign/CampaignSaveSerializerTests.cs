using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Campaign;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class CampaignSaveSerializerTests
    {
        [Fact]
        public void Serialize_New()
        {
            Assert.Equal("{\"version\":1,\"liberated\":[]}", CampaignSaveSerializer.Serialize(CampaignProgress.CreateNew()));
        }

        [Fact]
        public void Serialize_TwoRegions()
        {
            CampaignProgress p = CampaignProgress.FromLiberated(new[] { "kakheti", "kartli" });
            Assert.Equal("{\"version\":1,\"liberated\":[\"kartli\",\"kakheti\"]}", CampaignSaveSerializer.Serialize(p));
        }

        [Fact]
        public void Serialize_Null_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => CampaignSaveSerializer.Serialize(null));
        }

        [Fact]
        public void RoundTrip_AllSubsets()
        {
            IReadOnlyList<Region> all = CampaignRegions.All;
            for (int mask = 0; mask < (1 << all.Count); mask++)
            {
                var ids = new List<string>();
                for (int i = 0; i < all.Count; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        ids.Add(all[i].Id);
                    }
                }

                CampaignProgress p = CampaignProgress.FromLiberated(ids);
                CampaignProgress back = CampaignSaveSerializer.Deserialize(CampaignSaveSerializer.Serialize(p));
                Assert.Equal(p.LiberatedRegionIds.ToArray(), back.LiberatedRegionIds.ToArray());
            }
        }

        [Theory]
        [InlineData("{\"version\":1,\"liberated\":[\"kartli\"]}", new[] { "kartli" })]
        [InlineData("  \r\n{ \"version\" :\t1 ,\n \"liberated\" : [ \"kartli\" , \"kakheti\" ] }\r\n ", new[] { "kartli", "kakheti" })]
        [InlineData("{\"liberated\":[\"kartli\"],\"version\":1}", new[] { "kartli" })]
        [InlineData("{\"version\":1,\"extra\":{\"a\":[1,2,null,true,-3.5e2,{\"b\":\"x\"}]},\"liberated\":[]}", new string[0])]
        [InlineData("{\"version\":1,\"liberated\":[\"\\u006bartli\"]}", new[] { "kartli" })]
        [InlineData("{\"version\":1,\"liberated\":[\"kakheti\"]}", new[] { "kakheti" })]
        [InlineData("{\"version\":1,\"liberated\":[]}", new string[0])]
        public void Deserialize_AcceptsValidVariants(string json, string[] expected)
        {
            CampaignProgress p = CampaignSaveSerializer.Deserialize(json);
            Assert.Equal(expected, p.LiberatedRegionIds.ToArray());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("{")]
        [InlineData("null")]
        [InlineData("[]")]
        [InlineData("hello")]
        [InlineData("{\"liberated\":[]}")]
        [InlineData("{\"version\":\"1\",\"liberated\":[]}")]
        [InlineData("{\"version\":2,\"liberated\":[]}")]
        [InlineData("{\"version\":1.5,\"liberated\":[]}")]
        [InlineData("{\"version\":1e0,\"liberated\":[]}")]
        [InlineData("{\"version\":1}")]
        [InlineData("{\"version\":1,\"liberated\":\"kartli\"}")]
        [InlineData("{\"version\":1,\"liberated\":[1]}")]
        [InlineData("{\"version\":1,\"liberated\":[\"rome\"]}")]
        [InlineData("{\"version\":1,\"liberated\":[\"kartli\",\"kartli\"]}")]
        [InlineData("{\"version\":1,\"version\":1,\"liberated\":[]}")]
        [InlineData("{\"version\":1,\"liberated\":[]} x")]
        [InlineData("{\"version\":1,\"liberated\":[\"kartli]}")]
        [InlineData("{\"version\":1,\"liberated\":[\"\\q\"]}")]
        [InlineData("{\"version\":1,\"liberated\":[\"kar\ttli\"]}")]
        [InlineData("{\"version\":1,\"liberated\":[ \"kartli\", ]}")]
        [InlineData("{\"version\":1,\"liberated\":[\"\\u12G4\"]}")]
        [InlineData("{\"version\":1,\"liberated\":[],}")]
        [InlineData("{\"version\":01,\"liberated\":[]}")]
        [InlineData("{\"version\":1,\"liberated\":[],\"x\":tru}")]
        [InlineData("{\"version\":1 \"liberated\":[]}")]
        public void Deserialize_Invalid_ThrowsFormatException(string json)
        {
            CampaignSaveFormatException ex = Assert.Throws<CampaignSaveFormatException>(() => CampaignSaveSerializer.Deserialize(json));
            Assert.StartsWith("Invalid campaign save: ", ex.Message);
        }

        [Fact]
        public void Deserialize_DeepNesting_ThrowsFormatException()
        {
            string json = "{\"version\":1,\"liberated\":[],\"x\":" + new string('[', 100000);
            CampaignSaveFormatException ex = Assert.Throws<CampaignSaveFormatException>(() => CampaignSaveSerializer.Deserialize(json));
            Assert.StartsWith("Invalid campaign save: ", ex.Message);
        }

        [Fact]
        public void Deserialize_NestingAtLimit_IsAccepted()
        {
            // root object = depth 1, "x" array chain brings the total to exactly 64.
            string json = "{\"version\":1,\"liberated\":[],\"x\":" + new string('[', 63) + new string(']', 63) + "}";
            Assert.Empty(CampaignSaveSerializer.Deserialize(json).LiberatedRegionIds);

            string tooDeep = "{\"version\":1,\"liberated\":[],\"x\":" + new string('[', 64) + new string(']', 64) + "}";
            Assert.Throws<CampaignSaveFormatException>(() => CampaignSaveSerializer.Deserialize(tooDeep));
        }

        [Fact]
        public void Deserialize_Null_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => CampaignSaveSerializer.Deserialize(null));
        }
    }
}
