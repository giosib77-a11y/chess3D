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

        private GameState _state;

        private void Start()
        {
            ResetGame();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                Restart();
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
        }
    }
}
