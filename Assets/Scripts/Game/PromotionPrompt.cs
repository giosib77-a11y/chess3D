using System;
using Chess.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess.Game
{
    /// <summary>Small OnGUI window for choosing the promotion piece; Esc cancels.</summary>
    public class PromotionPrompt : MonoBehaviour
    {
        private static readonly PieceType[] Choices =
        {
            PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight
        };

        private Side _side;
        private Action<PieceType> _onChosen;
        private Action _onCancelled;

        public bool IsOpen { get; private set; }

        /// <summary>Frame in which the prompt was last closed; lets callers ignore the click that closed it.</summary>
        public int LastClosedFrame { get; private set; } = -1;

        public void Open(Side side, Action<PieceType> onChosen, Action onCancelled)
        {
            _side = side;
            _onChosen = onChosen;
            _onCancelled = onCancelled;
            IsOpen = true;
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Cancel();
        }

        private void OnGUI()
        {
            if (!IsOpen)
                return;

            const float width = 220f;
            const float height = 230f;
            Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            GUI.Box(rect, "Promote pawn (" + _side + ")");

            float y = rect.y + 30f;
            foreach (PieceType type in Choices)
            {
                if (GUI.Button(new Rect(rect.x + 10f, y, width - 20f, 30f), type.ToString()))
                {
                    Choose(type);
                    return;
                }
                y += 36f;
            }
            if (GUI.Button(new Rect(rect.x + 10f, y, width - 20f, 30f), "Cancel"))
                Cancel();
        }

        private void Choose(PieceType type)
        {
            Action<PieceType> callback = _onChosen;
            Close();
            callback?.Invoke(type);
        }

        private void Cancel()
        {
            Action callback = _onCancelled;
            Close();
            callback?.Invoke();
        }

        // IsOpen is cleared before any callback runs.
        private void Close()
        {
            IsOpen = false;
            LastClosedFrame = Time.frameCount;
            _onChosen = null;
            _onCancelled = null;
        }
    }
}
