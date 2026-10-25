using Chess.AI;
using Chess.Core;
using Xunit;
using Xunit.Abstractions;

namespace ChessCore.Tests.AI
{
    public class StockfishIntegrationTests
    {
        private const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private readonly ITestOutputHelper _output;

        public StockfishIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task BestMoveFromStartPosition_RequiresStockfish()
        {
            string path = Environment.GetEnvironmentVariable("STOCKFISH_PATH");
            if (string.IsNullOrWhiteSpace(path))
            {
                _output.WriteLine("SKIPPED: STOCKFISH_PATH not set");
                return;
            }

            var transport = new ProcessUciTransport(path);
            var ai = new StockfishAI(transport, AIDifficulty.FromPreset(DifficultyPreset.Easy));
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                Move move = await ai.GetBestMoveAsync(StartFen, cts.Token);

                Assert.True(new GameState(StartFen).TryResolveUciMove(move.ToUci(), out _));
            }
            finally
            {
                ai.Dispose();
            }

            Assert.True(transport.HasExited);
        }
    }
}
