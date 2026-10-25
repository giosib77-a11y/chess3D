# Georgian Battle Chess (chess3D)

## Overview
3D ჭადრაკი Unity 6-ზე, კონცეფციით „ქართული საბრძოლო ჭადრაკი“ (`docs/CONCEPT.md`): ჩვეულებრივი დაფის ნაცვლად რეალური საბრძოლო ველია. ველზე ჩანს ოდნავ შესამჩნევი 8x8 ბადე, რომელიც ფიგურის არჩევისას ანათებს დასაშვებ უჯრებს. მოთამაშეს აქვს „სარდლის ხედი“ (პირველი პირი, მეფის პოზიციიდან) და ტაქტიკური ხედი (TAB). მოწინააღმდეგეა Stockfish, სირთულით დაახლოებით 800-დან 2200+ Elo-მდე.

**Stage 1 (MVP)** ამოწმებს მთავარ ტექნიკურ საფუძველს: სწორი წესები, 3D ველი და ბადე, სვლა მაუსით, ორი კამერა და Stockfish-ის მიერთება. გრაფიკა დროებითია (Unity primitive-ები).

> დამწყებისთვის: პროექტი სამ ნაწილად იყოფა. **Core** არის „ჭადრაკის ტვინი“ (წესები, სვლები, FEN) Unity-ის გარეშე, ამიტომ მის შემოწმებას ტესტებით Unity-ის გახსნა არ სჭირდება. **AI** ელაპარაკება Stockfish პროგრამას. **Game** არის Unity-ის ნაწილი: ხატავს ველს და ფიგურებს და კითხულობს მაუსს და კლავიატურას. Game თავად არასდროს წყვეტს, სვლა დასაშვებია თუ არა; ამას ყოველთვის Core-ს ეკითხება.

## Users
- ერთი ადამიანი, თავის Windows PC-ზე (ლოკალურად, ინტერნეტის გარეშე). როლები, ანგარიშები ან ავტორიზაცია არ არსებობს.

## Current Stage
Stage 1 — MVP

## Scope
### In scope (Stage 1)
- `Chess.Core`: ყველა წესი. როქი (უფლების დაკარგვა, მათ შორის როცა ეტლს მოჭრიან), en passant (მათ შორის შეკრული პაიკის შემთხვევა), გარდაქმნა Q/R/B/N-ად, შახი, მატი, პატი, 50 სვლის წესი, სამჯერადი გამეორება, მასალის ნაკლებობა, FEN-ის წაკითხვა და ჩაწერა, UCI სვლის ფორმატი, perft.
- `Chess.AI`: `IChessAI` ინტერფეისი და `StockfishAI` (UCI, ცალკე პროცესად). 4 სირთულის preset.
- `Chess.Game` (Unity):
  - ველი და 8x8 ბადე;
  - ფიგურები primitive-ებით;
  - მონიშვნა მაუსით, დასაშვები უჯრების განათება, სვლა და გარდაქმნის არჩევა;
  - ტაქტიკური და სარდლის კამერა (TAB);
  - ადამიანი Stockfish-ის წინააღმდეგ; თუ Stockfish არ არის, ორი ადამიანი ერთ კომპიუტერზე (hot-seat);
  - სტატუსის ტექსტი და restart (R).
- სატესტო ინფრასტრუქტურა: `dotnet build` / `dotnet test` Unity-ის გარეშე.

### Out of scope (Stage 1)
- ფაზა 2 და 3 (3D მოდელები, რუკა/კამპანია, ხმები, დიალექტები, cutscene-ები, განათება, ბალანსი) — `PROGRESS.md` Backlog.
- Mac/Linux, mobile, build-ის/installer-ის მომზადება, Stockfish-ის build-ში გავრცელება.
- Undo, PGN, საათები, შენახვა/ჩატვირთვა, მთავარი მენიუ, სვლის ანიმაცია, ქართული UI ტექსტი.

## Core Flows
1. **თამაშის დაწყება:** Play → ველი, ბადე და 32 ფიგურა → კამერა სარდლის ხედშია (ადამიანის მეფის უკან) → Stockfish-ის ინიციალიზაცია ფონურად.
2. **ადამიანის სვლა:** დააჭერ საკუთარ ფიგურას → ფიგურის უჯრა მოინიშნება და დასაშვები უჯრები ანათებს → დააჭერ დასაშვებ უჯრას → სვლა სრულდება → (თუ ვარიანტი ჩართულია) კამერა სარდლის ხედს უბრუნდება.
   - თუ სვლა გარდაქმნაა, ჯერ ჩნდება არჩევანი Queen/Rook/Bishop/Knight.
3. **AI-ის სვლა:** input იბლოკება → სტატუსი „AI thinking...“ → Stockfish აბრუნებს სვლას → Core ამოწმებს და ასრულებს → input იხსნება.
4. **თამაშის დასრულება:** მატი, პატი, 50 სვლა, სამჯერადი გამეორება ან მასალის ნაკლებობა → სტატუსი აჩვენებს შედეგს → სვლები აღარ მიიღება → R იწყებს ახალ თამაშს.
5. **Stockfish არ არის / გაფუჭდა:** სტატუსი აჩვენებს მკაფიო შეცდომას → თამაში გრძელდება hot-seat რეჟიმში (ორივე მხარეს მაუსით ათამაშებ).

## Business Rules
- წესები FIDE-ს მიხედვით, ორი გამარტივებით (Decision Log):
  - 50 სვლის წესი: ფრე ავტომატურად, როცა `HalfmoveClock >= 100`.
  - სამჯერადი გამეორება: ფრე ავტომატურად მესამე გამეორებაზე. მოთხოვნა (claim) არ სჭირდება.
- სტატუსის რიგი ყოველი სვლის შემდეგ:
  1. დასაშვები სვლა არ არის → შახის დროს `Checkmate`, სხვა შემთხვევაში `Stalemate`;
  2. `InsufficientMaterial`;
  3. `FiftyMoveRule`;
  4. `ThreefoldRepetition`.

  მატს უპირატესობა აქვს 50 სვლის წესზე.
- მასალის ნაკლებობა (ავტომატური ფრე) მხოლოდ ამ შემთხვევებშია: K v K, K+B v K, K+N v K, K+B(s) v K+B(s), როცა ყველა ოფიცერი ერთი ფერის უჯრებზეა. K+N+N v K **არ** არის ფრე.
- გამეორების გასაღები: ფიგურების განლაგება + სვლის რიგი + როქის უფლებები + en passant უჯრა. en passant უჯრა გასაღებში მხოლოდ მაშინ შედის, თუ en passant მოჭრა რეალურად დასაშვებია.
- Game ფენა სვლას ასრულებს მხოლოდ `GameState.TryMakeMove`-ით. Stockfish-ის სვლაც იმავე გზით მოწმდება.

