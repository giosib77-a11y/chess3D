using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.AI
{
    /// <summary>Line-based channel to a UCI engine process.</summary>
    public interface IUciTransport : IDisposable
    {
        /// <summary>Starts the engine. Throws ChessAIException if the process cannot start.</summary>
        void Start();

        void SendLine(string line);

        /// <summary>Returns null at EOF; throws TimeoutException on timeout.</summary>
        Task<string> ReadLineAsync(TimeSpan timeout, CancellationToken ct);

        bool HasExited { get; }
    }
}
