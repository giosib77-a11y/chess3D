using System;
using System.Collections.Generic;
using System.Globalization;

namespace Chess.AI
{
    /// <summary>An option advertised by the engine in response to "uci".</summary>
    public sealed class UciOption
    {
        public UciOption(string name, string type, string defaultValue, int? min, int? max)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Min = min;
            Max = max;
        }

        public string Name { get; }
        public string Type { get; }
        public string DefaultValue { get; }
        public int? Min { get; }
        public int? Max { get; }
    }

    /// <summary>Pure UCI command building and response parsing (no I/O).</summary>
    public static class UciProtocol
    {
        public const string Uci = "uci";
        public const string UciOk = "uciok";
        public const string IsReady = "isready";
        public const string ReadyOk = "readyok";
        public const string UciNewGame = "ucinewgame";
        public const string Stop = "stop";
        public const string Quit = "quit";

        private const string SkillLevelOption = "Skill Level";
        private const string LimitStrengthOption = "UCI_LimitStrength";
        private const string EloOption = "UCI_Elo";

        public static string BuildSetOption(string name, string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            return "setoption name " + name + " value " + value;
        }

        public static IReadOnlyList<string> BuildDifficultyCommands(
            AIDifficulty difficulty, IReadOnlyDictionary<string, UciOption> engineOptions)
        {
            if (difficulty == null) throw new ArgumentNullException(nameof(difficulty));
            if (engineOptions == null) throw new ArgumentNullException(nameof(engineOptions));

            var commands = new List<string>();

            if (engineOptions.TryGetValue(SkillLevelOption, out UciOption skill))
                commands.Add(BuildSetOption(SkillLevelOption, Clamp(difficulty.SkillLevel, skill).ToString(CultureInfo.InvariantCulture)));

            if (difficulty.LimitStrength)
            {
                if (engineOptions.ContainsKey(LimitStrengthOption))
                    commands.Add(BuildSetOption(LimitStrengthOption, "true"));
                if (engineOptions.TryGetValue(EloOption, out UciOption elo))
                    commands.Add(BuildSetOption(EloOption, Clamp(difficulty.Elo.Value, elo).ToString(CultureInfo.InvariantCulture)));
            }
            else if (engineOptions.ContainsKey(LimitStrengthOption))
            {
                commands.Add(BuildSetOption(LimitStrengthOption, "false"));
            }

            return commands;
        }

        public static string BuildPosition(string fen)
        {
            if (fen == null) throw new ArgumentNullException(nameof(fen));
            return "position fen " + fen;
        }

        public static string BuildGo(AIDifficulty difficulty)
        {
            if (difficulty == null) throw new ArgumentNullException(nameof(difficulty));
            return difficulty.Depth.HasValue
                ? "go depth " + difficulty.Depth.Value.ToString(CultureInfo.InvariantCulture)
                : "go movetime " + difficulty.MoveTimeMs.ToString(CultureInfo.InvariantCulture);
        }

        public static bool TryParseOption(string line, out UciOption option)
        {
            option = null;
            if (string.IsNullOrWhiteSpace(line)) return false;

            string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 5 || tokens[0] != "option" || tokens[1] != "name") return false;

            int typeIdx = Array.IndexOf(tokens, "type", 2);
            if (typeIdx < 3 || typeIdx + 1 >= tokens.Length) return false;

            string name = string.Join(" ", tokens, 2, typeIdx - 2);
            string type = tokens[typeIdx + 1];

            string defaultValue = null;
            int? min = null;
            int? max = null;

            // Keywords after the type; "default" value may contain spaces, so collect until the next keyword.
            int i = typeIdx + 2;
            while (i < tokens.Length)
            {
                string key = tokens[i];
                if (key == "default" || key == "min" || key == "max" || key == "var")
                {
                    int j = i + 1;
                    while (j < tokens.Length && !IsKeyword(tokens[j])) j++;
                    string value = string.Join(" ", tokens, i + 1, j - i - 1);
                    if (key == "default") defaultValue = value;
                    else if (key == "min" && type == "spin" && TryInt(value, out int mn)) min = mn;
                    else if (key == "max" && type == "spin" && TryInt(value, out int mx)) max = mx;
                    i = j;
                }
                else
                {
                    i++;
                }
            }

            option = new UciOption(name, type, defaultValue, min, max);
            return true;
        }

        public static bool TryParseBestMove(string line, out string uciMove)
        {
            uciMove = null;
            if (string.IsNullOrWhiteSpace(line)) return false;

            string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2 || tokens[0] != "bestmove") return false;

            uciMove = tokens[1] == "(none)" ? null : tokens[1];
            return true;
        }

        private static bool IsKeyword(string token)
        {
            return token == "default" || token == "min" || token == "max" || token == "var";
        }

        private static bool TryInt(string s, out int value)
        {
            return int.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
        }

        private static int Clamp(int value, UciOption option)
        {
            if (option.Min.HasValue && value < option.Min.Value) value = option.Min.Value;
            if (option.Max.HasValue && value > option.Max.Value) value = option.Max.Value;
            return value;
        }
    }
}