## Data
მონაცემები მხოლოდ მეხსიერებაშია: ბაზა, ფაილები და შენახვა არ არის.
- `Position`: 64 უჯრა, სვლის რიგი, როქის უფლებები, en passant უჯრა, halfmove clock, fullmove number.
- `GameState`: მიმდინარე `Position`, სვლების ისტორია, გამეორების მთვლელი, სტატუსი და შედეგი.
- `AIDifficulty`: Skill Level, depth/movetime, Elo.

## Integrations
- **Stockfish** (ლოკალური პროგრამა, GPLv3), UCI პროტოკოლით stdin/stdout-ზე. binary git-ში არ ინახება. ნაგულისხმევი გზაა `Assets/StreamingAssets/Stockfish/stockfish.exe`; inspector-ში შეიძლება შეცვლა. MVP-ში მხოლოდ Windows x86-64.
- ყველა გამოძახებას აქვს timeout. Stockfish-ის შეცდომა თამაშს არ აჩერებს (hot-seat fallback).

## Stack & Architecture
- **Complexity tier:** Tier 1 — ერთი desktop აპლიკაცია; backend, ბაზა და ქსელი არ არის.
- **Engine:** Unity 6000.6.7f1, URP 17.6.0, Input System 1.20.1. მხოლოდ ახალი Input System (`activeInputHandler: 1`).
- **ენა:** C# 9. Core და AI იწერება netstandard2.1 API-ზე (Unity-ის Api Compatibility Level = .NET Standard 2.1).
- **ტესტები:** xUnit 2.x, net10.0 (ერთადერთი დაყენებული SDK: .NET 10.0.402).
- **AI:** Stockfish, UCI, `System.Diagnostics.Process`.
- **Frontend / Backend / Database / Storage / Auth / Hosting:** არ არის (ლოკალური თამაში).

### Assemblies და დამოკიდებულებები
```text
Chess.Core  (Assets/Scripts/Core, noEngineReferences)      ← არაფერზეა დამოკიდებული
   ↑
Chess.AI    (Assets/Scripts/AI,   noEngineReferences)      ← Chess.Core
   ↑
Chess.Game  (Assets/Scripts/Game, UnityEngine)             ← Chess.Core, Chess.AI, Unity.InputSystem
```
მიმართულება მხოლოდ ასეთია. Core-მა არ იცის AI-ის ან Unity-ის შესახებ, AI-მა არ იცის Unity-ის შესახებ.

### Folder tree
```text
Assets/
  Scenes/MainScene.unity              # თამაშის სცენა (USER აწყობს)
  Materials/                          # USER ქმნის Editor-ში (M_Ground, M_Tile, ...)
  StreamingAssets/Stockfish/
    README.md                         # სად დადო stockfish.exe
    stockfish.exe                     # USER დებს; git-ში არ ინახება
  Scripts/
    Core/  Chess.Core.asmdef + *.cs   # წესები (namespace Chess.Core)
    AI/    Chess.AI.asmdef   + *.cs   # IChessAI, UCI, StockfishAI (namespace Chess.AI)
    Game/  Chess.Game.asmdef + *.cs   # MonoBehaviour-ები (namespace Chess.Game)
Tests/
  ChessCore.Build/ChessCore.Build.csproj   # netstandard2.1, AssemblyName Chess.Core, აკომპილირებს Assets/Scripts/Core/**
  ChessAI.Build/ChessAI.Build.csproj       # netstandard2.1, AssemblyName Chess.AI,  აკომპილირებს Assets/Scripts/AI/**
  ChessCore.Tests/ChessCore.Tests.csproj   # net10.0 xUnit, ProjectReference ორივეზე
    BuildTargetTests.cs
    Core/*.cs                              # Core-ის ტესტები
    AI/*.cs                                # AI-ის ტესტები
```

## API Contract
ეს არის ერთადერთი ჭეშმარიტება Core, AI და Game ფენებს შორის. ხელმოწერის შეცვლა მხოლოდ ამ სექციის განახლებით (და Decision Log-ის ჩანაწერით) შეიძლება.

### Error model (ერთიანი წესი ყველა ფენისთვის)
| სიტუაცია | ქცევა |
|---|---|
| არასწორი FEN, არასწორი უჯრის სახელი (`Square.Parse`) | `FormatException`, შეტყობინების ფორმატი: `Invalid FEN: <reason>. FEN: '<fen>'` / `Invalid square: '<text>'` |
| არასწორი სვლა მომხმარებლისგან ან engine-ისგან | **არ** აგდებს exception-ს: `GameState.TryMakeMove` / `TryMakeUciMove` აბრუნებს `false`-ს და state არ იცვლება |
| პროგრამისტის შეცდომა (null არგუმენტი, დიაპაზონს გარეთ) | `ArgumentNullException` / `ArgumentOutOfRangeException` / `ArgumentException` |
| Stockfish-ის ფაილი არ არსებობს | `StockfishNotFoundException` (`ChessAIException`-ის ქვეკლასი), `Path` property-ით |
| Stockfish ვერ გაეშვა, გაითიშა, timeout, პროტოკოლის შეცდომა, არასწორი `bestmove` | `ChessAIException`. timeout-ის დროს `InnerException` არის `TimeoutException`. ამის შემდეგ instance „faulted“ ხდება და შემდეგი გამოძახებებიც `ChessAIException`-ს აგდებს |
| გაუქმება (`CancellationToken`) | `OperationCanceledException` |
| `GetBestMoveAsync` ერთდროულად ორჯერ / დამთავრებულ პოზიციაზე | `InvalidOperationException` |
| Dispose-ის შემდეგ გამოძახება | `ObjectDisposedException` |
| Game ფენა | AI-ის ყველა exception-ს იჭერს, აჩვენებს `StatusHud`-ში (მოკლე ტექსტი, stack trace-ის გარეშე), სრულ ტექსტს წერს `Debug.LogException`-ით და გადადის hot-seat-ზე |

