using System;
using Chess.Campaign;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.Game
{
    /// <summary>Shows one campaign region on the map. Pure view: state comes from the caller.</summary>
    public sealed class RegionButton : MonoBehaviour
    {
        private static readonly Color LockedColor = new Color32(0x5A, 0x5A, 0x5A, 0xFF);
        private static readonly Color AvailableColor = new Color32(0xC8, 0x96, 0x2D, 0xFF);
        private static readonly Color LiberatedColor = new Color32(0x3C, 0x8C, 0x3C, 0xFF);

        [SerializeField] private string regionId;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image background;

        public string RegionId => regionId;

        public event Action<string> Clicked;

        private void Awake()
        {
            if (button == null)
            {
                Debug.LogError("[RegionButton] 'button' is not assigned on " + name, this);
                return;
            }
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        public void Show(Region region, RegionState state)
        {
            if (region == null)
            {
                Debug.LogError("[RegionButton] Show called with null region on " + name, this);
                return;
            }
            if (button == null || label == null || background == null)
            {
                Debug.LogError("[RegionButton] 'button', 'label' or 'background' is not assigned on " + name, this);
                return;
            }

            label.text = CampaignText.RegionLabel(region, state);
            label.color = Color.white;
            button.interactable = state != RegionState.Locked;
            background.color = ColorFor(state);
        }

        private static Color ColorFor(RegionState state)
        {
            switch (state)
            {
                case RegionState.Locked: return LockedColor;
                case RegionState.Available: return AvailableColor;
                default: return LiberatedColor;
            }
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(regionId);
        }
    }
}
