using Chess.AI;
using Chess.Core;
using Xunit;

namespace ChessCore.Tests.AI
{
    public class StockfishAITests
    {
        private const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string CastlingFen = "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1";
        private const string MatedFen = "rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3";

        private const string SkillOption = "option name Skill Level type spin default 20 min 0 max 20";
        private const string LimitOption = "option name UCI_LimitStrength type check default false";
        private const string EloOption = "option name UCI_Elo type spin default 1320 min 1320 max 3190";

        private static StockfishAI Create(FakeUciTransport fake, DifficultyPreset preset = DifficultyPreset.Easy)
        {
            return new StockfishAI(fake, AIDifficulty.FromPreset(preset))
            {
                HandshakeTimeout = TimeSpan.FromMilliseconds(500),
                SearchTimeout = TimeSpan.FromMilliseconds(500)
            };
        }

        private static async Task WaitUntil(Func<bool> condition)
        {
            for (int i = 0; i < 400 && !condition(); i++) await Task.Delay(5);
            Assert.True(condition(), "condition not met in time");
        }

        [Fact]
        public async Task Initialize_PerformsHandshakeInOrder()
        {
            var fake = FakeUciTransport.CreateEngine(SkillOption, LimitOption);
            using var ai = Create(fake);

            Assert.False(ai.IsReady);
            await ai.InitializeAsync(CancellationToken.None);

            Assert.True(ai.IsReady);
            Assert.Equal(1, fake.StartCount);
            Assert.Equal(
                new[] { "uci", "setoption name Skill Level value 0", "setoption name UCI_LimitStrength value false", "isready" },
                fake.Sent.ToArray());
        }

        [Fact]
        public async Task Initialize_Medium_SendsStrengthLimitOptionsClamped()
        {
            var fake = FakeUciTransport.CreateEngine(SkillOption, LimitOption, EloOption);
            using var ai = Create(fake, DifficultyPreset.Medium);

            await ai.InitializeAsync(CancellationToken.None);

            Assert.Equal(
                new[]
                {
                    "uci", "setoption name Skill Level value 20", "setoption name UCI_LimitStrength value true",
                    "setoption name UCI_Elo value 1400", "isready"
                },
                fake.Sent.ToArray());
        }

        [Fact]
        public async Task Initialize_DoesNotSendUnadvertisedOptions()
        {
            var fake = FakeUciTransport.CreateEngine();
            using var ai = Create(fake);

            await ai.InitializeAsync(CancellationToken.None);

            Assert.Equal(new[] { "uci", "isready" }, fake.Sent.ToArray());
        }

        [Fact]
        public async Task Initialize_IsIdempotentAndShared()
        {
            var fake = FakeUciTransport.CreateEngine(SkillOption);
            using var ai = Create(fake);

            Task a = ai.InitializeAsync(CancellationToken.None);
            Task b = ai.InitializeAsync(CancellationToken.None);
            await Task.WhenAll(a, b);
            await ai.InitializeAsync(CancellationToken.None);

            Assert.Equal(1, fake.Sent.Count(l => l == "uci"));
            Assert.Equal(1, fake.StartCount);
        }

        [Fact]
        public async Task Initialize_StartFailure_ThrowsChessAIExceptionAndFaults()
        {
            var inner = new InvalidOperationException("boom");
            var fake = FakeUciTransport.CreateEngine();
            fake.StartException = inner;
            using var ai = Create(fake);

            var ex = await Assert.ThrowsAsync<ChessAIException>(() => ai.InitializeAsync(CancellationToken.None));
            Assert.Same(inner, ex.InnerException);
            await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
        }

        [Fact]
        public async Task Initialize_NoUciOk_TimesOut()
        {
            var fake = new FakeUciTransport();
            using var ai = Create(fake);
            ai.HandshakeTimeout = TimeSpan.FromMilliseconds(50);

            var ex = await Assert.ThrowsAsync<ChessAIException>(() => ai.InitializeAsync(CancellationToken.None));
            Assert.IsType<TimeoutException>(ex.InnerException);
            Assert.False(ai.IsReady);
        }

        [Fact]
        public async Task Initialize_EofDuringHandshake_ThrowsChessAIException()
        {
            var fake = new FakeUciTransport().When("uci", "option name Foo type check default false");
            fake.Eof();
            using var ai = Create(fake);

            await Assert.ThrowsAsync<ChessAIException>(() => ai.InitializeAsync(CancellationToken.None));
        }

        [Fact]
        public async Task GetBestMove_Easy_SendsPositionAndGoDepth()
        {
            var fake = FakeUciTransport.CreateEngine(SkillOption).When("go", "info depth 1 score cp 20", "bestmove e2e4 ponder e7e5");
            using var ai = Create(fake);

            Move move = await ai.GetBestMoveAsync(StartFen, CancellationToken.None);

            Assert.Equal("e2e4", move.ToUci());
            Assert.Equal(
                new[] { "uci", "setoption name Skill Level value 0", "isready", "position fen " + StartFen, "go depth 1" },
                fake.Sent.ToArray());
        }

