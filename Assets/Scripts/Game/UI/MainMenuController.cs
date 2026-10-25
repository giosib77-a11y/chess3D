using Chess.AI;
using Chess.Campaign;
using Chess.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Chess.Game
{
    /// <summary>Main menu: new/continue campaign, quick game setup, quit.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private GameObject quickGamePanel;

        [SerializeField] private Button newCampaignButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quickGameButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        [SerializeField] private Button colorButton;
        [SerializeField] private Button difficultyButton;
        [SerializeField] private Button startQuickGameButton;
        [SerializeField] private Button backFromQuickGameButton;

        [SerializeField] private Text warningText;
        [SerializeField] private Text colorLabel;
        [SerializeField] private Text difficultyLabel;

        private Side quickSide = Side.White;
        private DifficultyPreset quickDifficulty = DifficultyPreset.Medium;

        private void Awake()
        {
            Bind(newCampaignButton, "newCampaignButton", OnNewCampaign);
            Bind(continueButton, "continueButton", OnContinue);
            Bind(quickGameButton, "quickGameButton", OnQuickGame);
            Bind(quitButton, "quitButton", OnQuit);
            Bind(confirmYesButton, "confirmYesButton", StartNewCampaignAndOpenMap);
            Bind(confirmNoButton, "confirmNoButton", ShowMainPanel);
            Bind(colorButton, "colorButton", OnColorClicked);
            Bind(difficultyButton, "difficultyButton", OnDifficultyClicked);
            Bind(startQuickGameButton, "startQuickGameButton", OnStartQuickGame);
            Bind(backFromQuickGameButton, "backFromQuickGameButton", ShowMainPanel);

            RequireObject(mainPanel, "mainPanel");
            RequireObject(confirmPanel, "confirmPanel");
            RequireObject(quickGamePanel, "quickGamePanel");
            RequireObject(warningText, "warningText");
            RequireObject(colorLabel, "colorLabel");
            RequireObject(difficultyLabel, "difficultyLabel");
        }

        private void Start()
        {
            GameSession.EnsureLoaded();

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(GameSession.Campaign != null);
            }

            quickSide = Side.White;
            quickDifficulty = DifficultyPreset.Medium;
            RefreshQuickLabels();
            RefreshWarning();
            ShowMainPanel();
        }

        private void Bind(Button button, string fieldName, UnityAction action)
        {
            if (button == null)
            {
                Debug.LogError("[MainMenuController] '" + fieldName + "' is not assigned.", this);
                return;
            }
            button.onClick.AddListener(action);
        }

        private void RequireObject(Object value, string fieldName)
        {
            if (value == null)
            {
                Debug.LogError("[MainMenuController] '" + fieldName + "' is not assigned.", this);
            }
        }

        private void RefreshWarning()
        {
            if (warningText == null)
            {
                return;
            }

            string text = string.Empty;
            if (GameSession.Campaign == null && !string.IsNullOrEmpty(GameSession.LoadWarning))
            {
                text = GameSession.LoadWarning;
            }
            if (GameSession.LastSaveFailed)
            {
                text = text.Length == 0
                    ? CampaignText.SaveFailedWarning
                    : text + "\n" + CampaignText.SaveFailedWarning;
            }
            warningText.text = text;
        }

        private void RefreshQuickLabels()
        {
            if (colorLabel != null)
            {
                colorLabel.text = quickSide == Side.White ? "Color: White" : "Color: Black";
            }
            if (difficultyLabel != null)
            {
                difficultyLabel.text = "Difficulty: " + quickDifficulty;
            }
        }

        private void ShowPanel(GameObject panel)
        {
            SetActive(mainPanel, panel == mainPanel);
            SetActive(confirmPanel, panel == confirmPanel);
            SetActive(quickGamePanel, panel == quickGamePanel);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private void ShowMainPanel()
        {
            ShowPanel(mainPanel);
        }

        private void OnNewCampaign()
        {
            if (GameSession.Campaign != null)
            {
                ShowPanel(confirmPanel);
                return;
            }
            StartNewCampaignAndOpenMap();
        }

        private void StartNewCampaignAndOpenMap()
        {
            GameSession.StartNewCampaign();
            GameSession.GoToCampaignMap();
        }

        private void OnContinue()
        {
            if (GameSession.Campaign == null)
            {
                Debug.LogError("[MainMenuController] Continue clicked without a campaign.", this);
                return;
            }
            GameSession.GoToCampaignMap();
        }

        private void OnQuickGame()
        {
            ShowPanel(quickGamePanel);
        }

        private void OnColorClicked()
        {
            quickSide = quickSide == Side.White ? Side.Black : Side.White;
            RefreshQuickLabels();
        }

        private void OnDifficultyClicked()
        {
            switch (quickDifficulty)
            {
                case DifficultyPreset.Easy: quickDifficulty = DifficultyPreset.Medium; break;
                case DifficultyPreset.Medium: quickDifficulty = DifficultyPreset.Hard; break;
                case DifficultyPreset.Hard: quickDifficulty = DifficultyPreset.Master; break;
                default: quickDifficulty = DifficultyPreset.Easy; break;
            }
            RefreshQuickLabels();
        }

        private void OnStartQuickGame()
        {
            GameSession.StartQuickGame(quickSide, quickDifficulty);
        }

        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quit requested");
            Application.Quit();
        }
    }
}
