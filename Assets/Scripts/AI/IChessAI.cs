using System;
using System.Threading;
using System.Threading.Tasks;
using Chess.Core;

namespace Chess.AI
{
    public interface IChessAI : IDisposable
    {
        bool IsReady { get; }

        /// <summary>Idempotent; concurrent callers share one initialization.</summary>
        Task InitializeAsync(CancellationToken ct);

        /// <summary>Returns a legal move (with Flags) for the position.</summary>
        Task<Move> GetBestMoveAsync(string fen, CancellationToken ct);
    }
}