        [Fact]
        public async Task GetBestMove_Medium_SendsGoMovetime()
        {
            var fake = FakeUciTransport.CreateEngine().When("go", "bestmove e2e4");
            using var ai = Create(fake, DifficultyPreset.Medium);

            await ai.GetBestMoveAsync(StartFen, CancellationToken.None);

            Assert.Contains("go movetime 300", fake.Sent);
        }

        [Fact]
        public async Task GetBestMove_Castling_ReturnsMoveWithFlags()
        {
            var fake = FakeUciTransport.CreateEngine().When("go", "bestmove e1g1");
            using var ai = Create(fake);

            Move move = await ai.GetBestMoveAsync(CastlingFen, CancellationToken.None);

            Assert.Equal("e1g1", move.ToUci());
            Assert.True((move.Flags & MoveFlags.CastleKingside) != 0);
        }

        [Fact]
        public async Task GetBestMove_CanBeCalledRepeatedly()
        {
            var fake = FakeUciTransport.CreateEngine().When("go", "bestmove e2e4");
            using var ai = Create(fake);

            await ai.GetBestMoveAsync(StartFen, CancellationToken.None);
            await ai.GetBestMoveAsync(StartFen, CancellationToken.None);

            Assert.Equal(1, fake.Sent.Count(l => l == "uci"));
            Assert.Equal(2, fake.Sent.Count(l => l == "go depth 1"));
        }

        [Fact]
        public async Task GetBestMove_IllegalMove_ThrowsChessAIExceptionAndFaults()
        {
            var fake = FakeUciTransport.CreateEngine().When("go", "bestmove e2e5");
            using var ai = Create(fake);

            await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
            await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
        }

        [Fact]
        public async Task GetBestMove_Garbage_ThrowsChessAIException()
        {
            var fake = FakeUciTransport.CreateEngine().When("go", "bestmove zzzz");
            using var ai = Create(fake);

            await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
        }

        [Fact]
        public async Task GetBestMove_BestmoveNone_ThrowsChessAIException()
        {
            var fake = FakeUciTransport.CreateEngine().When("go", "bestmove (none)");
            using var ai = Create(fake);

            await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
        }

        [Fact]
        public async Task GetBestMove_Timeout_ThrowsWithTimeoutInnerAndFaults()
        {
            var fake = FakeUciTransport.CreateEngine();
            using var ai = Create(fake);
            ai.SearchTimeout = TimeSpan.FromMilliseconds(50);

            var ex = await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
            Assert.IsType<TimeoutException>(ex.InnerException);

            await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
        }

        [Fact]
        public async Task GetBestMove_EofDuringSearch_ThrowsChessAIException()
        {
            var fake = FakeUciTransport.CreateEngine().When("go", "info depth 1");
            using var ai = Create(fake);
            await ai.InitializeAsync(CancellationToken.None);
            fake.Eof();

            await Assert.ThrowsAsync<ChessAIException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
        }

        [Fact]
        public async Task GetBestMove_Cancel_SendsStopAndThrowsOperationCanceled()
        {
            var fake = FakeUciTransport.CreateEngine().When("stop", "bestmove e2e4");
            using var ai = Create(fake);
            ai.SearchTimeout = TimeSpan.FromSeconds(30);
            using var cts = new CancellationTokenSource();

            Task<Move> task = ai.GetBestMoveAsync(StartFen, cts.Token);
            await WaitUntil(() => fake.WasSent("go depth 1"));
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
            Assert.True(fake.WasSent("stop"));
        }

        [Fact]
        public async Task GetBestMove_Cancel_DiscardsStaleBestmoveSoNextSearchIsClean()
        {
            var fake = FakeUciTransport.CreateEngine().When("stop", "bestmove a2a3");
            using var ai = Create(fake);
            using var cts = new CancellationTokenSource();

            Task<Move> task = ai.GetBestMoveAsync(StartFen, cts.Token);
            await WaitUntil(() => fake.WasSent("go depth 1"));
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);

            fake.When("go", "bestmove e2e4");
            Move move = await ai.GetBestMoveAsync(StartFen, CancellationToken.None);
            Assert.Equal("e2e4", move.ToUci());
        }

