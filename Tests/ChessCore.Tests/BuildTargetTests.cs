using System.Reflection;
using System.Runtime.Versioning;
using Xunit;

namespace Chess.Tests
{
    public class BuildTargetTests
    {
        private const string NetStandard21 = ".NETStandard,Version=v2.1";

        [Theory]
        [InlineData("Chess.Core")]
        [InlineData("Chess.AI")]
        [InlineData("Chess.Campaign")]
        public void AssemblyTargetsNetStandard21(string assemblyName)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            TargetFrameworkAttribute attribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal(NetStandard21, attribute.FrameworkName);
        }

        [Fact]
        public void CampaignReferencesOnlyCore()
        {
            string[] allowed = { "Chess.Core", "netstandard" };
            foreach (AssemblyName reference in Assembly.Load("Chess.Campaign").GetReferencedAssemblies())
            {
                Assert.Contains(reference.Name, allowed);
            }
        }

        [Fact]
        public void CoreAndAiTargetNetStandard21()
        {
            foreach (string name in new[] { "Chess.Core", "Chess.AI" })
            {
                TargetFrameworkAttribute attribute =
                    Assembly.Load(name).GetCustomAttribute<TargetFrameworkAttribute>();
                Assert.NotNull(attribute);
                Assert.Equal(NetStandard21, attribute.FrameworkName);
            }
        }
    }
}
