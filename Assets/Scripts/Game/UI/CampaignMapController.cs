using System;
using Chess.Campaign;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.Game
{
    /// <summary>Fills the campaign map from GameSession.Campaign and starts battles.</summary>
    public sealed class CampaignMapController : MonoBehaviour
    {
        [SerializeField] private RegionButton[] regionButtons;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (regionButtons == null)
            {
                Debug.LogError("[CampaignMapController] 'regionButtons' is not assigned.", this);
                return;
            }
            if (statusText == null)
            {
                Debug.LogError("[CampaignMapController] 'statusText' is not assigned.", this);
            }
            if (backButton == null)
            {
                Debug.LogError("[CampaignMapController] 'backButton' is not assigned.", this);
            }
            else
            {
                backButton.onClick.AddListener(GameSession.GoToMainMenu);
            }

            GameSession.EnsureLoaded();
            if (GameSession.Campaign == null)
            {
                GameSession.StartNewCampaign();
            }

            CampaignProgress progress = GameSession.Campaign;
            foreach (RegionButton regionButton in regionButtons)
            {
                if (regionButton == null)
                {
                    Debug.LogError("[CampaignMapController] 'regionButtons' contains an unassigned element.", this);
                    continue;
                }

                if (!CampaignRegions.TryGet(regionButton.RegionId, out Region region))
                {
                    Debug.LogError("[CampaignMapController] Unknown regionId '" + regionButton.RegionId
                        + "' on " + regionButton.name, regionButton);
                    DisableUnknown(regionButton);
                    continue;
                }

                regionButton.Show(region, progress.GetState(region.Id));
                regionButton.Clicked += OnRegionClicked;
            }

            if (statusText != null)
            {
                string status = CampaignText.MapStatus(progress);
                if (GameSession.LastSaveFailed)
                {
                    status += "\n" + CampaignText.SaveFailedWarning;
                }
                statusText.text = status;
            }
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(GameSession.GoToMainMenu);
            }
            if (regionButtons == null)
            {
                return;
            }
            foreach (RegionButton regionButton in regionButtons)
            {
                if (regionButton != null)
                {
                    regionButton.Clicked -= OnRegionClicked;
                }
            }
        }

        private static void DisableUnknown(RegionButton regionButton)
        {
            Button unknownButton = regionButton.GetComponent<Button>();
            if (unknownButton != null)
            {
                unknownButton.interactable = false;
            }
        }

        private void OnRegionClicked(string regionId)
        {
            try
            {
                GameSession.StartCampaignBattle(regionId);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError("[CampaignMapController] Cannot start battle for '" + regionId + "': " + ex.Message, this);
            }
        }
    }
}
