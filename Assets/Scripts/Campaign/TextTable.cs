using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Chess.Campaign
{
    // Language tables. Data lives in TextTable.Georgian.cs / TextTable.English.cs (explicit TextId keys).
    public sealed partial class TextTable
    {
        private static readonly TextTable GeorgianTable;
        private static readonly TextTable EnglishTable;
        private static readonly string Atlas;

        private readonly string _languageCode;
        private readonly string[] _texts;

        // A single static constructor keeps initialization order independent of the partial files.
        static TextTable()
        {
            var sb = new StringBuilder(143);
            for (char c = ' '; c <= '~'; c++) sb.Append(c);
            for (int code = 0x10D0; code <= 0x10FF; code++) sb.Append((char)code);
            Atlas = sb.ToString();

            GeorgianTable = new TextTable("ka", BuildGeorgian());
            EnglishTable = new TextTable("en", BuildEnglish());
        }

        private TextTable(string languageCode, Dictionary<TextId, string> entries)
        {
            _languageCode = languageCode;
            var ids = (TextId[])Enum.GetValues(typeof(TextId));
            _texts = new string[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                if (!entries.TryGetValue(ids[i], out string text) || string.IsNullOrEmpty(text))
                    throw new InvalidOperationException(
                        "Text table '" + languageCode + "' is missing or has an empty entry for " + ids[i] + ".");
                _texts[(int)ids[i]] = text;
            }
        }

        public static TextTable Georgian { get { return GeorgianTable; } }
        public static TextTable English { get { return EnglishTable; } }
        public static TextTable Active { get { return GeorgianTable; } }

        public string LanguageCode { get { return _languageCode; } }

        public string Get(TextId id)
        {
            int index = (int)id;
            if (index < 0 || index >= _texts.Length || !Enum.IsDefined(typeof(TextId), id))
                throw new ArgumentOutOfRangeException(nameof(id), id, "Undefined TextId.");
            return _texts[index];
        }

        public string Format(TextId id, params object[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            return string.Format(CultureInfo.InvariantCulture, Get(id), args);
        }

        // U+0020..U+007E, then U+10D0..U+10FF (95 + 48 = 143 chars).
        public static string AtlasCharacters { get { return Atlas; } }

        public static bool IsSupportedChar(char c)
        {
            return c == '\n' || Atlas.IndexOf(c) >= 0;
        }
    }
}
