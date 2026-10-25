using System;
using System.IO;
using System.Text;

namespace Chess.Campaign
{
    public enum CampaignLoadStatus { NoSave, Loaded, Corrupted }

    public sealed class CampaignLoadResult
    {
        internal CampaignLoadResult(CampaignLoadStatus status, CampaignProgress progress, string warning)
        {
            Status = status;
            Progress = progress;
            Warning = warning;
        }

        public CampaignLoadStatus Status { get; }
        public CampaignProgress Progress { get; }
        public string Warning { get; }
    }

    public sealed class CampaignSaveStore
    {
        public const string FileName = "campaign.json";
        public const string TempFileName = "campaign.json.tmp";
        public const string BackupFileName = "campaign.corrupt.json";

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public CampaignSaveStore(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Directory must not be empty.", nameof(directory));
            }

            Directory = directory;
        }

        public string Directory { get; }
        public string FilePath
        {
            get { return Path.Combine(Directory, FileName); }
        }

        public string BackupPath
        {
            get { return Path.Combine(Directory, BackupFileName); }
        }

        private string TempPath
        {
            get { return Path.Combine(Directory, TempFileName); }
        }

        public CampaignLoadResult Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return new CampaignLoadResult(CampaignLoadStatus.NoSave, CampaignProgress.CreateNew(), null);
                }

                string text = StrictUtf8.GetString(File.ReadAllBytes(FilePath));
                if (text.Length > 0 && text[0] == '﻿')
                {
                    text = text.Substring(1);
                }

                CampaignProgress progress = CampaignSaveSerializer.Deserialize(text);
                return new CampaignLoadResult(CampaignLoadStatus.Loaded, progress, null);
            }
            catch (Exception)
            {
                // Contract: Load never throws; content or read I/O errors count as a damaged save.
                TryBackUpCorruptFile();
                return new CampaignLoadResult(
                    CampaignLoadStatus.Corrupted, CampaignProgress.CreateNew(), CampaignText.CorruptedSaveWarning);
            }
        }

        public void Save(CampaignProgress progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            byte[] bytes = StrictUtf8.GetBytes(CampaignSaveSerializer.Serialize(progress));
            System.IO.Directory.CreateDirectory(Directory);

            string temp = TempPath;
            try
            {
                File.WriteAllBytes(temp, bytes);
                if (File.Exists(FilePath))
                {
                    try
                    {
                        File.Replace(temp, FilePath, null);
                    }
                    catch (Exception ex) when (ex is PlatformNotSupportedException || ex is IOException)
                    {
                        // Fallback for runtimes where File.Replace is unavailable.
                        File.Delete(FilePath);
                        File.Move(temp, FilePath);
                    }
                }
                else
                {
                    File.Move(temp, FilePath);
                }
            }
            catch
            {
                TryDelete(temp);
                throw;
            }
        }

        private void TryBackUpCorruptFile()
        {
            try
            {
                if (File.Exists(BackupPath))
                {
                    File.Delete(BackupPath);
                }

                File.Move(FilePath, BackupPath);
            }
            catch (Exception)
            {
                // Best effort: the user still gets a new campaign and the warning.
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
