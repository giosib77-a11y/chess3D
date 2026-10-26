using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.Game
{
    /// <summary>Battle HUD: info line, leave button and the result panel. Pure view: GameController drives it.</summary>
    public sealed class BattlePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private Button leaveButton;
        [SerializeField] private TMP_Text leaveLabel;
        [SerializeField] private GameObject resultRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private TMP_Text retryLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text backLabel;

        private bool _inGame; // ShowInGame was called and HideAll has not been

        public event Action RetryClicked;
        public event Action BackClicked;

        private void Awake()
        {
            Bind(leaveButton, "leaveButton", OnBackClicked);
            Bind(retryButton, "retryButton", OnRetryClicked);
            Bind(backButton, "backButton", OnBackClicked);
            RequireObject(infoText, "infoText");
            RequireObject(leaveLabel, "leaveLabel");
            RequireObject(resultRoot, "resultRoot");
            RequireObject(titleText, "titleText");
            RequireObject(messageText, "messageText");
            RequireObject(retryLabel, "retryLabel");
            RequireObject(backLabel, "backLabel");
            HideAll();
        }

        private void OnDestroy()
        {
            Unbind(leaveButton, OnBackClicked);
            Unbind(retryButton, OnRetryClicked);
            Unbind(backButton, OnBackClicked);
        }

        public void ShowInGame(string info, string leaveLabelText)
        {
            _inGame = true;
            if (infoText != null)
            {
                infoText.text = info ?? string.Empty;
                infoText.gameObject.SetActive(true);
            }
            if (leaveLabel != null)
            {
                leaveLabel.text = leaveLabelText ?? string.Empty;
            }
            SetActive(resultRoot, false);
            SetActive(leaveButton, true);
        }

        public void ShowResult(string title, string message, bool showRetry, string retryLabelText, string backLabelText)
        {
            if (titleText != null)
            {
                titleText.text = title ?? string.Empty;
            }
            if (messageText != null)
            {
                messageText.text = message ?? string.Empty;
            }
            if (retryLabel != null)
            {
                retryLabel.text = retryLabelText ?? string.Empty;
            }
            if (backLabel != null)
            {
                backLabel.text = backLabelText ?? string.Empty;
            }
            SetActive(retryButton, showRetry);
            SetActive(leaveButton, false);
            SetActive(resultRoot, true);
        }

        public void HideResult()
        {
            SetActive(resultRoot, false);
            SetActive(leaveButton, _inGame);
        }

        public void HideAll()
        {
            _inGame = false;
            if (infoText != null)
            {
                infoText.gameObject.SetActive(false);
            }
            SetActive(leaveButton, false);
            SetActive(resultRoot, false);
        }

        private void Bind(Button button, string fieldName, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                Debug.LogError("[BattlePanel] '" + fieldName + "' is not assigned.", this);
                return;
            }
            button.onClick.AddListener(action);
        }

        private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private void RequireObject(UnityEngine.Object value, string fieldName)
        {
            if (value == null)
            {
                Debug.LogError("[BattlePanel] '" + fieldName + "' is not assigned.", this);
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetActive(Button target, bool active)
        {
            if (target != null)
            {
                target.gameObject.SetActive(active);
            }
        }

        private void OnRetryClicked()
        {
            RetryClicked?.Invoke();
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }
    }
}
