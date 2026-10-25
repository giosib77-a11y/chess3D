using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chess.Core;

namespace Chess.AI
{
    /// <summary>IChessAI backed by a Stockfish process (or any IUciTransport).</summary>
    public sealed class StockfishAI : IChessAI
    {
        private static readonly TimeSpan StopWait = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan QuitWait = TimeSpan.FromSeconds(1);

        private readonly IUciTransport _transport;
        private readonly object _gate = new object();

        private Task _initTask;
        private ChessAIException _fault;
        private int _busy;
        private int _disposed;
        private volatile bool _ready;
        private TimeSpan _handshakeTimeout = TimeSpan.FromSeconds(10);
        private TimeSpan _searchTimeout = TimeSpan.FromSeconds(30);

        public StockfishAI(string executablePath, AIDifficulty difficulty)
            : this(CreateProcessTransport(executablePath), difficulty)
        {
        }

        public StockfishAI(IUciTransport transport, AIDifficulty difficulty)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
        }

        public AIDifficulty Difficulty { get; }

        public TimeSpan HandshakeTimeout
        {
            get => _handshakeTimeout;
            set => _handshakeTimeout = ValidateTimeout(value);
        }

        public TimeSpan SearchTimeout
        {
            get => _searchTimeout;
            set => _searchTimeout = ValidateTimeout(value);
        }

        public bool IsReady => _ready;

        public async Task InitializeAsync(CancellationToken ct)
        {
            ThrowIfUnusable();
            ct.ThrowIfCancellationRequested();

            Task init;
            lock (_gate)
            {
                // The shared init is bounded by HandshakeTimeout, not by one caller's token.
                if (_initTask == null) _initTask = Task.Run(InitCoreAsync);
                init = _initTask;
            }
            await init.ConfigureAwait(false);
        }

        public async Task<Move> GetBestMoveAsync(string fen, CancellationToken ct)
        {
            if (fen == null) throw new ArgumentNullException(nameof(fen));
            ThrowIfUnusable();

            var state = new GameState(fen); // FormatException on bad FEN
            if (state.IsGameOver)
                throw new InvalidOperationException("The game is already over; no move can be searched.");
            ct.ThrowIfCancellationRequested();

            if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
                throw new InvalidOperationException("Another GetBestMoveAsync call is already in progress.");
            try
            {
                await InitializeAsync(ct).ConfigureAwait(false);
                return await SearchAsync(state, fen, ct).ConfigureAwait(false);
            }
            finally
            {
                Volatile.Write(ref _busy, 0);
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

            bool started;
            lock (_gate) { started = _initTask != null; }

            if (started)
            {
                try
                {
                    _transport.SendLine(UciProtocol.Quit);
                    DateTime deadline = DateTime.UtcNow + QuitWait;
                    while (!_transport.HasExited && DateTime.UtcNow < deadline)
                        Thread.Sleep(10);
                }
                catch (Exception)
                {
                    // Best effort; the transport is disposed (killed) below anyway.
                }
            }

            try { _transport.Dispose(); } catch (Exception) { }
        }

        private async Task InitCoreAsync()
        {
            try
            {
                try { _transport.Start(); }
                catch (ChessAIException) { throw; }
                catch (Exception ex) { throw new ChessAIException("Cannot start Stockfish.", ex); }

                Send(UciProtocol.Uci);
                var options = new Dictionary<string, UciOption>(StringComparer.Ordinal);
                while (true)
                {
                    string line = await ReadAsync(_handshakeTimeout, CancellationToken.None).ConfigureAwait(false);
                    if (line.Trim() == UciProtocol.UciOk) break;
                    if (UciProtocol.TryParseOption(line, out UciOption option)) options[option.Name] = option;
                }

                foreach (string command in UciProtocol.BuildDifficultyCommands(Difficulty, options))
                    Send(command);

                Send(UciProtocol.IsReady);
                while ((await ReadAsync(_handshakeTimeout, CancellationToken.None).ConfigureAwait(false)).Trim() != UciProtocol.ReadyOk)
                {
                }

                _ready = true;
            }
            catch (ChessAIException ex)
            {
                _fault = ex;
                throw;
            }
        }

        private async Task<Move> SearchAsync(GameState state, string fen, CancellationToken ct)
        {
            try
            {
                Send(UciProtocol.BuildPosition(fen));
                Send(UciProtocol.BuildGo(Difficulty));

                string uci;
                try
                {
                    while (true)
                    {
                        string line = await ReadAsync(_searchTimeout, ct).ConfigureAwait(false);
                        if (UciProtocol.TryParseBestMove(line, out uci)) break;
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    await StopSearchAsync().ConfigureAwait(false);
                    throw;
                }

                if (uci == null)
                    throw new ChessAIException("Stockfish returned no move (bestmove (none)).");
                if (!state.TryResolveUciMove(uci, out Move move))
                    throw new ChessAIException("Stockfish returned an illegal move: '" + uci + "'.");
                return move;
            }
            catch (ChessAIException ex)
            {
                _fault = ex;
                throw;
            }
        }

        // Asks the engine to stop and discards its bestmove so the next search starts clean.
        private async Task StopSearchAsync()
        {
            try
            {
                Send(UciProtocol.Stop);
                while (true)
                {
                    string line = await ReadAsync(StopWait, CancellationToken.None).ConfigureAwait(false);
                    if (UciProtocol.TryParseBestMove(line, out _)) return;
                }
            }
            catch (ChessAIException ex)
            {
                // Engine state is unknown now; refuse further use.
                _fault = ex;
            }
        }

        private void Send(string line)
        {
            try { _transport.SendLine(line); }
            catch (ChessAIException) { throw; }
            catch (Exception ex) { throw new ChessAIException("Cannot write to Stockfish.", ex); }
        }

        private async Task<string> ReadAsync(TimeSpan timeout, CancellationToken ct)
        {
            string line;
            try
            {
                line = await _transport.ReadLineAsync(timeout, ct).ConfigureAwait(false);
            }
            catch (TimeoutException ex)
            {
                throw new ChessAIException("Stockfish did not respond in time.", ex);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ChessAIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ChessAIException("Stockfish communication failed.", ex);
            }

            if (line == null) throw new ChessAIException("Stockfish process ended unexpectedly.");
            return line;
        }

        private void ThrowIfUnusable()
        {
            if (Volatile.Read(ref _disposed) != 0) throw new ObjectDisposedException(nameof(StockfishAI));
            ChessAIException fault = _fault;
            if (fault != null)
                throw new ChessAIException("Stockfish is in a failed state: " + fault.Message, fault);
        }

        private static TimeSpan ValidateTimeout(TimeSpan value)
        {
            if (value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(value), value, "Timeout must be positive.");
            return value;
        }

        private static IUciTransport CreateProcessTransport(string executablePath)
        {
            if (executablePath == null) throw new ArgumentNullException(nameof(executablePath));
            if (!File.Exists(executablePath)) throw new StockfishNotFoundException(executablePath);
            return new ProcessUciTransport(executablePath);
        }
    }
}
