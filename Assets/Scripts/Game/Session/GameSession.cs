using System;
using System.IO;
using Chess.AI;
using Chess.Campaign;
using Chess.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chess.Game
{
    public enum LaunchMode { None, Campaign, QuickGame }

    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string CampaignMap = "CampaignMap";
        public const string Battle = "MainScene";
    }

    /// <summary>
    /// Carries launch parameters, campaign progress and navigation between scenes.
    /// All statics are reset on every Play (domain reload is disabled in this project).
    /// </summary>
    public static class GameSession
    {
        private static CampaignSaveStore store;
        private static bool loaded;

        public static LaunchMode Mode { get; private set; }
        public static string RegionId { get; private set; }
        public static Side HumanSide { get; private set; }
        public static DifficultyPreset QuickDifficulty { get; private set; }
        public static CampaignProgress Campaign { get; private set; }
        public static string LoadWarning { get; private set; }
        public static bool LastSaveFailed { get; private set; }

        public static CampaignSaveStore Store
        {
            get
            {
                if (store == null)
                {
                    store = new CampaignSaveStore(Application.persistentDataPath);
                }
                return store;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            store = null;
            loaded = false;
            Mode = LaunchMode.None;
            RegionId = null;
            HumanSide = default;
            QuickDifficulty = default;
            Campaign = null;
            LoadWarning = null;
            LastSaveFailed = false;
        }

        public static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }
            loaded = true;

            Debug.Log("[GameSession] Campaign save file: " + Store.FilePath);
            CampaignLoadResult result = Store.Load();
            switch (result.Status)
            {
                case CampaignLoadStatus.Loaded:
                    Campaign = result.Progress;
                    break;
                case CampaignLoadStatus.Corrupted:
                    LoadWarning = result.Warning;
                    break;
            }
        }

        public static void StartNewCampaign()
        {
            Campaign = CampaignProgress.CreateNew();
            LoadWarning = null;
            TrySaveCampaign();
        }

        public static bool TrySaveCampaign()
        {
            if (Campaign == null)
            {
                throw new InvalidOperationException("No campaign to save.");
            }

            try
            {
                Store.Save(Campaign);
                LastSaveFailed = false;
                return true;
            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
                LastSaveFailed = true;
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogException(ex);
                LastSaveFailed = true;
                return false;
            }
        }

        public static void StartCampaignBattle(string regionId)
        {
            if (Campaign == null)
            {
                throw new InvalidOperationException("No campaign is loaded.");
            }
            if (Campaign.GetState(regionId) == RegionState.Locked)
            {
                throw new InvalidOperationException("Region '" + regionId + "' is locked.");
            }

            Mode = LaunchMode.Campaign;
            RegionId = regionId;
            HumanSide = Side.White;
            SceneManager.LoadScene(SceneNames.Battle);
        }

        public static void StartQuickGame(Side humanSide, DifficultyPreset difficulty)
        {
            Mode = LaunchMode.QuickGame;
            RegionId = null;
            HumanSide = humanSide;
            QuickDifficulty = difficulty;
            SceneManager.LoadScene(SceneNames.Battle);
        }

        public static void GoToMainMenu()
        {
            Mode = LaunchMode.None;
            RegionId = null;
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        public static void GoToCampaignMap()
        {
            Mode = LaunchMode.None;
            RegionId = null;
            SceneManager.LoadScene(SceneNames.CampaignMap);
        }
    }
}