        [Fact]
        public async Task GetBestMove_AlreadyCancelled_ThrowsWithoutTouchingEngine()
        {
            var fake = FakeUciTransport.CreateEngine();
            using var ai = Create(fake);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ai.GetBestMoveAsync(StartFen, cts.Token));
            Assert.Empty(fake.Sent);
        }

        [Fact]
        public async Task GetBestMove_Concurrent_ThrowsInvalidOperation()
        {
            var fake = FakeUciTransport.CreateEngine().When("stop", "bestmove e2e4");
            using var ai = Create(fake);
            ai.SearchTimeout = TimeSpan.FromSeconds(30);
            using var cts = new CancellationTokenSource();

            Task<Move> first = ai.GetBestMoveAsync(StartFen, cts.Token);
            await WaitUntil(() => fake.WasSent("go depth 1"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));

            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
        }

        [Fact]
        public async Task GetBestMove_BadFen_ThrowsFormatException()
        {
            var fake = FakeUciTransport.CreateEngine();
            using var ai = Create(fake);

            await Assert.ThrowsAsync<FormatException>(() => ai.GetBestMoveAsync("not a fen", CancellationToken.None));
            Assert.Empty(fake.Sent);
        }

        [Fact]
        public async Task GetBestMove_GameOverFen_ThrowsInvalidOperation()
        {
            var fake = FakeUciTransport.CreateEngine();
            using var ai = Create(fake);

            await Assert.ThrowsAsync<InvalidOperationException>(() => ai.GetBestMoveAsync(MatedFen, CancellationToken.None));
            Assert.Empty(fake.Sent);
        }

        [Fact]
        public async Task GetBestMove_NullFen_ThrowsArgumentNull()
        {
            var fake = FakeUciTransport.CreateEngine();
            using var ai = Create(fake);

            await Assert.ThrowsAsync<ArgumentNullException>(() => ai.GetBestMoveAsync(null, CancellationToken.None));
        }

        [Fact]
        public async Task Dispose_SendsQuitDisposesTransportAndIsIdempotent()
        {
            var fake = FakeUciTransport.CreateEngine();
            var ai = Create(fake);
            await ai.InitializeAsync(CancellationToken.None);

            ai.Dispose();
            ai.Dispose();

            Assert.Equal(1, fake.Sent.Count(l => l == "quit"));
            Assert.Equal(1, fake.DisposeCount);
        }

        [Fact]
        public async Task CallsAfterDispose_ThrowObjectDisposed()
        {
            var fake = FakeUciTransport.CreateEngine();
            var ai = Create(fake);
            await ai.InitializeAsync(CancellationToken.None);
            ai.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => ai.GetBestMoveAsync(StartFen, CancellationToken.None));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => ai.InitializeAsync(CancellationToken.None));
        }

        [Fact]
        public void Dispose_WithoutUse_DoesNotSendQuitButDisposesTransport()
        {
            var fake = FakeUciTransport.CreateEngine();
            Create(fake).Dispose();

            Assert.Empty(fake.Sent);
            Assert.Equal(1, fake.DisposeCount);
        }

        [Fact]
        public void Constructor_MissingExecutable_ThrowsStockfishNotFound()
        {
            string path = Path.Combine(Path.GetTempPath(), "definitely-missing-" + Guid.NewGuid().ToString("N"), "stockfish.exe");

            var ex = Assert.Throws<StockfishNotFoundException>(() => new StockfishAI(path, AIDifficulty.FromPreset(DifficultyPreset.Easy)));
            Assert.Equal(path, ex.Path);
        }

        [Fact]
        public void Constructor_Nulls_ThrowArgumentNull()
        {
            AIDifficulty easy = AIDifficulty.FromPreset(DifficultyPreset.Easy);
            Assert.Throws<ArgumentNullException>(() => new StockfishAI((string)null, easy));
            Assert.Throws<ArgumentNullException>(() => new StockfishAI((IUciTransport)null, easy));
            Assert.Throws<ArgumentNullException>(() => new StockfishAI(new FakeUciTransport(), null));
        }

        [Fact]
        public void Timeouts_HaveContractDefaultsAndRejectNonPositive()
        {
            using var ai = new StockfishAI(new FakeUciTransport(), AIDifficulty.FromPreset(DifficultyPreset.Easy));

            Assert.Equal(TimeSpan.FromSeconds(10), ai.HandshakeTimeout);
            Assert.Equal(TimeSpan.FromSeconds(30), ai.SearchTimeout);
            Assert.Throws<ArgumentOutOfRangeException>(() => ai.SearchTimeout = TimeSpan.Zero);
        }

        [Fact]
        public void ProcessTransport_MissingExecutable_StartThrowsChessAIExceptionWithInner()
        {
            string path = Path.Combine(Path.GetTempPath(), "definitely-missing-" + Guid.NewGuid().ToString("N"), "stockfish.exe");
            using var transport = new ProcessUciTransport(path);

            var ex = Assert.Throws<ChessAIException>(() => transport.Start());
            Assert.NotNull(ex.InnerException);
        }
    }
}
