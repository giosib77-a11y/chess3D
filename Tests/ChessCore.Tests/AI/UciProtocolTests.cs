using Chess.AI;
using Xunit;

namespace ChessCore.Tests.AI
{
    public class UciProtocolTests
    {
        private static Dictionary<string, UciOption> Options(params UciOption[] options)
        {
            var d = new Dictionary<string, UciOption>();
            foreach (var o in options) d[o.Name] = o;
            return d;
        }

        private static readonly UciOption Skill = new UciOption("Skill Level", "spin", "20", 0, 20);
        private static readonly UciOption Limit = new UciOption("UCI_LimitStrength", "check", "false", null, null);
        private static readonly UciOption Elo = new UciOption("UCI_Elo", "spin", "1320", 1320, 3190);

        [Theory]
        [InlineData("bestmove e2e4 ponder e7e5", "e2e4")]
        [InlineData("bestmove e7e8q", "e7e8q")]
        [InlineData("bestmove (none)", null)]
        public void TryParseBestMove_Valid(string line, string expected)
        {
            Assert.True(UciProtocol.TryParseBestMove(line, out string move));
            Assert.Equal(expected, move);
        }

        [Theory]
        [InlineData("info depth 5 score cp 20 pv e2e4")]
        [InlineData("bestmove")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void TryParseBestMove_Invalid(string line)
        {
            Assert.False(UciProtocol.TryParseBestMove(line, out string move));
            Assert.Null(move);
        }

        [Fact]
        public void TryParseOption_Spin()
        {
            Assert.True(UciProtocol.TryParseOption("option name Hash type spin default 16 min 1 max 33554432", out var o));
            Assert.Equal("Hash", o.Name);
            Assert.Equal("spin", o.Type);
            Assert.Equal("16", o.DefaultValue);
            Assert.Equal(1, o.Min);
            Assert.Equal(33554432, o.Max);
        }

        [Fact]
        public void TryParseOption_NameWithSpace()
        {
            Assert.True(UciProtocol.TryParseOption("option name Skill Level type spin default 20 min 0 max 20", out var o));
            Assert.Equal("Skill Level", o.Name);
            Assert.Equal(0, o.Min);
            Assert.Equal(20, o.Max);
        }

        [Fact]
        public void TryParseOption_RealStockfishElo()
        {
            Assert.True(UciProtocol.TryParseOption("option name UCI_Elo type spin default 1320 min 1320 max 3190", out var o));
            Assert.Equal("UCI_Elo", o.Name);
            Assert.Equal(1320, o.Min);
            Assert.Equal(3190, o.Max);
        }

        [Fact]
        public void TryParseOption_Check_NoMinMax()
        {
            Assert.True(UciProtocol.TryParseOption("option name Ponder type check default false", out var o));
            Assert.Equal("check", o.Type);
            Assert.Equal("false", o.DefaultValue);
            Assert.Null(o.Min);
            Assert.Null(o.Max);
        }

        [Fact]
        public void TryParseOption_ComboWithVars_NoMinMax()
        {
            Assert.True(UciProtocol.TryParseOption("option name Style type combo default Normal var Solid var Normal var Risky", out var o));
            Assert.Equal("combo", o.Type);
            Assert.Equal("Normal", o.DefaultValue);
            Assert.Null(o.Min);
        }

        [Fact]
        public void TryParseOption_StringAndButton()
        {
            Assert.True(UciProtocol.TryParseOption("option name EvalFile type string default nn.nnue", out var s));
            Assert.Equal("nn.nnue", s.DefaultValue);
            Assert.True(UciProtocol.TryParseOption("option name Clear Hash type button", out var b));
            Assert.Equal("Clear Hash", b.Name);
            Assert.Null(b.DefaultValue);
        }

        [Theory]
        [InlineData("uciok")]
        [InlineData("id name Stockfish 16")]
        [InlineData("option name X")]
        [InlineData("option name type spin")]
        [InlineData("")]
        [InlineData(null)]
        public void TryParseOption_Invalid(string line)
        {
            Assert.False(UciProtocol.TryParseOption(line, out var o));
            Assert.Null(o);
        }

        [Fact]
        public void TryParseOption_GarbageMin_TreatedAsNull()
        {
            Assert.True(UciProtocol.TryParseOption("option name X type spin default 1 min abc max 5", out var o));
            Assert.Null(o.Min);
            Assert.Equal(5, o.Max);
        }

        [Fact]
        public void BuildDifficultyCommands_LimitStrength_ClampsElo()
        {
            var d = new AIDifficulty(20, null, 300, 800);
            var cmds = UciProtocol.BuildDifficultyCommands(d, Options(Skill, Limit, Elo));
            Assert.Equal(new[]
            {
                "setoption name Skill Level value 20",
                "setoption name UCI_LimitStrength value true",
                "setoption name UCI_Elo value 1320"
            }, cmds);
        }

        [Fact]
        public void BuildDifficultyCommands_EloClampedToMax()
        {
            var d = new AIDifficulty(20, null, 300, 3400);
            var cmds = UciProtocol.BuildDifficultyCommands(d, Options(Skill, Limit, Elo));
            Assert.Equal("setoption name UCI_Elo value 3190", cmds[2]);
        }

        [Fact]
        public void BuildDifficultyCommands_SkillClamped()
        {
            var d = new AIDifficulty(20, null, 300, null);
            var narrow = new UciOption("Skill Level", "spin", "10", 0, 10);
            var cmds = UciProtocol.BuildDifficultyCommands(d, Options(narrow));
            Assert.Equal(new[] { "setoption name Skill Level value 10" }, cmds);
        }

        [Fact]
        public void BuildDifficultyCommands_NoLimit_SendsLimitFalse()
        {
            var d = AIDifficulty.FromPreset(DifficultyPreset.Easy);
            var cmds = UciProtocol.BuildDifficultyCommands(d, Options(Skill, Limit, Elo));
            Assert.Equal(new[]
            {
                "setoption name Skill Level value 0",
                "setoption name UCI_LimitStrength value false"
            }, cmds);
        }

        [Fact]
        public void BuildDifficultyCommands_SkipsUnadvertised()
        {
            var d = AIDifficulty.FromPreset(DifficultyPreset.Medium);
            Assert.Empty(UciProtocol.BuildDifficultyCommands(d, Options()));
            var onlyElo = UciProtocol.BuildDifficultyCommands(d, Options(Elo));
            Assert.Equal(new[] { "setoption name UCI_Elo value 1400" }, onlyElo);
            var easy = AIDifficulty.FromPreset(DifficultyPreset.Easy);
            Assert.Empty(UciProtocol.BuildDifficultyCommands(easy, Options(Elo)));
        }

        [Fact]
        public void BuildDifficultyCommands_NullArgs_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => UciProtocol.BuildDifficultyCommands(null, Options()));
            Assert.Throws<ArgumentNullException>(() => UciProtocol.BuildDifficultyCommands(AIDifficulty.FromPreset(DifficultyPreset.Easy), null));
        }

        [Fact]
        public void BuildGo_DepthOrMoveTime()
        {
            Assert.Equal("go depth 1", UciProtocol.BuildGo(AIDifficulty.FromPreset(DifficultyPreset.Easy)));
            Assert.Equal("go movetime 500", UciProtocol.BuildGo(AIDifficulty.FromPreset(DifficultyPreset.Hard)));
        }

        [Fact]
        public void BuildPosition_And_SetOption()
        {
            const string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            Assert.Equal("position fen " + fen, UciProtocol.BuildPosition(fen));
            Assert.Equal("setoption name Skill Level value 5", UciProtocol.BuildSetOption("Skill Level", "5"));
            Assert.Throws<ArgumentNullException>(() => UciProtocol.BuildPosition(null));
            Assert.Throws<ArgumentNullException>(() => UciProtocol.BuildSetOption(null, "1"));
        }

        [Fact]
        public void Exceptions_Messages()
        {
            var e = new StockfishNotFoundException("C:\\x\\sf.exe");
            Assert.Equal("Stockfish executable not found: C:\\x\\sf.exe", e.Message);
            Assert.Equal("C:\\x\\sf.exe", e.Path);
            Assert.IsAssignableFrom<ChessAIException>(e);
        }
    }
}
