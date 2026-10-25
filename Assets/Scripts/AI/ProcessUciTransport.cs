using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.AI
{
    /// <summary>IUciTransport over a child process: no shell, no arguments, redirected stdio.</summary>
    public sealed class ProcessUciTransport : IUciTransport
    {
        private readonly string _executablePath;
        private readonly ConcurrentQueue<string> _lines = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly object _gate = new object();

        private Process _process;
        private bool _disposed;
        private bool _exitedAtDispose;

        public ProcessUciTransport(string executablePath)
        {
            if (executablePath == null) throw new ArgumentNullException(nameof(executablePath));
            _executablePath = executablePath;
        }

        public bool HasExited
        {
            get
            {
                lock (_gate)
                {
                    if (_disposed) return _exitedAtDispose;
                    if (_process == null) return false;
                    try { return _process.HasExited; }
                    catch (InvalidOperationException) { return true; }
                }
            }
        }

        public void Start()
        {
            lock (_gate)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ProcessUciTransport));
                if (_process != null) throw new InvalidOperationException("Transport already started.");

                var process = new Process();
                try
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = _executablePath,
                        WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(_executablePath)) ?? string.Empty,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardInputEncoding = new UTF8Encoding(false)
                    };
                    process.OutputDataReceived += OnOutputData;
                    process.ErrorDataReceived += (s, e) => { }; // drain so the engine never blocks on stderr
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                catch (Exception ex)
                {
                    try { process.Dispose(); } catch (Exception) { }
                    throw new ChessAIException("Cannot start Stockfish: " + _executablePath, ex);
                }
                _process = process;
            }
        }

        public void SendLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            Process process;
            lock (_gate)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ProcessUciTransport));
                if (_process == null) throw new InvalidOperationException("Transport not started.");
                process = _process;
            }

            try
            {
                process.StandardInput.WriteLine(line);
                process.StandardInput.Flush();
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is ObjectDisposedException)
            {
                throw new ChessAIException("Cannot write to Stockfish.", ex);
            }
        }

        public async Task<string> ReadLineAsync(TimeSpan timeout, CancellationToken ct)
        {
            if (!await _signal.WaitAsync(timeout, ct).ConfigureAwait(false))
                throw new TimeoutException("No output from Stockfish within " + timeout.TotalMilliseconds + " ms.");

            if (!_lines.TryDequeue(out string line)) return null;
            if (line == null)
            {
                // EOF is sticky: later reads see it too.
                _lines.Enqueue(null);
                _signal.Release();
            }
            return line;
        }

        public void Dispose()
        {
            Process process;
            lock (_gate)
            {
                if (_disposed) return;
                _disposed = true;
                process = _process;
                _process = null;
            }
            if (process == null) return;

            try
            {
                if (!process.HasExited) process.Kill();
            }
            catch (Exception) { /* already exited or not killable */ }

            try { _exitedAtDispose = process.WaitForExit(2000) || process.HasExited; }
            catch (Exception) { _exitedAtDispose = true; }

            try { process.Dispose(); } catch (Exception) { }
        }

        private void OnOutputData(object sender, DataReceivedEventArgs e)
        {
            _lines.Enqueue(e.Data); // null Data means EOF
            _signal.Release();
        }
    }
}
