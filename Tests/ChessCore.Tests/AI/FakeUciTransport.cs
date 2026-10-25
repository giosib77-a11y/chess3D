using System.Collections.Concurrent;
using Chess.AI;

namespace ChessCore.Tests.AI
{
    /// <summary>Scripted IUciTransport: responses are enqueued when a matching command is sent.</summary>
    public sealed class FakeUciTransport : IUciTransport
    {
        private readonly ConcurrentQueue<string> _lines = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly List<string> _sent = new List<string>();
        private readonly List<KeyValuePair<string, string[]>> _rules = new List<KeyValuePair<string, string[]>>();
        private readonly object _gate = new object();
        private int _startCount;
        private int _disposeCount;

        public Exception StartException { get; set; }
        public bool HasExitedValue { get; set; }
        public bool ExitOnQuit { get; set; } = true;

        public bool HasExited => HasExitedValue;
        public int StartCount => Volatile.Read(ref _startCount);
        public int DisposeCount => Volatile.Read(ref _disposeCount);

        public IReadOnlyList<string> Sent
        {
            get { lock (_gate) return _sent.ToArray(); }
        }

        /// <summary>Standard engine: replies to "uci" with the options and "uciok", to "isready" with "readyok".</summary>
        public static FakeUciTransport CreateEngine(params string[] optionLines)
        {
            var fake = new FakeUciTransport();
            var uciReply = new List<string>(optionLines) { "uciok" };
            fake.When("uci", uciReply.ToArray());
            fake.When("isready", "readyok");
            return fake;
        }

        /// <summary>When a sent line equals command or starts with "command ", enqueue the responses.</summary>
        public FakeUciTransport When(string command, params string[] responses)
        {
            lock (_gate) _rules.Add(new KeyValuePair<string, string[]>(command, responses));
            return this;
        }

        public void Enqueue(params string[] lines)
        {
            foreach (string line in lines)
            {
                _lines.Enqueue(line);
                _signal.Release();
            }
        }

        /// <summary>Simulates the process closing stdout (null line).</summary>
        public void Eof() => Enqueue((string)null);

        public bool WasSent(string line) => Sent.Contains(line);

        public void Start()
        {
            Interlocked.Increment(ref _startCount);
            if (StartException != null) throw StartException;
        }

        public void SendLine(string line)
        {
            List<string[]> replies = new List<string[]>();
            lock (_gate)
            {
                _sent.Add(line);
                foreach (var rule in _rules)
                    if (line == rule.Key || line.StartsWith(rule.Key + " ")) replies.Add(rule.Value);
            }
            foreach (string[] reply in replies) Enqueue(reply);
            if (line == "quit" && ExitOnQuit) HasExitedValue = true;
        }

        public async Task<string> ReadLineAsync(TimeSpan timeout, CancellationToken ct)
        {
            if (!await _signal.WaitAsync(timeout, ct).ConfigureAwait(false))
                throw new TimeoutException("fake timeout");
            if (!_lines.TryDequeue(out string line)) return null;
            if (line == null)
            {
                _lines.Enqueue(null);
                _signal.Release();
            }
            return line;
        }

        public void Dispose() => Interlocked.Increment(ref _disposeCount);
    }
}
