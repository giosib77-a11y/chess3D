using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chess.AI;
using Chess.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess.Game
{
    /// <summary>Composition root: owns the GameState, wires views, runs the optional Stockfish opponent.</summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private PiecesView piecesView;
        [SerializeField] private string startFen = "";

        [SerializeField] private SelectionController selection;

        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private Side humanSide = Side.White;

        [SerializeField] private StatusHud hud;
        [SerializeField] private bool playAgainstAI = true;
        [SerializeField] private DifficultyPreset difficulty = DifficultyPreset.Medium;
        [SerializeField] private string stockfishRelativePath = "Stockfish/stockfish.exe";
        [SerializeField] private string stockfishPathOverride = "";

        private GameState _state;
        private IChessAI _ai; // null = hot-seat
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private int _generation; // bumped on restart; stale async results are ignored
        private bool _thinking;
        private string _error = "";

        private bool AiActive => playAgainstAI && _ai != null;

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
            CreateAi();
            ResetGame();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame
                && (selection == null || !selection.IsPromotionOpen))
                Restart();
        }

        private void OnDestroy()
        {
            // Must kill the Stockfish process when Play stops.
            try
            {
                _cts.Cancel();
            }
            finally
            {
                _ai?.Dispose();
                _ai = null;
                _cts.Dispose();
            }
        }

        private void OnMoveChosen(Move move)
        {
            if (_state == null || _thinking || (AiActive && _state.SideToMove != humanSide))
                return;
            if (_state.TryMakeMove(move))
            {
                if (boardView != null)
                    boardView.ClearHighlights();
                Refresh();
                if (cameraRig != null)
                    cameraRig.NotifyHumanMoveMade();
                StartAiTurnIfNeeded();
            }
            else
            {
                Debug.LogWarning("Rejected move: " + move);
                selection.Bind(_state);
            }
        }

        private void Restart()
        {
            bool wasThinking = _thinking;
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _generation++;
            _thinking = false;

            // A cancelled search leaves the engine in an unknown state (StockfishAI may fault itself),
            // so after an interrupted search the instance is replaced by a fresh one. The old one is
            // disposed off the main thread because Dispose can wait up to a second for the process.
            if (wasThinking && _ai != null)
            {
                IChessAI old = _ai;
                _ai = null;
                Task.Run(() => old.Dispose());
                CreateAi();
            }

            ResetGame();
        }

        private void ResetGame()
        {
            _state = CreateState();
            if (boardView != null)
                boardView.ClearHighlights();
            Refresh();
            StartAiTurnIfNeeded();
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

        private void CreateAi()
        {
            _ai = null;
            if (!playAgainstAI)
                return;

            StockfishAI ai;
            try
            {
                string path = !string.IsNullOrWhiteSpace(stockfishPathOverride)
                    ? stockfishPathOverride
                    : Path.Combine(Application.streamingAssetsPath, stockfishRelativePath);
                ai = new StockfishAI(path, AIDifficulty.FromPreset(difficulty));
            }
            catch (Exception ex)
            {
                ReportAiFailure(ex);
                return;
            }

            _ai = ai;
            InitializeAiAsync(ai, _cts.Token);
        }

        private async void InitializeAiAsync(IChessAI ai, CancellationToken token)
        {
            try
            {
                await ai.InitializeAsync(token);
            }
            catch (OperationCanceledException)
            {
                // Restart or destroy.
            }
            catch (Exception ex)
            {
                if (this == null)
                    return;
                FallBackToHotSeat(ai, ex);
            }
        }

        private void StartAiTurnIfNeeded()
        {
            if (AiActive && !_thinking && !_state.IsGameOver && _state.SideToMove != humanSide)
                RunAiTurnAsync();
        }

        private async void RunAiTurnAsync()
        {
            IChessAI ai = _ai;
            int gen = _generation;
            CancellationToken token = _cts.Token;
            _thinking = true;
            UpdateHud();
            UpdateInteractable();
            try
            {
                Move move = await ai.GetBestMoveAsync(_state.ToFen(), token);
                if (this == null || gen != _generation)
                    return; // restarted or destroyed while thinking: drop the stale move
                _thinking = false;
                if (!_state.TryMakeMove(move))
                    throw new ChessAIException("AI returned a move the game rejected: " + move);
                Refresh();
            }
            catch (OperationCanceledException)
            {
                // Restart or destroy; Restart already reset the flags.
            }
            catch (Exception ex)
            {
                if (this == null || gen != _generation)
                    return;
                FallBackToHotSeat(ai, ex);
            }
        }

        // Idempotent per AI instance: ignored if that instance was already replaced or dropped.
        private void FallBackToHotSeat(IChessAI failed, Exception ex)
        {
            if (!ReferenceEquals(failed, _ai))
                return;
            ReportAiFailure(ex);
            _ai = null;
            _thinking = false;
            try
            {
                failed.Dispose();
            }
            catch (Exception disposeEx)
            {
                Debug.LogException(disposeEx);
            }
            UpdateHud();
            UpdateInteractable();
        }

        private void ReportAiFailure(Exception ex)
        {
            Debug.LogException(ex);
            _error = ex is StockfishNotFoundException notFound
                ? "Stockfish not found: " + notFound.Path + ". Human vs human mode."
                : "AI error: " + ex.Message + ". Human vs human mode.";
        }

        private void Refresh()
        {
            piecesView.Sync(_state.Position);
            if (selection != null)
                selection.Bind(_state);
            if (cameraRig != null)
                cameraRig.SetCommander(_state.Position.FindKing(humanSide), humanSide);
            UpdateHud();
            UpdateInteractable();
        }

        private void UpdateInteractable()
        {
            if (selection == null || _state == null)
                return;
            selection.Interactable = !_thinking && (!AiActive || _state.SideToMove == humanSide);
        }

        private void UpdateHud()
        {
            if (hud != null && _state != null)
                hud.Show(BuildStatus(), _error);
        }

        private string BuildStatus()
        {
            switch (_state.Status)
            {
                case GameStatus.Checkmate:
                    return "Checkmate - " + (_state.SideToMove == Side.White ? "Black" : "White") + " wins";
                case GameStatus.Stalemate:
                    return "Stalemate - draw";
                case GameStatus.FiftyMoveRule:
                    return "Draw - 50-move rule";
                case GameStatus.ThreefoldRepetition:
                    return "Draw - threefold repetition";
                case GameStatus.InsufficientMaterial:
                    return "Draw - insufficient material";
            }
            if (_thinking)
                return "AI thinking...";
            string text = (_state.SideToMove == Side.White ? "White" : "Black") + " to move";
            return _state.IsCheck ? text + "  Check!" : text;
        }
    }
}
