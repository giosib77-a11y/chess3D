using System;

namespace Chess.AI
{
    /// <summary>Base exception for AI engine failures (cannot start, crashed, timed out, bad output).</summary>
    public class ChessAIException : Exception
    {
        public ChessAIException(string message) : base(message) { }

        public ChessAIException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>Thrown when the Stockfish executable does not exist at the configured path.</summary>
    public sealed class StockfishNotFoundException : ChessAIException
    {
        public StockfishNotFoundException(string path)
            : base("Stockfish executable not found: " + path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