### Chess.Core (namespace `Chess.Core`)
```csharp
public enum Side : byte { White = 0, Black = 1 }
public static class SideExtensions { public static Side Opposite(this Side side); }

public enum PieceType : byte { None = 0, Pawn = 1, Knight = 2, Bishop = 3, Rook = 4, Queen = 5, King = 6 }

public readonly struct Piece : IEquatable<Piece>
{
    public static readonly Piece None;                    // Type == PieceType.None
    public Piece(PieceType type, Side side);              // ArgumentException if type == None
    public PieceType Type { get; }
    public Side Side { get; }                             // meaningless when IsNone
    public bool IsNone { get; }
    public char ToFenChar();                              // 'P','N','B','R','Q','K' white; lowercase black; InvalidOperationException for None
    public static bool TryFromFenChar(char c, out Piece piece);
    // ==, !=, Equals, GetHashCode
}

public readonly struct Square : IEquatable<Square>
{
    public Square(int index);                             // 0..63, else ArgumentOutOfRangeException
    public static Square FromFileRank(int file, int rank);// file 0..7 (a..h), rank 0..7 (1..8)
    public int Index { get; }                             // a1=0, b1=1, ... h1=7, a2=8, ... h8=63
    public int File { get; }                              // Index % 8
    public int Rank { get; }                              // Index / 8
    public static Square Parse(string name);              // "e4"; FormatException otherwise (lowercase only)
    public static bool TryParse(string name, out Square square);
    public override string ToString();                    // "e4"
    // ==, !=, Equals, GetHashCode
}

[Flags] public enum CastlingRights : byte
{ None = 0, WhiteKingside = 1, WhiteQueenside = 2, BlackKingside = 4, BlackQueenside = 8, All = 15 }

[Flags] public enum MoveFlags : byte
{ None = 0, Capture = 1, DoublePawnPush = 2, EnPassant = 4, CastleKingside = 8, CastleQueenside = 16, Promotion = 32 }

public readonly struct Move : IEquatable<Move>
{
    public Move(Square from, Square to, PieceType promotion = PieceType.None, MoveFlags flags = MoveFlags.None);
                                                          // promotion ∈ {None, Knight, Bishop, Rook, Queen}, else ArgumentException
    public Square From { get; }
    public Square To { get; }
    public PieceType Promotion { get; }                   // None if not a promotion
    public MoveFlags Flags { get; }
    public bool IsPromotion { get; }
    public bool IsCapture { get; }                        // Capture or EnPassant
    public bool IsCastle { get; }
    public string ToUci();                                // "e2e4", "e7e8q"; castling = king move "e1g1"/"e1c1"
    public static bool TryParseUci(string uci, out Move move); // syntax only (4 or 5 chars, promo n/b/r/q); Flags = None
    // Equality and hash use From, To, Promotion ONLY (Flags ignored),
    // so a parsed UCI move equals the generated legal move.
}

public sealed partial class Position                       // public surface is read-only
{
    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static Position CreateStart();
    public static Position FromFen(string fen);           // FormatException (see FEN rules)
    public string ToFen();                                // all 6 fields; FromFen(f).ToFen() == f for normalized FENs
    public Piece GetPiece(Square square);
    public Side SideToMove { get; }
    public CastlingRights CastlingRights { get; }
    public Square? EnPassantSquare { get; }               // set after every double push (standard FEN), else null
    public int HalfmoveClock { get; }
    public int FullmoveNumber { get; }
    public Square FindKing(Side side);                    // exactly one king per side is guaranteed by FromFen
    public Position Clone();

    // Position.Moves.cs (task 4)
    public IReadOnlyList<Move> GetLegalMoves();           // fully legal, with Flags set; 4 entries per promotion
    public bool IsInCheck();                              // side to move
    public bool IsSquareAttacked(Square square, Side bySide);
    internal Position MakeMove(Move legalMove);           // no legality check; copy-make or make/unmake = implementer's choice
}

public static class Perft
{
    public static long Count(Position position, int depth);           // depth 0 → 1; negative → ArgumentOutOfRangeException
    public static IReadOnlyDictionary<string, long> Divide(Position position, int depth); // key = root move UCI
}

public enum GameStatus { InProgress, Checkmate, Stalemate, FiftyMoveRule, ThreefoldRepetition, InsufficientMaterial }
public enum GameResult { None, WhiteWins, BlackWins, Draw }

public sealed class GameState
{
    public GameState();                                   // standard start position
    public GameState(string fen);                         // FormatException incl. "side not to move is in check"
    public Position Position { get; }                     // current position (read-only API)
    public Side SideToMove { get; }
    public IReadOnlyList<Move> LegalMoves { get; }        // empty when IsGameOver
    public IReadOnlyList<Move> GetLegalMovesFrom(Square from); // empty if none / game over
    public bool IsCheck { get; }                          // side to move in check (true at checkmate too)
    public GameStatus Status { get; }
    public GameResult Result { get; }                     // None while InProgress
    public bool IsGameOver { get; }                       // Status != InProgress
    public IReadOnlyList<Move> MoveHistory { get; }       // moves with full Flags
    public Move? LastMove { get; }
    public bool TryMakeMove(Move move);                   // matched by From/To/Promotion against LegalMoves;
                                                          // false if illegal, game over, or promotion piece missing
    public bool TryResolveUciMove(string uci, out Move legalMove); // UCI → legal Move with Flags
    public bool TryMakeUciMove(string uci);
    public string ToFen();
}
```

**FEN rules (`Position.FromFen`):**
- ზუსტად 6 ველი უნდა იყოს, გამოყოფილი ერთი ან მეტი space-ით.
- ზუსტად 8 რიგი (rank) `/`-ით; თითო რიგი = 8 უჯრა; დასაშვები სიმბოლოები: `pnbrqkPNBRQK1-8`.
- თითო მხარეს ზუსტად ერთი მეფე; პაიკი 1-ელ ან მე-8 რიგზე არ დგას.
- სვლის რიგი: `w` ან `b`.
- როქი: `-` ან `KQkq`-ის ქვესიმრავლე, გამეორების გარეშე (რიგი ნებისმიერია). თითო უფლებას სჭირდება მეფე და ეტლი საწყის უჯრებზე, თორემ `FormatException`.
- en passant: `-`, ან მე-6 რიგის უჯრა (როცა თეთრის სვლაა) / მე-3 რიგის უჯრა (როცა შავის სვლაა).
- halfmove ≥ 0, fullmove ≥ 1, მთელი რიცხვები.
- `GameState(string)` დამატებით ამოწმებს, რომ იმ მხარის მეფე, რომლის სვლაც არ არის, შახში არ დგას.

