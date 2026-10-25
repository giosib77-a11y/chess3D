using System;
using Chess.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess.Game
{
    /// <summary>Composition root. At this stage: builds the GameState, shows the pieces, R restarts.</summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private PiecesView piecesView;
        [SerializeField] private string startFen = "";

        [SerializeField] private SelectionController selection;

        private GameState _state;

        private void OnEnable()
        {
            if (selection != null)
                selection.MoveChosen += OnMoveChosen;
        }

        private void OnDisable()
        {
            if (selection != null)
                selection.MoveChosen -= OnMoveChosen;
        }

        private void Start()
        {
            ResetGame();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame
                && (selection == null || !selection.IsPromotionOpen))
                Restart();
        }

        private void OnMoveChosen(Move move)
        {
            if (_state == null)
                return;
            if (_state.TryMakeMove(move))
            {
                if (boardView != null)
                    boardView.ClearHighlights();
                Refresh();
            }
            else
            {
                Debug.LogWarning("Rejected move: " + move);
                selection.Bind(_state);
            }
        }

        private void Restart()
        {
            ResetGame();
        }

        private void ResetGame()
        {
            _state = CreateState();
            if (boardView != null)
                boardView.ClearHighlights();
            Refresh();
        }

        private GameState CreateState()
        {
            if (!string.IsNullOrWhiteSpace(startFen))
            {
                try
                {
                    return new GameState(startFen);
                }
                catch (FormatException ex)
                {
                    Debug.LogError("Invalid startFen, using the start position: " + ex.Message);
                }
            }
            return new GameState();
        }

        private void Refresh()
        {
            piecesView.Sync(_state.Position);
            if (selection != null)
                selection.Bind(_state);
        }
    }
}
