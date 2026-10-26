using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chess.AI;
using Chess.Campaign;
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

        [SerializeField] private BattlePanel battlePanel;

        // Serialized in every build (layout must match); only read inside #if UNITY_EDITOR.
#pragma warning disable CS0414 // assigned but never used in the non-Editor variant
        [SerializeField] private string debugCampaignStartFen = "";
#pragma warning restore CS0414

        private LaunchMode _mode = LaunchMode.None; // captured from GameSession in Start
        private Region _region; // Campaign only
        private AIDifficulty _launchDifficulty; // null = use the inspector 'difficulty'
        private bool _gameOverHandled; // reset in ResetGame

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
            if (battlePanel != null)
            {
                battlePanel.RetryClicked += OnRetryClicked;
                battlePanel.BackClicked += OnBackClicked;
            }
        }

        private void OnDisable()
        {
            if (selection != null)
                selection.MoveChosen -= OnMoveChosen;
            if (battlePanel != null)
            {
                battlePanel.RetryClicked -= OnRetryClicked;
                battlePanel.BackClicked -= OnBackClicked;
            }
            // A domain reload (script recompile during Play) calls OnDisable but NOT OnDestroy, so the
            // engine must be stopped here or stockfish.exe is orphaned. Disposed synchronously on the
            // main thread (blocks <= ~1-3 s at teardown): a Task.Run might not finish before the domain
            // unloads. Side effect: disabling/enabling this component in the inspector during Play
            // loses the AI and the game continues as hot-seat.
            StopAi();
        }

        private void StopAi()
        {
            try
            {
                _cts.Cancel();
            }
            finally
            {
                _generation++;
                _thinking = false;
                IChessAI ai = _ai;
                _ai = null;
                try
                {
                    ai?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                // Fresh token source so OnDestroy / a later OnEnable never touches a disposed one.
                _cts.Dispose();
                _cts = new CancellationTokenSource();
            }
        }

        private void Start()
        {
            ApplyLaunchParameters();
            CreateAi();
#if UNITY_EDITOR
            if (DebugFenActive)
                Debug.LogWarning("[GameController] DEBUG FEN active: " + debugCampaignStartFen);
#endif
            ResetGame();
        }

        // Stage 2: scene launch parameters. Mode None (MainScene played directly) keeps the Stage 1 behavior.
        private void ApplyLaunchParameters()
        {
            _mode = GameSession.Mode;
            _region = null;
            _launchDifficulty = null;

            if (_mode == LaunchMode.Campaign)
            {
                if (!CampaignRegions.TryGet(GameSession.RegionId, out _region))
                {
                    Debug.LogError("[GameController] Unknown campaign region '" + GameSession.RegionId
                        + "'. Playing without campaign progress.");
                }
                if (GameSession.Campaign == null)
                {
                    Debug.LogError("[GameController] Campaign launch without a loaded campaign. Results will not be recorded.");
                }

                humanSide = Side.White;
                playAgainstAI = true;
                if (_region != null)
                {
                    _launchDifficulty = AIDifficulty.FromElo(_region.Elo);
                    ShowInGame(CampaignText.BattleInfo(_region), TextTable.Active.Get(TextId.BackToMap));
                }
                else
                {
                    ShowInGame(TextTable.Active.Get(TextId.BattleInfoUnknown), TextTable.Active.Get(TextId.BackToMap));
                }
            }
            else if (_mode == LaunchMode.QuickGame)
            {
                humanSide = GameSession.HumanSide;
                playAgainstAI = true;
                _launchDifficulty = AIDifficulty.FromPreset(GameSession.QuickDifficulty);
                ShowInGame(GameText.QuickGameInfo(DifficultyText.Name(GameSession.QuickDifficulty), humanSide),
                    TextTable.Active.Get(TextId.BackToMenu));
            }
            else if (battlePanel != null)
            {
                battlePanel.HideAll();
            }
        }

        private void ShowInGame(string info, string leaveLabel)
        {
            if (battlePanel != null)
                battlePanel.ShowInGame(info, leaveLabel);
        }

        private void OnRetryClicked()
        {
            Restart();
        }

        private void OnBackClicked()
        {
            if (_mode == LaunchMode.Campaign)
                GameSession.GoToCampaignMap();
            else if (_mode == LaunchMode.QuickGame)
                GameSession.GoToMainMenu();
        }

        // Called after every successful move; records the result once per game.
        private void CheckGameOver()
        {
            if (_state == null || !_state.IsGameOver || _gameOverHandled)
                return;
            _gameOverHandled = true;
            try
            {
                HandleGameOver();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void HandleGameOver()
        {
            if (_mode == LaunchMode.None)
                return;

            // Counted only if the AI is still the opponent (not lost to a hot-seat fallback / missing Stockfish).
            bool counted = playAgainstAI && _ai != null;
            BattleOutcome outcome = BattleRules.GetOutcome(_state.Result, humanSide);

            if (_mode == LaunchMode.Campaign)
            {
                CampaignProgress campaign = GameSession.Campaign;
                if (campaign == null || _region == null)
                {
                    // Invalid launch (already logged in Start): show the outcome, record nothing.
                    ShowResult(BattleRules.GetTitle(outcome), BuildStatus(), true,
                        TextTable.Active.Get(TextId.Retry), TextTable.Active.Get(TextId.BackToMap));
                    return;
                }

                BattleSummary summary = campaign.RecordBattle(_region.Id, outcome, counted);
                string message = summary.Message;
                if (summary.ProgressChanged && !GameSession.TrySaveCampaign())
                    message += "\n" + CampaignText.SaveFailedWarning;
                ShowResult(summary.Title, message, summary.CanRetry,
                    TextTable.Active.Get(TextId.Retry), TextTable.Active.Get(TextId.BackToMap));
            }
            else
            {
                ShowResult(BattleRules.GetTitle(outcome), BuildStatus(), true,
                    TextTable.Active.Get(TextId.PlayAgain), TextTable.Active.Get(TextId.BackToMenu));
            }
        }

        private void ShowResult(string title, string message, bool showRetry, string retryLabel, string backLabel)
        {
            if (battlePanel != null)
                battlePanel.ShowResult(title, message, showRetry, retryLabel, backLabel);
        }

        private void Update()
        {
            // After a domain reload non-serialized fields are reset (_state and _ai are null, Start is
            // not called again): start a fresh game once; with no AI it continues as hot-seat.
            if (_state == null)
                ResetGame();

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame
                && (selection == null || !selection.IsPromotionOpen))
                Restart();
        }

        private void OnDestroy()
        {
            // Must kill the Stockfish process when Play stops. Idempotent: OnDisable already ran StopAi.
            StopAi();
            _cts.Dispose();
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
                CheckGameOver();
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

            // A cancelled search (stop + waiting up to 2 s for bestmove) still occupies the instance
            // (busy), so an immediate new GetBestMoveAsync would throw InvalidOperationException.
            // The instance is therefore replaced by a fresh one; the old one is disposed off the main
            // thread because Dispose can wait up to a second for the process.
            if (wasThinking && _ai != null)
            {
                IChessAI old = _ai;
                _ai = null;
                DisposeInBackground(old);
                CreateAi();
            }

            ResetGame();
        }

        private void ResetGame()
        {
            _state = CreateState();
            _gameOverHandled = false;
            if (battlePanel != null)
                battlePanel.HideResult();
            if (boardView != null)
                boardView.ClearHighlights();
            Refresh();
            StartAiTurnIfNeeded();
        }

        private GameState CreateState()
        {
            // None: startFen (Stage 1). Campaign: Editor-only debug FEN. QuickGame: always the start position.
            string fen = "";
            string fenName = "startFen";
            if (_mode == LaunchMode.None)
            {
                fen = startFen;
            }
#if UNITY_EDITOR
            else if (DebugFenActive)
            {
                fen = debugCampaignStartFen;
                fenName = "debugCampaignStartFen";
            }
#endif

            if (!string.IsNullOrWhiteSpace(fen))
            {
                try
                {
                    return new GameState(fen);
                }
                catch (FormatException ex)
                {
                    Debug.LogError("Invalid " + fenName + ", using the start position: " + ex.Message);
                }
            }
            return new GameState();
        }

#if UNITY_EDITOR
        private bool DebugFenActive =>
            _mode == LaunchMode.Campaign && !string.IsNullOrWhiteSpace(debugCampaignStartFen);
#endif

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
                AIDifficulty level = _launchDifficulty ?? AIDifficulty.FromPreset(difficulty);
                ai = new StockfishAI(path, level);
                Debug.Log("[GameController] AI difficulty: skill " + level.SkillLevel
                    + ", depth " + (level.Depth.HasValue ? level.Depth.Value.ToString() : "-")
                    + ", movetime " + level.MoveTimeMs + " ms, UCI_Elo "
                    + (level.Elo.HasValue ? level.Elo.Value.ToString() : "off"));
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
            if (_state != null && AiActive && !_thinking && !_state.IsGameOver && _state.SideToMove != humanSide)
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
                CheckGameOver();
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
            DisposeInBackground(failed);
            UpdateHud();
            UpdateInteractable();
        }

        private static void DisposeInBackground(IChessAI ai)
        {
            Task.Run(() =>
            {
                try
                {
                    ai.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex); // thread-safe
                }
            });
        }

        private void ReportAiFailure(Exception ex)
        {
            Debug.LogException(ex);
            _error = ex is StockfishNotFoundException notFound
                ? GameText.StockfishNotFound(notFound.Path)
                : GameText.AiError(ex.Message);
        }

        private void Refresh()
        {
            if (piecesView != null)
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
            if (hud == null || _state == null)
                return;
            string status = BuildStatus();
#if UNITY_EDITOR
            if (DebugFenActive)
                status = GameText.WithDebugFen(status);
#endif
            hud.Show(status, _error);
        }

        private string BuildStatus()
        {
            return GameText.Status(_state.Status, _state.SideToMove, _state.IsCheck, _thinking);
        }
    }
}