### Chess.AI (namespace `Chess.AI`)
```csharp
public enum DifficultyPreset { Easy, Medium, Hard, Master }

public sealed class AIDifficulty
{
    public AIDifficulty(int skillLevel, int? depth, int moveTimeMs, int? elo);
    // skillLevel 0..20; depth null or 1..30; moveTimeMs 50..60000; elo null or 500..3500
    // → ArgumentOutOfRangeException otherwise
    public int SkillLevel { get; }
    public int? Depth { get; }               // if set: "go depth N", else "go movetime MoveTimeMs"
    public int MoveTimeMs { get; }
    public int? Elo { get; }                 // if set: UCI_LimitStrength true + UCI_Elo (clamped to engine's min/max)
    public bool LimitStrength { get; }       // Elo.HasValue
    public static AIDifficulty FromPreset(DifficultyPreset preset);
}
```
| Preset | SkillLevel | Depth | MoveTimeMs | Elo | სამიზნე (მიახლოებით) |
|---|---|---|---|---|---|
| Easy | 0 | 1 | 100 | null | ~800–1000 (UCI_Elo-ს მინიმუმი ~1320-ია, ამიტომ აქ Skill 0 + depth 1 გამოიყენება) |
| Medium | 20 | null | 300 | 1400 | ~1400 |
| Hard | 20 | null | 500 | 1800 | ~1800 |
| Master | 20 | null | 1000 | 2200 | ~2200 |

```csharp
public sealed class UciOption
{
    public UciOption(string name, string type, string defaultValue, int? min, int? max);
    public string Name { get; }  public string Type { get; }  public string DefaultValue { get; }
    public int? Min { get; }     public int? Max { get; }
}

public static class UciProtocol
{
    public const string Uci = "uci", UciOk = "uciok", IsReady = "isready", ReadyOk = "readyok",
                        UciNewGame = "ucinewgame", Stop = "stop", Quit = "quit";
    public static string BuildSetOption(string name, string value);    // "setoption name Skill Level value 5"
    public static IReadOnlyList<string> BuildDifficultyCommands(AIDifficulty difficulty,
        IReadOnlyDictionary<string, UciOption> engineOptions);
    // only options the engine advertised; values clamped to option Min/Max.
    // "Skill Level"; if LimitStrength: "UCI_LimitStrength" true + "UCI_Elo"; else "UCI_LimitStrength" false
    public static string BuildPosition(string fen);                    // "position fen <fen>"
    public static string BuildGo(AIDifficulty difficulty);             // "go depth N" | "go movetime N"
    public static bool TryParseOption(string line, out UciOption option); // "option name X type spin default D min A max B"
    public static bool TryParseBestMove(string line, out string uciMove);
    // "bestmove e2e4 ponder e7e5" → true, "e2e4"; "bestmove (none)" → true, null; other lines → false
}

public interface IUciTransport : IDisposable
{
    void Start();                                         // ChessAIException if the process cannot start
    void SendLine(string line);
    Task<string> ReadLineAsync(TimeSpan timeout, CancellationToken ct); // null at EOF; TimeoutException on timeout
    bool HasExited { get; }
}

public sealed class ProcessUciTransport : IUciTransport
{
    public ProcessUciTransport(string executablePath);
    // UseShellExecute=false, CreateNoWindow=true, redirect stdin/stdout, WorkingDirectory = exe folder,
    // no arguments, no shell. Stdout lines collected via OutputDataReceived (no System.Threading.Channels).
    // Dispose: Kill() if still running, then dispose the Process.
}

public class ChessAIException : Exception
{
    public ChessAIException(string message);
    public ChessAIException(string message, Exception inner);
}
public sealed class StockfishNotFoundException : ChessAIException
{
    public StockfishNotFoundException(string path);       // message: "Stockfish executable not found: <path>"
    public string Path { get; }
}

public interface IChessAI : IDisposable
{
    bool IsReady { get; }                                 // handshake completed
    Task InitializeAsync(CancellationToken ct);           // idempotent; concurrent callers share one init
    Task<Move> GetBestMoveAsync(string fen, CancellationToken ct); // initializes if needed; returns a LEGAL Move with Flags
}

public sealed class StockfishAI : IChessAI
{
    public StockfishAI(string executablePath, AIDifficulty difficulty); // StockfishNotFoundException if file missing
    public StockfishAI(IUciTransport transport, AIDifficulty difficulty); // for tests
    public AIDifficulty Difficulty { get; }
    public TimeSpan HandshakeTimeout { get; set; }        // default 10 s (uciok, readyok)
    public TimeSpan SearchTimeout { get; set; }           // default 30 s, waiting for bestmove
}
```
**StockfishAI protocol:**
1. `InitializeAsync`: `Start()` → `uci` → `option` ხაზების შეგროვება `uciok`-მდე → `BuildDifficultyCommands` → `isready` → `readyok`.
2. `GetBestMoveAsync(fen)`:
   - ჯერ `new GameState(fen)`: არასწორ FEN-ზე `FormatException`, დამთავრებულ თამაშზე `InvalidOperationException`;
   - შემდეგ `position fen ...` → `go ...` → ხაზების კითხვა `bestmove`-მდე;
   - `TryResolveUciMove`; არასწორი სვლის ან `(none)`-ის დროს `ChessAIException`.
3. გაუქმებისას: `stop`, `bestmove`-ის მოლოდინი ≤ 2 s, შემდეგ `OperationCanceledException`.
4. Dispose: `quit`, ≤ 1 s მოლოდინი, შემდეგ transport.Dispose (Kill). Dispose idempotent-ია.

### Chess.Game (namespace `Chess.Game`, Unity)
**Coordinate mapping (ერთი უჯრა = 1 Unity unit):**
- უჯრის (file f, rank r) ცენტრი = world `(f, 0, r)`. მაგალითად: a1 = (0,0,0), h1 = (7,0,0), a8 = (0,0,7), h8 = (7,0,7). დაფის ცენტრი = (3.5, 0, 3.5).
- x = file (a→h), z = rank (1→8). თეთრის მხარეა დაბალი z, თეთრის „წინ“ = +z, შავის „წინ“ = −z.
- ველი (ground) y = 0. ბადის უჯრები (tile) y = 0.01, ზომა 0.94 × 0.02 × 0.94 (ღრიჭოები ბადის ხაზებად ჩანს). ფიგურები დგას y = 0.02-ზე.

**Inspector-ის ველების სახელები სავალდებულოა**, რადგან USER-ის ინსტრუქციები მათ სახელით ასახელებს (Unity აჩვენებს `boardView`-ს როგორც „Board View“).

