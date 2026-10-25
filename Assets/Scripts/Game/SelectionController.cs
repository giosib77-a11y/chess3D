using System;
using System.Collections.Generic;
using Chess.Core;
using UnityEngine;

namespace Chess.Game
{
    /// <summary>Click-to-select / click-to-move logic; raises MoveChosen, never mutates the state.</summary>
    public class SelectionController : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private SquarePicker picker;
        [SerializeField] private PromotionPrompt promotionPrompt;

        private GameState _state;
        private Square? _selected;
        private bool _interactable = true;

        public event Action<Move> MoveChosen;

        /// <summary>True while the promotion window is showing (GameController uses it to block restart).</summary>
        public bool IsPromotionOpen => promotionPrompt != null && promotionPrompt.IsOpen;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                if (!value)
                    ClearSelection();
            }
        }

        public void Bind(GameState state)
        {
            _state = state;
            ClearSelection();
        }

        public void ClearSelection()
        {
            _selected = null;
            if (boardView != null)
                boardView.ClearHighlights();
        }

        private void OnEnable()
        {
            if (picker != null)
                picker.Clicked += OnClicked;
        }

        private void OnDisable()
        {
            if (picker != null)
                picker.Clicked -= OnClicked;
        }

        private void OnClicked(Square? clicked)
        {
            if (!_interactable || _state == null || _state.IsGameOver || IsPromotionOpen)
                return;
            // The click that closed the prompt must not also hit the board behind it.
            if (promotionPrompt != null && promotionPrompt.LastClosedFrame == Time.frameCount)
                return;

            if (clicked == null)
            {
                ClearSelection();
                return;
            }

            Square square = clicked.Value;
            if (IsOwnPiece(square))
            {
                Select(square);
                return;
            }

            if (_selected == null)
                return;

            Square from = _selected.Value;
            Move? legal = null;
            bool promotion = false;
            foreach (Move move in _state.GetLegalMovesFrom(from))
            {
                if (move.To != square)
                    continue;
                legal = move;
                if (move.IsPromotion)
                    promotion = true;
            }

            if (legal == null)
            {
                ClearSelection();
                return;
            }

            if (promotion && promotionPrompt != null)
            {
                promotionPrompt.Open(
                    _state.SideToMove,
                    type => MoveChosen?.Invoke(new Move(from, square, type)),
                    ClearSelection);
                return;
            }

            MoveChosen?.Invoke(legal.Value);
        }

        private bool IsOwnPiece(Square square)
        {
            Piece piece = _state.Position.GetPiece(square);
            return !piece.IsNone && piece.Side == _state.SideToMove;
        }

        private void Select(Square square)
        {
            _selected = square;
            var targets = new List<Square>();
            foreach (Move move in _state.GetLegalMovesFrom(square))
            {
                if (!targets.Contains(move.To))
                    targets.Add(move.To);
            }
            if (boardView != null)
                boardView.ShowSelection(square, targets);
        }
    }
}
