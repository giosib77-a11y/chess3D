using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Chess.Campaign;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class TextTableTests
    {
        // Contract table (PROJECT.md): id, Georgian, English. Two ordinary spaces where the contract has two.
        private static readonly (TextId Id, string Ka, string En)[] Rows =
        {
            (TextId.MenuTitle, "ქართული საბრძოლო ჭადრაკი", "Georgian Battle Chess"),
            (TextId.MenuNewCampaign, "ახალი კამპანია", "New Campaign"),
            (TextId.MenuContinue, "გაგრძელება", "Continue"),
            (TextId.MenuQuickGame, "სწრაფი თამაში", "Quick Game"),
            (TextId.MenuQuit, "გასვლა", "Quit"),
            (TextId.ConfirmNewCampaign, "დავიწყოთ ახალი კამპანია? მიმდინარე პროგრესი დაიკარგება.", "Start a new campaign? Current progress will be lost."),
            (TextId.ConfirmYes, "ახლის დაწყება", "Start new"),
            (TextId.Cancel, "გაუქმება", "Cancel"),
            (TextId.QuickGameTitle, "სწრაფი თამაში", "Quick Game"),
            (TextId.QuickColorFormat, "ფერი: {0}", "Color: {0}"),
            (TextId.QuickDifficultyFormat, "სირთულე: {0}", "Difficulty: {0}"),
            (TextId.QuickStart, "დაწყება", "Start"),
            (TextId.Back, "უკან", "Back"),
            (TextId.SideWhite, "თეთრი", "White"),
            (TextId.SideBlack, "შავი", "Black"),
            (TextId.DifficultyEasy, "მარტივი", "Easy"),
            (TextId.DifficultyMedium, "საშუალო", "Medium"),
            (TextId.DifficultyHard, "რთული", "Hard"),
            (TextId.DifficultyMaster, "ოსტატი", "Master"),
            (TextId.SaveFailedWarning, "გაფრთხილება: პროგრესი ვერ შეინახა.", "Warning: progress could not be saved."),
            (TextId.CorruptedSaveWarning, "შენახული კამპანია დაზიანებულია და ვერ ჩაიტვირთა. ფაილი შენახულია სახელით campaign.corrupt.json. დაიწყე ახალი კამპანია.", "The saved campaign was damaged and could not be loaded. It was kept as campaign.corrupt.json. Start a new campaign."),
            (TextId.MapTitle, "კამპანიის რუკა", "Campaign Map"),
            (TextId.StateLocked, "ჩაკეტილია", "Locked"),
            (TextId.StateAvailable, "ხელმისაწვდომია", "Available"),
            (TextId.StateLiberated, "გათავისუფლებულია", "Liberated"),
            (TextId.RegionLabelFormat, "{0}\n{1}\nElo {2}\n{3}", "{0}\n{1}\nElo {2}\n{3}"),
            (TextId.MapStatusFormat, "გათავისუფლებულია: {0} / {1}. აირჩიე რეგიონი ბრძოლისთვის.", "Liberated {0} of {1}. Choose a region to fight for."),
            (TextId.AllLiberated, "ყველა რეგიონი გათავისუფლებულია! საქართველო თავისუფალია.", "All regions are liberated! Georgia is free."),
            (TextId.BackToMenu, "მენიუში დაბრუნება", "Back to menu"),
            (TextId.RegionKartliName, "ქართლი", "Kartli"),
            (TextId.RegionKartliEra, "V საუკუნე - მეფე ვახტანგ გორგასალი", "5th century - King Vakhtang Gorgasali"),
            (TextId.RegionKakhetiName, "კახეთი", "Kakheti"),
            (TextId.RegionKakhetiEra, "XVI საუკუნე - მეფე ლევანი", "16th century - King Levan"),
            (TextId.RegionImeretiName, "იმერეთი", "Imereti"),
            (TextId.RegionImeretiEra, "XVII საუკუნე - მეფე ალექსანდრე III", "17th century - King Alexander III"),
            (TextId.RegionSvanetiName, "სვანეთი", "Svaneti"),
            (TextId.RegionSvanetiEra, "XII საუკუნე - მეფე დავით აღმაშენებელი", "12th century - King David the Builder"),
            (TextId.RegionKhevsuretiName, "ხევსურეთი", "Khevsureti"),
            (TextId.RegionKhevsuretiEra, "XVIII საუკუნე - მეფე ერეკლე II", "18th century - King Erekle II"),
            (TextId.BattleInfoFormat, "{0} - {1} - მოწინააღმდეგის Elo {2}", "{0} - {1} - enemy Elo {2}"),
            (TextId.BattleInfoUnknown, "კამპანია", "Campaign"),
            (TextId.BackToMap, "რუკაზე დაბრუნება", "Back to map"),
            (TextId.Retry, "თავიდან ცდა", "Retry"),
            (TextId.PlayAgain, "ხელახლა თამაში", "Play again"),
            (TextId.QuickGameInfoFormat, "სწრაფი თამაში - {0} - შენი ფერი: {1}", "Quick Game - {0} - you play {1}"),
            (TextId.TitleVictory, "გამარჯვება!", "Victory!"),
            (TextId.TitleDefeat, "მარცხი", "Defeat"),
            (TextId.TitleDraw, "ფრე", "Draw"),
            (TextId.TitleNotCounted, "თამაში დასრულდა", "Game over"),
            (TextId.NotCountedMessage, "AI მოწინააღმდეგე არ იყო აქტიური, ამიტომ ეს შედეგი არ ითვლება.", "The AI opponent was not active, so this result does not count."),
            (TextId.LiberatedFormat, "{0} გათავისუფლდა!", "{0} is liberated!"),
            (TextId.NextOpenFormat, "{0} ახლა ხელმისაწვდომია.", "{0} is now open."),
            (TextId.RemainsFreeFormat, "{0} კვლავ თავისუფალია.", "{0} remains free."),
            (TextId.EnemyControlFormat, "{0} ჯერ კიდევ მტრის ხელშია.", "{0} is still under enemy control."),
            (TextId.WhiteToMove, "თეთრის სვლაა", "White to move"),
            (TextId.BlackToMove, "შავის სვლაა", "Black to move"),
            (TextId.CheckFormat, "{0}  შახი!", "{0}  Check!"),
            (TextId.AiThinking, "AI ფიქრობს...", "AI thinking..."),
            (TextId.CheckmateWhiteWins, "მატი - თეთრმა მოიგო", "Checkmate - White wins"),
            (TextId.CheckmateBlackWins, "მატი - შავმა მოიგო", "Checkmate - Black wins"),
            (TextId.Stalemate, "პატი - ფრე", "Stalemate - draw"),
            (TextId.DrawFiftyMove, "ფრე - 50 სვლის წესი", "Draw - 50-move rule"),
            (TextId.DrawThreefold, "ფრე - სამჯერადი გამეორება", "Draw - threefold repetition"),
            (TextId.DrawInsufficient, "ფრე - მასალის ნაკლებობა", "Draw - insufficient material"),
            (TextId.StockfishNotFoundFormat, "Stockfish ვერ მოიძებნა: {0}. ორი მოთამაშის რეჟიმი.", "Stockfish not found: {0}. Human vs human mode."),
            (TextId.AiErrorFormat, "AI-ის შეცდომა: {0}. ორი მოთამაშის რეჟიმი.", "AI error: {0}. Human vs human mode."),
            (TextId.HelpLine, "მაუსი: არჩევა/სვლა  TAB: ხედი  R: თავიდან", "Click: select/move  TAB: view  R: restart"),
            (TextId.DebugFenFormat, "{0}  [DEBUG FEN]", "{0}  [DEBUG FEN]"),
            (TextId.PromotionTitleFormat, "პაიკის გარდაქმნა ({0})", "Promote pawn ({0})"),
            (TextId.PieceQueen, "ლაზიერი", "Queen"),
            (TextId.PieceRook, "ეტლი", "Rook"),
            (TextId.PieceBishop, "კუ", "Bishop"),
            (TextId.PieceKnight, "მხედარი", "Knight"),
        };

        public static IEnumerable<object[]> GeorgianRows()
        {
            foreach (var r in Rows) yield return new object[] { r.Id, r.Ka };
        }

        public static IEnumerable<object[]> EnglishRows()
        {
            foreach (var r in Rows) yield return new object[] { r.Id, r.En };
        }

        [Fact]
        public void Georgian_IsActive()
        {
            Assert.Same(TextTable.Georgian, TextTable.Active);
            Assert.Equal("ka", TextTable.Georgian.LanguageCode);
            Assert.Equal("en", TextTable.English.LanguageCode);
        }

        [Fact]
        public void ContractTable_Has73Rows_InEnumOrder()
        {
            Assert.Equal(73, Rows.Length);
            Assert.Equal(Rows.Select(r => r.Id), Enum.GetValues(typeof(TextId)).Cast<TextId>());
        }

        [Fact]
        public void EveryId_HasText_InBothTables()
        {
            var ids = Enum.GetValues(typeof(TextId)).Cast<TextId>().ToArray();
            Assert.Equal(73, ids.Length);
            foreach (TextId id in ids)
            {
                Assert.False(string.IsNullOrEmpty(TextTable.Georgian.Get(id)), "ka " + id);
                Assert.False(string.IsNullOrEmpty(TextTable.English.Get(id)), "en " + id);
            }
        }

        [Theory]
        [MemberData(nameof(GeorgianRows))]
        public void Georgian_ExactTexts(TextId id, string expected)
        {
            Assert.Equal(expected, TextTable.Georgian.Get(id));
        }

        [Theory]
        [MemberData(nameof(EnglishRows))]
        public void English_ExactTexts(TextId id, string expected)
        {
            Assert.Equal(expected, TextTable.English.Get(id));
        }

        private static int[] PlaceholderIndexes(string text)
        {
            return Regex.Matches(text, @"\{(\d+)\}").Cast<Match>()
                .Select(m => int.Parse(m.Groups[1].Value)).Distinct().OrderBy(i => i).ToArray();
        }

        [Fact]
        public void Placeholders_MatchBetweenTables()
        {
            foreach (TextId id in Enum.GetValues(typeof(TextId)))
            {
                int[] ka = PlaceholderIndexes(TextTable.Georgian.Get(id));
                int[] en = PlaceholderIndexes(TextTable.English.Get(id));
                Assert.Equal(en, ka);

                object[] args = ka.Select(i => (object)("a" + i)).ToArray();
                // Placeholders are contiguous from 0, so the argument count equals the placeholder count.
                Assert.Equal(Enumerable.Range(0, ka.Length), ka);
                Assert.NotNull(TextTable.Georgian.Format(id, args));
                Assert.NotNull(TextTable.English.Format(id, args));
            }
        }

        [Fact]
        public void AllTexts_UseSupportedCharsOnly()
        {
            foreach (TextTable table in new[] { TextTable.Georgian, TextTable.English })
                foreach (TextId id in Enum.GetValues(typeof(TextId)))
                    foreach (char c in table.Get(id))
                        Assert.True(TextTable.IsSupportedChar(c), table.LanguageCode + " " + id + " U+" + ((int)c).ToString("X4"));
        }

        [Fact]
        public void IsSupportedChar_Rules()
        {
            foreach (char c in new[] { ' ', 'a', '~', '\n', 'ა', 'ჰ', 'ჿ' })
                Assert.True(TextTable.IsSupportedChar(c), "U+" + ((int)c).ToString("X4"));
            foreach (char c in new[] { '\t', '\r', ' ', '–', '—', '’', '«', 'Ⴀ', '჏', 'Ა', 'ⴀ' })
                Assert.False(TextTable.IsSupportedChar(c), "U+" + ((int)c).ToString("X4"));
        }

        [Fact]
        public void AtlasCharacters_AsciiThenMkhedruli()
        {
            string a = TextTable.AtlasCharacters;
            Assert.Equal(143, a.Length);
            Assert.Equal(143, a.Distinct().Count());
            Assert.Equal(' ', a[0]);
            Assert.Equal('~', a[94]);
            Assert.Equal('ა', a[95]);
            Assert.Equal('ჿ', a[142]);
            Assert.DoesNotContain('\n', a);
        }

        [Fact]
        public void Get_UndefinedId_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TextTable.Georgian.Get((TextId)9999));
            Assert.Throws<ArgumentOutOfRangeException>(() => TextTable.English.Get((TextId)(-1)));
        }

        [Fact]
        public void Format_NullArgs_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => TextTable.Georgian.Format(TextId.QuickColorFormat, null));
        }
    }
}