| Component (ფაილი) | სად დევს | პასუხისმგებლობა | Inspector fields / public API |
|---|---|---|---|
| `BoardCoordinates` (static) | — | Square ↔ world | `Vector3 SquareToWorld(Square sq, float height = 0f)`, `Vector3 BoardCenter`, `Vector3 ForwardFor(Side side)` |
| `SquareRef` | tile-ზე და ფიგურის root-ზე | ეუბნება raycast-ს, რომელ უჯრას ეკუთვნის ობიექტი | `public Square Square { get; set; }` |
| `BoardView` | `ChessGame` | `Awake`-ში ქმნის ველს (Plane) და 64 tile-ს (`Tile_e4`, BoxCollider + SquareRef); ანათებს უჯრებს | `groundMaterial`, `tileMaterial`, `legalTargetMaterial`, `selectedMaterial` (Material), `groundSize` (float, 30). API: `ShowSelection(Square selected, IEnumerable<Square> targets)`, `ClearHighlights()` |
| `PiecesView` | `ChessGame` | ფიგურები primitive-ებით; `Sync` შლის ძველს და ქმნის ახალს (ანიმაციის გარეშე) | `whiteMaterial`, `blackMaterial`. API: `Sync(Position position)` |
| `SquarePicker` | `ChessGame` | მაუსის მარცხენა click → raycast → უჯრა (`Mouse.current`) | `raycastCamera` (Camera), `maxDistance` (200). `event Action<Square?> Clicked` (null = ცარიელ ადგილას დააჭირა) |
| `PromotionPrompt` | `ChessGame` | OnGUI ფანჯარა: Queen / Rook / Bishop / Knight / Cancel; Esc = Cancel | `bool IsOpen`, `Open(Side side, Action<PieceType> onChosen, Action onCancelled)` |
| `SelectionController` | `ChessGame` | მონიშვნის ლოგიკა: საკუთარი ფიგურა → განათება; დასაშვები უჯრა → სვლა (გარდაქმნისას ჯერ PromotionPrompt); სხვა საკუთარი ფიგურა → ხელახალი მონიშვნა; სხვაგან → მონიშვნის მოხსნა | `boardView`, `picker` (SquarePicker), `promotionPrompt`. API: `Bind(GameState state)`, `bool Interactable { get; set; }`, `ClearSelection()`, `event Action<Move> MoveChosen` |
| `CameraRig` | `Main Camera` | ერთი კამერა, ორი გამოთვლილი პოზა, გლუვი lerp; TAB ცვლის ხედს | `transitionSpeed` (5), `tacticalHeight` (9), `tacticalDistance` (8), `commanderEyeHeight` (1.4), `commanderBackOffset` (0.6), `commanderPitch` (15), `returnToCommanderAfterMove` (bool, true), `startInCommanderView` (bool, true). API: `enum CameraView { Commander, Tactical }`, `CameraView CurrentView`, `SetCommander(Square kingSquare, Side side)`, `SetView(CameraView view)`, `Toggle()`, `NotifyHumanMoveMade()` |
| `StatusHud` | `ChessGame` | OnGUI ტექსტი ზედა-მარცხენა კუთხეში: სტატუსი, შეცდომა, დახმარების ხაზი | API: `Show(string status, string error)` |
| `GameController` | `ChessGame` | კომპოზიცია და თამაშის ნაკადი: GameState, სვლები, AI-ის რიგი, input-ის დაბლოკვა, R = restart, შეცდომა → hot-seat, `OnDestroy`-ში AI-ის Dispose | `boardView`, `piecesView`, `selection`, `cameraRig`, `hud`, `humanSide` (Side, White), `playAgainstAI` (bool, true), `difficulty` (DifficultyPreset, Medium), `stockfishRelativePath` (string, `Stockfish/stockfish.exe`, StreamingAssets-ის მიმართ), `stockfishPathOverride` (string, ცარიელი = გამოიყენება relative), `startFen` (string, ცარიელი = საწყისი პოზიცია) |

**ფიგურების ფორმები (დროებითი):**
- pawn: დაბალი cylinder (~0.5 სიმაღლე);
- rook: cube (~0.6);
- knight: წაგრძელებული cube (~0.7, z-ზე გრძელი; შავისთვის 180°-ით შემობრუნებული);
- bishop: capsule (~0.8);
- queen: cylinder + sphere (~0.95);
- king: cube + პატარა cube „ჯვრად“ (~1.05).

collider-ები ბავშვ primitive-ებზე რჩება; root-ზე დევს `SquareRef`, ხოლო raycast იყენებს `GetComponentInParent<SquareRef>()`-ს.

**Status ტექსტები (MVP, ინგლისურად):**
- `White to move` / `Black to move`, დამატებით `Check!`;
- `AI thinking...`;
- `Checkmate - White wins`, `Stalemate - draw`, `Draw - 50-move rule`, `Draw - threefold repetition`, `Draw - insufficient material`;
- შეცდომის ხაზი, მაგ. `Stockfish not found: <path>. Human vs human mode.`;
- დახმარების ხაზი: `Click: select/move  TAB: view  R: restart`.

**GameController-ის ნაკადი:**
- `Start`: `GameState` (`startFen` ან საწყისი პოზიცია) → `piecesView.Sync` → `selection.Bind` → `cameraRig.SetCommander(king of humanSide)` → თუ `playAgainstAI`, იქმნება `StockfishAI` და ფონურად ეშვება `InitializeAsync`.
- ადამიანის `MoveChosen` → `TryMakeMove` → Sync + Bind + კამერა + HUD → `cameraRig.NotifyHumanMoveMade()` → თუ AI-ის რიგია, `RunAiTurnAsync`.
- `RunAiTurnAsync`:
  - `selection.Interactable = false`;
  - `await ai.GetBestMoveAsync(state.ToFen(), cts.Token)`;
  - თუ ამასობაში თაობის (generation) მთვლელი შეიცვალა (restart), შედეგი უგულებელყოფა;
  - სხვა შემთხვევაში `TryMakeMove`; შეცდომისას hot-seat.
- R (Keyboard.current.rKey), როცა PromotionPrompt დახურულია: cts.Cancel → ახალი GameState → generation++ → Sync. თუ AI ამ დროს **არ** ფიქრობდა, AI instance-ი იმავე რჩება; თუ ფიქრობდა — ძველი instance-ი იშლება ფონურად (`Task.Run(Dispose)`) და იქმნება ახალი `StockfishAI` (იხ. Decision Log „Restart AI-ის ფიქრისას“).
- hot-seat რეჟიმში მოთამაშე ორივე მხარეს აკონტროლებს; კამერა `humanSide`-ის მეფესთან რჩება.

## Testing Strategy
- **Core (კრიტიკული ლოგიკა):** xUnit unit-ტესტები და perft (`Tests/ChessCore.Tests/Core`).
  - startpos d1–4: 20, 400, 8902, 197281;
  - Kiwipete `r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1` d1–3: 48, 2039, 97862;
  - pos3 `8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1` d1–4: 14, 191, 2812, 43238;
  - pos4 `r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1` d1–3: 6, 264, 9467;
  - pos5 `rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8` d1–3: 44, 1486, 62379;
  - startpos d5 = 4865609 მხოლოდ მაშინ, თუ < 5 s.
