using System;
using System.Collections.Generic;
using Chess.Campaign;
using Chess.Core;
using Xunit;

namespace Chess.Tests.Campaign
{
    public class GameTextTests
    {
        [Theory]
        [InlineData(GameStatus.InProgress, Side.White, false, false, "თეთრის სვლაა")]
        [InlineData(GameStatus.InProgress, Side.Black, true, false, "შავის სვლაა  შახი!")]
        [InlineData(GameStatus.InProgress, Side.White, false, true, "AI ფიქრობს...")]
        [InlineData(GameStatus.InProgress, Side.White, true, true, "AI ფიქრობს...")]
        [InlineData(GameStatus.Checkmate, Side.Black, true, false, "მატი - თეთრმა მოიგო")]
        [InlineData(GameStatus.Checkmate, Side.White, true, true, "მატი - შავმა მოიგო")]
        [InlineData(GameStatus.Stalemate, Side.White, false, false, "პატი - ფრე")]
        [InlineData(GameStatus.FiftyMoveRule, Side.White, false, false, "ფრე - 50 სვლის წესი")]
        [InlineData(GameStatus.ThreefoldRepetition, Side.Black, false, false, "ფრე - სამჯერადი გამეორება")]
        [InlineData(GameStatus.InsufficientMaterial, Side.White, false, false, "ფრე - მასალის ნაკლებობა")]
        [InlineData(GameStatus.InsufficientMaterial, Side.Black, false, true, "ფრე - მასალის ნაკლებობა")]
        public void Status_Exact(GameStatus status, Side side, bool check, bool ai, string expected)
        {
            Assert.Equal(expected, GameText.Status(status, side, check, ai));
        }

        [Fact]
        public void Status_Undefined_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GameText.Status((GameStatus)99, Side.White, false, false));
        }

        [Fact]
        public void SideName_Exact()
        {
            Assert.Equal("თეთრი", GameText.SideName(Side.White));
            Assert.Equal("შავი", GameText.SideName(Side.Black));
            Assert.Throws<ArgumentOutOfRangeException>(() => GameText.SideName((Side)5));
        }

        [Fact]
        public void PieceName_Exact()
        {
            Assert.Equal("ლაზიერი", GameText.PieceName(PieceType.Queen));
            Assert.Equal("ეტლი", GameText.PieceName(PieceType.Rook));
            Assert.Equal("კუ", GameText.PieceName(PieceType.Bishop));
            Assert.Equal("მხედარი", GameText.PieceName(PieceType.Knight));
            Assert.Throws<ArgumentOutOfRangeException>(() => GameText.PieceName(PieceType.King));
            Assert.Throws<ArgumentOutOfRangeException>(() => GameText.PieceName(PieceType.Pawn));
            Assert.Throws<ArgumentOutOfRangeException>(() => GameText.PieceName(PieceType.None));
        }

        [Fact]
        public void Labels_Exact()
        {
            Assert.Equal("პაიკის გარდაქმნა (თეთრი)", GameText.PromotionTitle(Side.White));
            Assert.Equal("ფერი: თეთრი", GameText.QuickColorLabel(Side.White));
            Assert.Equal("სირთულე: საშუალო", GameText.QuickDifficultyLabel("საშუალო"));
            Assert.Equal("სწრაფი თამაში - მარტივი - შენი ფერი: შავი", GameText.QuickGameInfo("მარტივი", Side.Black));
            Assert.Equal("მაუსი: არჩევა/სვლა  TAB: ხედი  R: თავიდან", GameText.HelpLine);
        }

        [Fact]
        public void ErrorTexts_Exact()
        {
            Assert.Equal(@"Stockfish ვერ მოიძებნა: C:\nope\stockfish.exe. ორი მოთამაშის რეჟიმი.",
                GameText.StockfishNotFound(@"C:\nope\stockfish.exe"));
            Assert.Equal("AI-ის შეცდომა: Engine crashed. ორი მოთამაშის რეჟიმი.", GameText.AiError("Engine crashed."));
            Assert.Equal("AI-ის შეცდომა: Boom. ორი მოთამაშის რეჟიმი.", GameText.AiError("Boom"));
            Assert.Equal("თეთრის სვლაა  [DEBUG FEN]", GameText.WithDebugFen("თეთრის სვლაა"));
        }

        [Fact]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => GameText.QuickDifficultyLabel(null));
            Assert.Throws<ArgumentNullException>(() => GameText.QuickGameInfo(null, Side.White));
            Assert.Throws<ArgumentNullException>(() => GameText.StockfishNotFound(null));
            Assert.Throws<ArgumentNullException>(() => GameText.AiError(null));
            Assert.Throws<ArgumentNullException>(() => GameText.WithDebugFen(null));
        }

        [Fact]
        public void AllOutputs_UseSupportedCharsOnly()
        {
            var outputs = new List<string>
            {
                GameText.HelpLine,
                GameText.SideName(Side.White), GameText.SideName(Side.Black),
                GameText.PromotionTitle(Side.White), GameText.PromotionTitle(Side.Black),
                GameText.QuickColorLabel(Side.White), GameText.QuickColorLabel(Side.Black),
                GameText.QuickDifficultyLabel("საშუალო"),
                GameText.QuickGameInfo("მარტივი", Side.Black),
                GameText.StockfishNotFound(@"C:\nope\stockfish.exe"),
                GameText.AiError("Engine crashed."),
                GameText.WithDebugFen("თეთრის სვლაა"),
            };
            foreach (PieceType t in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                outputs.Add(GameText.PieceName(t));
            foreach (GameStatus s in Enum.GetValues(typeof(GameStatus)))
                foreach (Side side in new[] { Side.White, Side.Black })
                    foreach (bool check in new[] { false, true })
                        foreach (bool ai in new[] { false, true })
                            outputs.Add(GameText.Status(s, side, check, ai));

            foreach (string text in outputs)
                foreach (char c in text)
                    Assert.True(TextTable.IsSupportedChar(c), text + " U+" + ((int)c).ToString("X4"));
        }
    }
}
