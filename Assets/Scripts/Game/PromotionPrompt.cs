using System;
using Chess.Campaign;
using Chess.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Chess.Game
{
    /// <summary>uGUI panel for choosing the promotion piece; Esc cancels.</summary>
    public class PromotionPrompt : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button queenButton;
        [SerializeField] private Button rookButton;
        [SerializeField] private Button bishopButton;
        [SerializeField] private Button knightButton;
        [SerializeField] private Button cancelButton;

        private Action<PieceType> _onChosen;
        private Action _onCancelled;
        private bool _showPending;
        private bool _shown;

        public bool IsOpen { get; private set; }

        /// <summary>Frame in which the prompt was last closed; lets callers ignore the click that closed it.</summary>
        public int LastClosedFrame { get; private set; } = -1;

        private void Awake()
        {
            if (panel == null) Debug.LogError("[PromotionPrompt] 'panel' is not assigned.", this);
            if (titleText == null) Debug.LogError("[PromotionPrompt] 'titleText' is not assigned.", this);
            Hook(queenButton, "queenButton", OnQueen);
            Hook(rookButton, "rookButton", OnRook);
            Hook(bishopButton, "bishopButton", OnBishop);
            Hook(knightButton, "knightButton", OnKnight);
            Hook(cancelButton, "cancelButton", OnCancelButton);

            if (panel != null)
                panel.SetActive(false);
        }

        private void OnDestroy()
        {
            Unhook(queenButton, OnQueen);
            Unhook(rookButton, OnRook);
            Unhook(bishopButton, OnBishop);
            Unhook(knightButton, OnKnight);
            Unhook(cancelButton, OnCancelButton);
        }

        private void Hook(Button button, string field, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                Debug.LogError("[PromotionPrompt] '" + field + "' is not assigned.", this);
            else
                button.onClick.AddListener(action);
        }

        private static void Unhook(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.RemoveListener(action);
        }

        public void Open(Side side, Action<PieceType> onChosen, Action onCancelled)
        {
            if (panel == null)
            {
                Debug.LogError("[PromotionPrompt] Cannot open: 'panel' is not assigned.", this);
                onCancelled?.Invoke();
                return;
            }

            _onChosen = onChosen;
            _onCancelled = onCancelled;
            if (titleText != null)
                titleText.text = GameText.PromotionTitle(side);
            // The panel is shown in LateUpdate, after EventSystem.Update, so the press that
            // opened the prompt cannot reach one of its buttons.
            _showPending = true;
            IsOpen = true;
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Cancel();
        }

        private void LateUpdate()
        {
            if (!_showPending)
                return;
            _showPending = false;
            if (IsOpen && panel != null)
            {
                panel.SetActive(true);
                _shown = true;
            }
        }

        private void OnQueen() { Choose(PieceType.Queen); }
        private void OnRook() { Choose(PieceType.Rook); }
        private void OnBishop() { Choose(PieceType.Bishop); }
        private void OnKnight() { Choose(PieceType.Knight); }
        private void OnCancelButton() { if (_shown) Cancel(); }

        private void Choose(PieceType type)
        {
            if (!IsOpen || !_shown)
                return;
            Action<PieceType> callback = _onChosen;
            Close();
            callback?.Invoke(type);
        }

        private void Cancel()
        {
            if (!IsOpen)
                return;
            Action callback = _onCancelled;
            Close();
            callback?.Invoke();
        }

        // IsOpen is cleared before any callback runs.
        private void Close()
        {
            IsOpen = false;
            _showPending = false;
            _shown = false;
            LastClosedFrame = Time.frameCount;
            _onChosen = null;
            _onCancelled = null;
            if (panel != null)
                panel.SetActive(false);
            EventSystem es = EventSystem.current;
            if (es != null)
                es.SetSelectedGameObject(null);
        }
    }
}