- **AI:** UCI parsing და ბრძანებების აწყობა სუფთა unit-ტესტებით. `StockfishAI`-ის ნაკადი მოწმდება `FakeUciTransport`-ით, binary-ს გარეშე.
- **Integration:** რეალური Stockfish-ით ტესტი ეშვება მხოლოდ მაშინ, როცა გარემოს ცვლადი `STOCKFISH_PATH` დაყენებულია. თუ არ არის, ტესტი ადრე ბრუნდება და `ITestOutputHelper`-ში წერს `SKIPPED: STOCKFISH_PATH not set`. ასეთი შედეგი report-ში აუცილებლად იწერება როგორც „შეუმოწმებელი“.
- **netstandard2.1 / C# 9 დაცვა:** `dotnet build Tests/ChessCore.Tests` აკომპილირებს Core-ს და AI-ს netstandard2.1-ზე LangVersion 9.0-ით და ImplicitUsings-ის გარეშე, როგორც Unity. `BuildTargetTests` ამოწმებს TargetFramework-ს.
- **Game (Unity):** dotnet-ით ვერ კომპილირდება. ყოველი Game task მოწმდება USER task-ით: Console-ში 0 შეცდომა + კონკრეტული Play-mode შემოწმებები. Unity Test Framework და Playwright არ გამოიყენება.

## Realistic Scale
ერთი მოთამაშე, ერთი პარტია ერთდროულად, ერთი Stockfish პროცესი. წარმადობის მოთხოვნა: perft ტესტები ჯამში დაახლოებით 30 წამამდე, AI-ის სვლა 1–2 წამამდე.

## Security & Privacy
- პერსონალური მონაცემები, ქსელი და ავტორიზაცია არ არის.
- გარე პროცესის გაშვება: მხოლოდ `ProcessUciTransport`, `UseShellExecute = false`, არგუმენტების და shell-ის გარეშე. გზა მოდის inspector-იდან ან StreamingAssets-იდან და არა მომხმარებლის runtime input-იდან. Play-ის დასრულებისას პროცესი აუცილებლად კვდება (`OnDestroy` → Dispose → Kill), რომ ობოლი `stockfish.exe` არ დარჩეს.
- Stockfish GPLv3-ია. binary git-ში არ ინახება. build-ის გავრცელებამდე საჭიროა ლიცენზიის გადაწყვეტილება (Backlog).

