using System;
using System.IO;
using System.Linq;
using System.Text;
using Chess.Campaign;
using Xunit;

namespace Chess.Tests.Campaign
{
    public sealed class CampaignSaveStoreTests : IDisposable
    {
        private readonly string _dir =
            Path.Combine(Path.GetTempPath(), "chess3d-tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_dir))
                {
                    Directory.Delete(_dir, true);
                }
            }
            catch (IOException)
            {
            }
        }

        private CampaignSaveStore NewStore()
        {
            Directory.CreateDirectory(_dir);
            return new CampaignSaveStore(_dir);
        }

        private static CampaignProgress Two()
        {
            return CampaignProgress.FromLiberated(new[] { "kartli", "kakheti" });
        }

        [Fact]
        public void Load_NoFile_ReturnsNoSave()
        {
            CampaignLoadResult r = new CampaignSaveStore(_dir).Load();
            Assert.Equal(CampaignLoadStatus.NoSave, r.Status);
            Assert.Equal(0, r.Progress.LiberatedCount);
            Assert.Null(r.Warning);
        }

        [Fact]
        public void Save_ThenLoad_ReturnsLoaded()
        {
            CampaignSaveStore store = NewStore();
            store.Save(Two());
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Loaded, r.Status);
            Assert.Equal(new[] { "kartli", "kakheti" }, r.Progress.LiberatedRegionIds.ToArray());
            Assert.Null(r.Warning);
        }

        [Fact]
        public void Save_Twice_OverwritesExistingFile()
        {
            CampaignSaveStore store = NewStore();
            store.Save(Two());
            store.Save(CampaignProgress.CreateNew());
            Assert.Equal("{\"version\":1,\"liberated\":[]}", File.ReadAllText(store.FilePath));
            Assert.Equal(0, store.Load().Progress.LiberatedCount);
            store.Save(Two());
            Assert.Equal(2, store.Load().Progress.LiberatedCount);
        }

        [Fact]
        public void Save_CreatesMissingDirectory()
        {
            string nested = Path.Combine(_dir, "a", "b");
            var store = new CampaignSaveStore(nested);
            store.Save(Two());
            Assert.True(File.Exists(Path.Combine(nested, CampaignSaveStore.FileName)));
            Assert.Equal(CampaignLoadStatus.Loaded, store.Load().Status);
        }

        [Fact]
        public void Save_LeavesNoTempFile()
        {
            CampaignSaveStore store = NewStore();
            store.Save(Two());
            store.Save(Two());
            Assert.False(File.Exists(Path.Combine(_dir, CampaignSaveStore.TempFileName)));
            Assert.Equal(new[] { CampaignSaveStore.FileName }, Directory.GetFiles(_dir).Select(Path.GetFileName).ToArray());
        }

        [Fact]
        public void Save_WritesUtf8WithoutBom()
        {
            CampaignSaveStore store = NewStore();
            store.Save(Two());
            byte[] bytes = File.ReadAllBytes(store.FilePath);
            Assert.Equal((byte)'{', bytes[0]);
            Assert.Equal("{\"version\":1,\"liberated\":[\"kartli\",\"kakheti\"]}", Encoding.UTF8.GetString(bytes));
        }

        [Fact]
        public void Save_Null_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => NewStore().Save(null));
        }

        [Fact]
        public void Load_FileWithBom_ReturnsLoaded()
        {
            CampaignSaveStore store = NewStore();
            byte[] body = Encoding.UTF8.GetBytes("{\"version\":1,\"liberated\":[\"kartli\"]}");
            File.WriteAllBytes(store.FilePath, new byte[] { 0xEF, 0xBB, 0xBF }.Concat(body).ToArray());
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Loaded, r.Status);
            Assert.Equal(new[] { "kartli" }, r.Progress.LiberatedRegionIds.ToArray());
        }

        [Fact]
        public void Load_Corrupted_ReturnsNewProgressWarningAndBackup()
        {
            CampaignSaveStore store = NewStore();
            File.WriteAllText(store.FilePath, "hello");
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Corrupted, r.Status);
            Assert.Equal(0, r.Progress.LiberatedCount);
            Assert.Equal(CampaignText.CorruptedSaveWarning, r.Warning);
            Assert.False(File.Exists(store.FilePath));
            Assert.Equal("hello", File.ReadAllText(store.BackupPath));
            Assert.Equal(Path.Combine(_dir, CampaignSaveStore.BackupFileName), store.BackupPath);
        }

        [Fact]
        public void Load_Corrupted_OverwritesOldBackup()
        {
            CampaignSaveStore store = NewStore();
            File.WriteAllText(store.BackupPath, "old backup");
            File.WriteAllText(store.FilePath, "new garbage");
            Assert.Equal(CampaignLoadStatus.Corrupted, store.Load().Status);
            Assert.Equal("new garbage", File.ReadAllText(store.BackupPath));
            Assert.False(File.Exists(store.FilePath));
        }

        [Fact]
        public void Load_EmptyFile_IsCorrupted()
        {
            CampaignSaveStore store = NewStore();
            File.WriteAllBytes(store.FilePath, new byte[0]);
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Corrupted, r.Status);
            Assert.True(File.Exists(store.BackupPath));
        }

        [Fact]
        public void Load_InvalidUtf8_IsCorrupted()
        {
            CampaignSaveStore store = NewStore();
            File.WriteAllBytes(store.FilePath, new byte[] { 0xFF, 0xFE, 0xFD });
            Assert.Equal(CampaignLoadStatus.Corrupted, store.Load().Status);
        }

        [Fact]
        public void Load_Corrupted_ThenSave_RecoversNewCampaign()
        {
            CampaignSaveStore store = NewStore();
            File.WriteAllText(store.FilePath, "hello");
            Assert.Equal(CampaignLoadStatus.Corrupted, store.Load().Status);
            store.Save(CampaignProgress.CreateNew());
            Assert.Equal(CampaignLoadStatus.Loaded, store.Load().Status);
            Assert.Equal("hello", File.ReadAllText(store.BackupPath));
        }

        [Fact]
        public void Load_LockedFile_IsCorruptedAndDoesNotThrow()
        {
            CampaignSaveStore store = NewStore();
            store.Save(Two());
            using (new FileStream(store.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                CampaignLoadResult r = store.Load();
                Assert.Equal(CampaignLoadStatus.Corrupted, r.Status);
                Assert.Equal(0, r.Progress.LiberatedCount);
                Assert.Equal(CampaignText.CorruptedSaveWarning, r.Warning);
            }

            Assert.Equal("{\"version\":1,\"liberated\":[\"kartli\",\"kakheti\"]}", File.ReadAllText(store.FilePath));
        }

        [Fact]
        public void Save_TargetLocked_ThrowsAndKeepsOldSave()
        {
            CampaignSaveStore store = NewStore();
            store.Save(Two());
            using (new FileStream(store.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.ThrowsAny<IOException>(() => store.Save(CampaignProgress.CreateNew()));
            }

            Assert.True(File.Exists(store.FilePath));
            Assert.False(File.Exists(Path.Combine(_dir, CampaignSaveStore.TempFileName)));
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Loaded, r.Status);
            Assert.Equal(new[] { "kartli", "kakheti" }, r.Progress.LiberatedRegionIds.ToArray());
        }

        [Fact]
        public void Load_OnlyTempFileExists_RecoversIt()
        {
            CampaignSaveStore store = NewStore();
            string temp = Path.Combine(_dir, CampaignSaveStore.TempFileName);
            File.WriteAllText(temp, "{\"version\":1,\"liberated\":[\"kartli\"]}");
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Loaded, r.Status);
            Assert.Equal(new[] { "kartli" }, r.Progress.LiberatedRegionIds.ToArray());
            Assert.True(File.Exists(store.FilePath));
            Assert.False(File.Exists(temp));
        }

        [Fact]
        public void Load_OnlyCorruptTempFile_IsCorrupted()
        {
            CampaignSaveStore store = NewStore();
            string temp = Path.Combine(_dir, CampaignSaveStore.TempFileName);
            File.WriteAllText(temp, "garbage");
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Corrupted, r.Status);
            Assert.Equal(CampaignText.CorruptedSaveWarning, r.Warning);
            Assert.False(File.Exists(temp));
            Assert.Equal("garbage", File.ReadAllText(store.BackupPath));
        }

        [Fact]
        public void Load_FileOver64KB_IsCorrupted()
        {
            CampaignSaveStore store = NewStore();
            string big = "{\"version\":1,\"liberated\":[]}" + new string(' ', 64 * 1024);
            File.WriteAllText(store.FilePath, big);
            CampaignLoadResult r = store.Load();
            Assert.Equal(CampaignLoadStatus.Corrupted, r.Status);
            Assert.Equal(CampaignText.CorruptedSaveWarning, r.Warning);
            Assert.False(File.Exists(store.FilePath));
            Assert.True(File.Exists(store.BackupPath));
        }

        [Fact]
        public void Constructor_InvalidDirectory_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignSaveStore(null));
            Assert.Throws<ArgumentException>(() => new CampaignSaveStore(""));
            Assert.Throws<ArgumentException>(() => new CampaignSaveStore("   "));
        }
    }
}