## Constraints
- მომხმარებელი Unity-ში დამწყებია, ამიტომ Editor-ის ყველა ნაბიჯი USER task-ია, ქართული ინსტრუქციით.
- აგენტები არ ეხებიან `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `ProjectSettings/`, `.claude/`-ს. `.meta` ფაილებს მხოლოდ Unity ქმნის.
- დაყენებულია მხოლოდ .NET SDK 10 (.NET 8 runtime არ არის).
- NuGet პაკეტები მხოლოდ სატესტო პროექტშია. Core/AI იყენებს მხოლოდ netstandard2.1 BCL-ს (Unity NuGet-ს არ კითხულობს).
- პლატფორმა: Windows. კონცეფციის ვადა: ფაზა 1 ≈ 1 თვე.

## Decision Log
### 2026-10-25 — Tier 1, ერთი Unity აპლიკაცია
- Decision: Tier 1. ერთი desktop თამაში backend-ის, ბაზისა და ქსელის გარეშე.
- Reason: ერთი ლოკალური მოთამაშე, მონაცემები მხოლოდ მეხსიერებაში.
- Alternatives considered: ონლაინ თამაში ან სერვერზე Stockfish.
- Why rejected: მოთხოვნებში არ არის. ცვლილება: ახალი ეტაპის მოთხოვნით.

### 2026-10-25 — Unity 6 და არა UE5
- Decision: Unity 6000.6.7f1 + URP. კონცეფცია ორივეს ასახელებს.
- Reason: პროექტი უკვე Unity-ზეა შექმნილი. C# იძლევა სუფთა, Unity-ის გარეშე დატესტვად Core-ს.
- Alternatives considered: UE5.
- Why rejected: თავიდან დაწყება იქნებოდა და C++/Blueprints ართულებს ტესტირებას.

### 2026-10-25 — საკუთარი წესების ძრავა (Chess.Core) და არა Stockfish-ით ვალიდაცია
- Decision: წესები და სვლების გენერაცია Chess.Core-შია. Stockfish მხოლოდ მოწინააღმდეგეა.
- Reason:
  - UCI-ს დასაშვები სვლების სიის მარტივი API არ აქვს;
  - თამაში Stockfish-ის გარეშეც უნდა მუშაობდეს (hot-seat);
  - Core დატესტვადია perft-ით.
- Alternatives considered: `go perft 1` Stockfish-ში, ან მზა C# ბიბლიოთეკა.
- Why rejected: ძლიერი დამოკიდებულება გარე პროცესზე; NuGet Unity-ში დამატებით სირთულეს ქმნის.

### 2026-10-25 — დაფის წარმოდგენა: 64-უჯრიანი mailbox
- Decision: `Piece[64]` (a1=0 … h8=63) და ფსევდო-ლეგალური გენერაცია + ფილტრი „მეფე შახში ხომ არ რჩება“. copy-make თუ make/unmake, იმპლემენტატორი წყვეტს (`MakeMove` internal-ია).
- Reason: უმარტივესი სწორი ვარიანტია; perft d4-ისთვის საკმარისად სწრაფია.
- Alternatives considered: bitboards, 0x88.
- Why rejected: bitboards ზედმეტად რთულია MVP-სთვის, 0x88-ს აქ რეალური სარგებელი არ აქვს. ცვლილება: მხოლოდ გაზომილი წარმადობის პრობლემისას.

### 2026-10-25 — netstandard2.1-ის დაცვა: ორი build პროექტი + net10 xUnit
- Decision:
  - `Tests/ChessCore.Build` (AssemblyName `Chess.Core`) და `Tests/ChessAI.Build` (AssemblyName `Chess.AI`, ProjectReference Core-ზე);
  - ორივე: netstandard2.1, LangVersion 9.0, Nullable disable, ImplicitUsings disable, EnableDefaultCompileItems false, `<Compile Include="../../Assets/Scripts/<Core|AI>/**/*.cs" />` (ასლის გარეშე);
  - `Tests/ChessCore.Tests` (net10.0, xUnit 2.x) ორივეს ProjectReference-ით იყენებს.
- Reason:
  - Core-ის პირდაპირ net10 ტესტ-პროექტში კომპილაცია ვერ დაიჭერდა API-ებს, რომლებიც Unity-ს არ აქვს;
  - ორი ცალკე პროექტი იმეორებს asmdef-ის საზღვრებს: AI→Core `internal` წვდომა ან Core→AI მითითება აქაც ჩავარდება, ისევე როგორც Unity-ში;
  - ImplicitUsings disable ნიშნავს, რომ გამოტოვებული `using` აქაც ჩავარდება, როგორც Unity-ში.
- Alternatives considered: ერთი build პროექტი Core+AI-ისთვის; NUnit; Unity Test Framework.
- Why rejected: ერთი პროექტი შლის assembly-ის საზღვარს. NUnit-ს უპირატესობა არ აქვს. UTF Editor-ის გაშვებას მოითხოვს და აგენტები მას ვერ უშვებენ.

### 2026-10-25 — C# 9 შეზღუდვები
- Decision: Core/AI-ში `record` და `init` არ გამოიყენება; გამოიყენება `readonly struct` ან კლასი get-only property-ებით.
- Reason: Unity-ს არ აქვს `IsExternalInit`. netstandard2.1 + LangVersion 9 build იგივე შეცდომას (CS0518) იძლევა.
- Alternatives / Why rejected: საკუთარი `IsExternalInit` polyfill — ზედმეტი ხრიკია, სარგებელი მცირე.

### 2026-10-25 — AI: ცალკე assembly, IChessAI, UCI ცალკე პროცესად
- Decision:
  - `Chess.AI` asmdef (noEngineReferences, → Chess.Core);
  - `StockfishAI` იყენებს `IUciTransport`-ს: რეალური = `ProcessUciTransport`, ტესტებში = fake;
  - parsing `UciProtocol` static კლასშია.
- Reason: პროტოკოლი და ნაკადი დატესტვადია binary-ს გარეშე; Unity-ს ეს კოდი არ სჭირდება.
- Alternatives considered: Stockfish native plugin-ად; WebAssembly.
- Why rejected: უფრო რთულია და GPL-ის მხრივ უფრო მგრძნობიარე. ცვლილება: ახალი `IChessAI` იმპლემენტაცია.

### 2026-10-25 — სირთულის preset-ები
- Decision: Easy = Skill 0 + depth 1; Medium/Hard/Master = UCI_LimitStrength + UCI_Elo 1400/1800/2200, movetime 300/500/1000 ms. Elo იჭრება engine-ის მიერ გამოცხადებულ min/max-ზე, რომელიც `option` ხაზებიდან იკითხება.
- Reason: UCI_Elo-ს მინიმუმი ახალ Stockfish-ში ~1320-ია, ამიტომ ~800 მხოლოდ Skill Level-ით და მცირე სიღრმით მიიღწევა. Elo მიახლოებითია.
- How to change: `AIDifficulty.FromPreset` ან `new AIDifficulty(...)`. ფაზა 3-ში რეგიონის მიხედვით დინამიური სირთულე.

### 2026-10-25 — Stockfish binary git-ში არ ინახება; მხოლოდ Windows
- Decision:
  - USER დებს `Assets/StreamingAssets/Stockfish/stockfish.exe`-ს; `.gitignore` შლის `*.exe`-ს და `*.exe.meta`-ს ამ საქაღალდეში;
  - გზის შეცვლა შეიძლება `stockfishPathOverride`-ით;
  - binary-ს თუ არ არის, მკაფიო შეცდომა და hot-seat.
- Reason: ზომა (~100 MB) და GPLv3.
- How to change: Mac/Linux binary-ები და build-ში ჩართვა — Backlog.

### 2026-10-25 — ფრეს წესები: ავტომატური; მასალის ნაკლებობა ჩართულია
- Decision: 50 სვლა (`HalfmoveClock >= 100`) და სამჯერადი გამეორება ავტომატურ ფრეს იძლევა. Insufficient material ჩართულია (K v K, KB v K, KN v K, ერთფეროვანი ოფიცრები).
- Reason: claim UI-ის გარეშე ეს უმარტივესია. მასალის ნაკლებობა იაფია და FIDE-ს „მკვდარ პოზიციას“ ფარავს ძირითად შემთხვევებში.
- How to change: claim-ის რეჟიმისთვის `GameStatus`-ს დაემატება `CanClaimDraw`.

### 2026-10-25 — შეცდომების მოდელი
- Decision: მომხმარებლის ან engine-ის არასწორი სვლა აბრუნებს `false`-ს (`TryMakeMove`). არასწორი FEN აგდებს `FormatException`-ს. AI-ის შეცდომა = `ChessAIException` (+ `StockfishNotFoundException`).
- Reason: არასწორი click თამაშში ჩვეულებრივი მოვლენაა და exception-ს არ იმსახურებს. FEN პროგრამისტის ან კონფიგურაციის შეცდომაა.

### 2026-10-25 — Game ფენა: თხელი სკრიპტები, ერთი GameObject, IMGUI
- Decision:
  - ყველა თამაშის კომპონენტი `ChessGame` GameObject-ზეა, `CameraRig` — `Main Camera`-ზე;
  - ველი და 64 tile იქმნება კოდით (`BoardView.Awake`); ფიგურები `PiecesView.Sync`-ით სრულად იქმნება თავიდან ყოველი სვლის შემდეგ, ანიმაციის გარეშე;
  - სტატუსი და გარდაქმნის არჩევა OnGUI-ით (IMGUI);
  - input მხოლოდ `Keyboard.current` / `Mouse.current`-ით, InputActions asset-ის გარეშე.
- Reason:
  - დამწყებისთვის ყველაზე ნაკლები Editor-ის ნაბიჯი;
  - uGUI-ს Canvas + EventSystem + InputSystemUIInputModule სჭირდება;
  - 32 primitive-ის თავიდან შექმნა იაფია.
- How to change: ფაზა 2-ში uGUI/TextMeshPro და ანიმაციები.

### 2026-10-25 — HUD-ის ტექსტი MVP-ში ინგლისურად
- Decision: სტატუსის სტრიქონები ინგლისურად.
- Reason: IMGUI-ის ნაგულისხმევ font-ს ქართული glyph-ები შეიძლება არ ჰქონდეს. ქართული UI სწორ font-თან (TextMeshPro font asset) ერთად ფაზა 2-ში.

### 2026-10-25 — გარდაქმნა: OnGUI არჩევანი
- Decision: Core უჭერს მხარს Q/R/B/N-ს. UI აჩვენებს 4 ღილაკს და Cancel-ს (Esc). ავტომატური დედოფალი არ არის.
- Reason: უმარტივესი სრული ვარიანტი. კლავიშები Q/R/B/N არ გამოიყენება, რადგან R უკვე restart-ია.

### 2026-10-25 — კამერა: ერთი კამერა, ორი გამოთვლილი პოზა
- Decision:
  - `CameraRig` `Main Camera`-ზე. სარდლის პოზა = ადამიანის მეფის უჯრა + `commanderEyeHeight` ზემოთ − `commanderBackOffset` უკან, მიმართული მოწინააღმდეგისკენ, `commanderPitch`° ქვემოთ;
  - ტაქტიკური პოზა = დაფის ცენტრი − forward·`tacticalDistance` + up·`tacticalHeight`, დაფის ცენტრისკენ მიმართული;
  - exponential lerp; TAB ცვლის ხედს; `returnToCommanderAfterMove` (ნაგულისხმევად true) ადამიანის სვლის შემდეგ აბრუნებს სარდლის ხედზე;
  - click ორივე ხედში მუშაობს.
- Reason: scene-ში anchor ობიექტები არ არის საჭირო, მეფის ტრანსფორმაციას (როქს) ავტომატურად მიჰყვება.
- How to change: orbit / zoom — Backlog.

### 2026-10-25 — owner-ები, დიზაინი და E2E
- Decision:
  - Core, AI და Game task-ების owner = `task-executor` (ბიბლიოთეკა/თამაშის კლიენტი, არა web UI ან სერვერი);
  - designer, DESIGN.md და Playwright არ გამოიყენება, რადგან primitive-ები დროებითია და Unity-ს Playwright ვერ ამოწმებს;
  - Editor-ის ნაბიჯები და Play-mode შემოწმება = USER task-ები.
- Reason: აგენტებს Unity Editor-ის გაშვება და დანახვა არ შეუძლიათ.

### 2026-10-25 — სცენა: MainScene
- Decision: თამაში `Assets/Scenes/MainScene.unity`-შია. placeholder `ChessSmokeTest.cs` (და `.meta`) იშლება task 9-ში; USER Main Camera-დან შლის „Missing script“ კომპონენტს.
- Reason: MainScene უკვე შეიცავს მომხმარებლის smoke-test-ს, ანუ ეს არის განზრახული სცენა.

### 2026-10-25 — Game-ის კომპილაციის შემოწმება Unity-ის გარეშე
- Decision: `Tests/GameCompileCheck/GameCompileCheck.csproj` (netstandard2.1, LangVersion 9.0, ImplicitUsings disable) აკომპილირებს `Assets/Scripts/Game/**/*.cs`-ს Unity-ის DLL-ებით: `$(UnityEditorDataDir)` (ნაგულისხმევი `C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Data`) → `Managed/UnityEngine/*.dll`, და `Library/ScriptAssemblies/Unity.InputSystem.dll` (მხოლოდ კითხვა). Core/AI — ProjectReference build პროექტებზე. ბრძანება: `dotnet build Tests/GameCompileCheck`. `dotnet test Tests/ChessCore.Tests`-ში არ შედის.
- Reason: აგენტებს Unity Editor არ აქვთ; ასე Game-ის კომპილაციის შეცდომები USER-ის შემოწმებამდე ჩანს.
- Limits: საჭიროა Unity-ის დაყენება ამ გზაზე და `Library/` ერთხელ მაინც დაგენერირებული (Unity-ის გახსნა). Unity-ის serialization/runtime ქცევას არ ამოწმებს — ეს ისევ USER task-ებია.
- How to change: სხვა Unity გზა → `dotnet build Tests/GameCompileCheck -p:UnityEditorDataDir=<path>`.

### 2026-10-25 — USER შემოწმებები აგენტის task-ებს არ აჩერებს
- Decision: task 9 ფორმალურად task 2-ზეა დამოკიდებული (Unity-ის პირველი გახსნა). `.meta` ფაილები უკვე შეიქმნა (Unity გაიხსნა), Console-ის შემოწმება USER-ის დადასტურებას ელოდება. აგენტის Game task-ები (9, 11, 13, 15) გრძელდება, USER task-ები (2, 8, 10, 12, 14, 16) მომხმარებელს რჩება თანმიმდევრობით.
- Reason: Game-ის კოდი Core-ზე და API Contract-ზეა დამოკიდებული, არა Editor-ის მდგომარეობაზე; კომპილაცია მოწმდება `Tests/GameCompileCheck`-ით.

### 2026-10-25 — Restart AI-ის ფიქრისას: ახალი StockfishAI
- Decision: R-ზე, თუ AI ფიქრობს, ძველი `StockfishAI` ფონურად Dispose-დება და იქმნება ახალი (ახალი პროცესი + InitializeAsync). თუ არ ფიქრობს — იგივე instance რჩება.
- Reason: გაუქმებული ძებნა (`stop` + `bestmove`-ის მოლოდინი ≤ 2 s) ჯერ კიდევ იკავებს instance-ს; humanSide = Black-ის დროს AI-ის ახალი სვლა მაშინვე `InvalidOperationException`-ს მიიღებდა და თამაში არასწორად გადავიდოდა hot-seat-ზე. (finish-check-მა აღმოაჩინა კონტრაქტთან შეუსაბამობა; კოდის ქცევა სწორია, კონტრაქტი განახლდა.)
- How to change: StockfishAI-ს გაუქმების ასინქრონული დასრულების API, რის შემდეგაც instance-ის ხელახლა გამოყენება შეიძლება.

## Open Questions
- ვარაუდი: პლატფორმა მხოლოდ Windows PC (x86-64). Stockfish-ის binary მხოლოდ Windows-ისთვის.
- ვარაუდი: ადამიანი ნაგულისხმევად თეთრია (`humanSide` inspector-ში იცვლება); ნაგულისხმევი სირთულე = Medium.
- ვარაუდი: პარტიის შენახვა, undo, საათი და PGN MVP-ში არ არის.
- ვარაუდი: HUD-ის ტექსტი MVP-ში ინგლისურადაა (font-ის რისკი); ქართული UI ფაზა 2-ში.
- ვარაუდი: Easy ≈ 800 Elo მიახლოებითია და გაზომილი არ არის; ბალანსის ტესტი ფაზა 3-ში.
- ვარაუდი: ფრე 50 სვლაზე და სამჯერად გამეორებაზე ავტომატურია (claim-ის გარეშე).
- ვარაუდი: hot-seat რეჟიმში კამერა არ ტრიალდება მოთამაშის მიხედვით.
- გადასაწყვეტი build-ის გავრცელებამდე: Stockfish GPLv3, ანუ build-თან ერთად გავრცელება მოითხოვს GPL-ის პირობების დაცვას.
- ტექნიკური შენიშვნა: პირველ `dotnet build`-ს NuGet restore-ისთვის (xUnit) ინტერნეტი სჭირდება.

## Accepted Risks
