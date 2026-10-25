# Progress

## Current Stage
Stage 1 — MVP

## Tasks
<!-- ფორმატი: - [ ] N. Title — owner: task-executor | USER — files: ... — depends: ... — done when: ...
     USER task = Unity Editor-ში ხელით შესასრულებელი ნაბიჯები (აგენტი მათ ვერ აკეთებს). დეტალური ინსტრუქცია task-ის ქვეშაა.
     ყოველი Game task (9, 11, 13, 15) საბოლოოდ მოწმდება მომდევნო USER task-ით (Console = 0 შეცდომა + Play-mode შემოწმება).
     API-ს, ველების სახელების და ქცევის ერთადერთი წყარო: PROJECT.md → ## API Contract. -->

- [x] 1. Test infrastructure, .gitignore, asmdef skeleton — owner: task-executor — files: .gitignore, Tests/ChessCore.Build/ChessCore.Build.csproj, Tests/ChessAI.Build/ChessAI.Build.csproj, Tests/ChessCore.Tests/ChessCore.Tests.csproj, Tests/ChessCore.Tests/BuildTargetTests.cs, Assets/Scripts/Core/Chess.Core.asmdef, Assets/Scripts/AI/Chess.AI.asmdef, Assets/Scripts/Game/Chess.Game.asmdef, Assets/StreamingAssets/Stockfish/README.md — depends: — — done when:
  - `dotnet build Tests/ChessCore.Tests` და `dotnet test Tests/ChessCore.Tests` წარმატებით სრულდება; ტესტი `BuildTargetTests.CoreAndAiTargetNetStandard21` გადის (`Assembly.Load("Chess.Core")` და `"Chess.AI"` → `TargetFrameworkAttribute` = `.NETStandard,Version=v2.1`).
  - Build პროექტები ზუსტად PROJECT.md-ის Decision Log-ის მიხედვით: netstandard2.1, LangVersion 9.0, Nullable disable, ImplicitUsings disable, EnableDefaultCompileItems false, Compile Include `../../Assets/Scripts/Core/**/*.cs` / `../../Assets/Scripts/AI/**/*.cs`; ChessAI.Build → ProjectReference ChessCore.Build; ChessCore.Tests: net10.0, xUnit 2.x + xunit.runner.visualstudio + Microsoft.NET.Test.Sdk (ზუსტი ვერსიებით), ProjectReference ორივეზე.
  - asmdef-ები: `Chess.Core` (rootNamespace Chess.Core, noEngineReferences true, references []); `Chess.AI` (noEngineReferences true, references ["Chess.Core"]); `Chess.Game` (references ["Chess.Core","Chess.AI","Unity.InputSystem"]).
  - `.gitignore`-ის ბოლოს დამატებულია: `!Tests/**/*.csproj`, `Tests/**/bin/`, `Tests/**/obj/`, `.env`, `.env.*`, `!.env.example`, `/Assets/StreamingAssets/Stockfish/*.exe`, `/Assets/StreamingAssets/Stockfish/*.exe.meta`; არსებული ხაზები შეუცვლელია. `git status --porcelain` build-ის შემდეგ აჩვენებს 3 csproj-ს და არცერთ `bin/` ან `obj/` ფაილს.
  - README.md (ქართულად) ხსნის, რომ აქ უნდა იდოს `stockfish.exe`, ხოლო ჩამოტვირთვის ინსტრუქცია PROGRESS.md-ის task 8-შია.
  - დეველოპერმა ერთხელ შეამოწმა დაცვა **რეპოზიტორიის გარეთ** (დროებით საქაღალდეში, მაგ. `%TEMP%`-ში; Assets-ში ფაილი არ იქმნება, რომ Unity-მ `.meta` არ შექმნას): ChessCore.Build.csproj-ის იგივე პარამეტრებით (netstandard2.1, LangVersion 9.0, ImplicitUsings disable) და ფაილით `public record R(int X);` build ჩავარდა CS0518-ით. დროებითი საქაღალდე წაიშალა. შედეგი report-შია.
  - `.meta` ფაილები ხელით არ შექმნილა.

- [x] 2. USER: Unity-ის პირველი გახსნა ახალი საქაღალდეების შემდეგ — owner: USER — files: (Unity თავად ქმნის .meta ფაილებს) — depends: 1 — done when: Console-ში 0 წითელი შეცდომაა და ახალ საქაღალდეებს/asmdef-ებს .meta აქვთ
    1. გახსენი **Unity Hub** → მარცხნივ **Projects** → დააჭირე პროექტს **chess3D** (ვერსია 6000.6.7f1). თუ სიაში არ ჩანს: **Add** → **Add project from disk** → აირჩიე `D:\yvelaperi\saitebi\chess\chess3D\chess3D`.
    2. დაელოდე, სანამ ქვედა მარჯვენა კუთხეში პროგრესის ზოლი (import / compiling) გაქრება. პირველ ჯერზე რამდენიმე წუთი შეიძლება დასჭირდეს.
    3. გახსენი Console: ზედა მენიუ **Window → General → Console** (ან Ctrl+Shift+C). Console-ის ფანჯარაში დააჭირე **Clear**-ს.
    4. შეამოწმე Console-ის ზედა მარჯვენა კუთხე: წითელი ძახილის ნიშნის (შეცდომა) მთვლელი უნდა იყოს **0**. ყვითელი გაფრთხილებები (warnings) ახლა არ გვაინტერესებს.
    5. ქვემოთ **Project** ფანჯარაში გახსენი `Assets/Scripts/Core`. იქ უნდა ჩანდეს `Chess.Core` (ფაზლის ხატულა). დააჭირე და მარჯვნივ **Inspector**-ში დარწმუნდი, რომ **No Engine References** მონიშნულია. იგივე შეამოწმე `Assets/Scripts/AI/Chess.AI`-ზე.
    6. შეამოწმე, რომ `Assets/StreamingAssets/Stockfish/README` ჩანს.
    7. თუ წითელი შეცდომაა: დააჭირე მას, დააკოპირე ტექსტი (Ctrl+C ქვედა ნაწილში) და გადაეცი გუნდს. თავად არაფერი შეცვალო.
    8. ახსნა: Unity ყოველ ახალ ფაილს და საქაღალდეს უქმნის `.meta` ფაილს (მისი „პირადობის მოწმობა“, GUID). ეს ფაილები git-ში აუცილებლად უნდა მოხვდეს და ხელით არასდროს იქმნება.

- [x] 3. Core: Square, Piece, Move, Position + FEN — owner: task-executor — files: Assets/Scripts/Core/Side.cs, Assets/Scripts/Core/Piece.cs, Assets/Scripts/Core/Square.cs, Assets/Scripts/Core/Move.cs, Assets/Scripts/Core/CastlingRights.cs, Assets/Scripts/Core/Position.cs, Tests/ChessCore.Tests/Core/SquareTests.cs, Tests/ChessCore.Tests/Core/MoveTests.cs, Tests/ChessCore.Tests/Core/FenTests.cs — depends: 1 — done when:
  - `dotnet build Tests/ChessCore.Tests` და `dotnet test Tests/ChessCore.Tests` გადის.
  - ტიპები და ხელმოწერები ზუსტად API Contract-ის მიხედვით. `Position` არის `public sealed partial class`, public API მხოლოდ კითხვისთვის; `PieceType`/`Piece` → Piece.cs, `MoveFlags` → Move.cs, `Side`+`SideExtensions` → Side.cs.
  - ტესტები:
    - `SquareTests`: ყველა 64 უჯრაზე Parse/ToString round-trip; a1=0, h8=63, File/Rank; არასწორი სახელები (`"i1"`, `"a9"`, `"E4"`, `""`, null) → FormatException ან TryParse = false.
    - `MoveTests`: ToUci/TryParseUci round-trip (`e2e4`, `e7e8q`, `a2a1n`); არასწორი (`e2e9`, `e7e8k`, `e2`, `e2e4qq`) → false; ტოლობა Flags-ს უგულებელყოფს.
    - `FenTests`: round-trip startpos-ზე, Kiwipete-ზე, pos3, pos4, pos5 (PROJECT.md → Testing Strategy); ცალკე `FormatException` თითოეულ არასწორ შემთხვევაზე, რომელიც FEN rules-შია ჩამოთვლილი (არასწორი ველების რაოდენობა, რიგის სიგრძე, უცნობი სიმბოლო, მეფის გარეშე ან ორი მეფე, პაიკი 1/8 რიგზე, სვლის რიგი, როქის სტრიქონი / უფლება მეფის ან ეტლის გარეშე, en passant-ის რიგი, უარყოფითი halfmove, fullmove 0).
  - `using UnityEngine` / `record` / `init` არ გამოიყენება.

- [x] 4. Core: სვლების გენერაცია + perft — owner: task-executor — files: Assets/Scripts/Core/Position.Moves.cs, Assets/Scripts/Core/Perft.cs, Tests/ChessCore.Tests/Core/MoveGenerationTests.cs, Tests/ChessCore.Tests/Core/PerftTests.cs — depends: 3 — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის.
  - `PerftTests` (xUnit Theory) ამოწმებს PROJECT.md → Testing Strategy-ის ყველა რიცხვს: startpos d1–4, Kiwipete d1–3, pos3 d1–4, pos4 d1–3, pos5 d1–3. startpos d5 ჩართულია მხოლოდ მაშინ, თუ < 5 s.
  - `MoveGenerationTests`:
    - en passant შეკრული პაიკით: `8/8/8/KPp4r/8/8/8/7k w - c6 0 1` → `b5c6` დასაშვებ სვლებში არ არის;
    - როქი არ შეიძლება შახის დროს, შეტევის ქვეშ მყოფ უჯრაზე გავლით ან მოხვედრით;
    - ეტლის მოჭრის შემდეგ შესაბამისი როქის უფლება ქრება;
    - გარდაქმნისას გენერირდება 4 სვლა;
    - `IsInCheck` და `IsSquareAttacked` მარტივ პოზიციებზე.
  - მთელი ტესტ-run 30 წამამდეა. დრო report-შია.

- [x] 5. Core: GameState, თამაშის დასრულების წესები, UCI სვლის resolve — owner: task-executor — files: Assets/Scripts/Core/GameState.cs, Assets/Scripts/Core/GameStatus.cs, Assets/Scripts/Core/DrawRules.cs, Tests/ChessCore.Tests/Core/GameStateTests.cs, Tests/ChessCore.Tests/Core/DrawRulesTests.cs — depends: 4 — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის. API და სტატუსის რიგი ზუსტად PROJECT.md-ის Business Rules-ის და API Contract-ის მიხედვით.
  - `GameStateTests`:
    - fool's mate (`f2f3 e7e5 g2g4 d8h4`) → Checkmate, BlackWins, IsCheck;
    - პატის პოზიცია → Stalemate, Draw;
    - halfmove 99 + მშვიდი სვლა → FiftyMoveRule;
    - მატი 100-ე halfmove-ზე → Checkmate (მატს უპირატესობა აქვს);
    - `g1f3 g8f6 f3g1 f6g8` ×2 → ThreefoldRepetition;
    - en passant უფლება, რომლის გამოყენებაც შეუძლებელია, გამეორების გასაღებს არ ცვლის;
    - არასწორი სვლა → false და FEN არ იცვლება;
    - თამაშის დასრულების შემდეგ ნებისმიერი სვლა → false, LegalMoves ცარიელია;
    - გარდაქმნის სვლა promotion-ის გარეშე → false; `TryMakeUciMove("e7e8q")` მუშაობს;
    - `TryResolveUciMove` აბრუნებს Flags-ს (როქი, en passant);
    - `GameState(fen)`, სადაც იმ მხარის მეფე, რომლის სვლაც არ არის, შახშია → FormatException.
  - `DrawRulesTests`: K v K, KB v K, KN v K, KB v KB ერთ ფერზე → InsufficientMaterial; KB v KB სხვადასხვა ფერზე, KNN v K, K+P v K → InProgress.

- [x] 6. AI: IChessAI, AIDifficulty, UCI პროტოკოლი (binary-ს გარეშე) — owner: task-executor — files: Assets/Scripts/AI/IChessAI.cs, Assets/Scripts/AI/AIDifficulty.cs, Assets/Scripts/AI/UciProtocol.cs, Assets/Scripts/AI/IUciTransport.cs, Assets/Scripts/AI/ChessAIException.cs, Tests/ChessCore.Tests/AI/UciProtocolTests.cs, Tests/ChessCore.Tests/AI/AIDifficultyTests.cs — depends: 3 — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის. ტიპები ზუსტად API Contract-ის მიხედვით: `DifficultyPreset` → AIDifficulty.cs, `UciOption` → UciProtocol.cs, `StockfishNotFoundException` → ChessAIException.cs.
  - `UciProtocolTests`:
    - `TryParseBestMove`: `bestmove e2e4 ponder e7e5`, `bestmove e7e8q`, `bestmove (none)`, `info depth 5 ...` → false;
    - `TryParseOption`: spin option, სახელი space-ით (`Skill Level`), არა-spin option;
    - `BuildDifficultyCommands`: Elo იჭრება engine-ის min/max-ზე (მაგ. 800 → 1320, როცა min = 1320); უცნობი option-ები გამოტოვებულია;
    - `BuildGo`: depth vs movetime;
    - `BuildPosition`.
  - `AIDifficultyTests`: 4 preset-ის მნიშვნელობები ცხრილის მიხედვით; დიაპაზონს გარეთ → ArgumentOutOfRangeException.

- [x] 7. AI: StockfishAI + ProcessUciTransport — owner: task-executor — files: Assets/Scripts/AI/StockfishAI.cs, Assets/Scripts/AI/ProcessUciTransport.cs, Tests/ChessCore.Tests/AI/FakeUciTransport.cs, Tests/ChessCore.Tests/AI/StockfishAITests.cs, Tests/ChessCore.Tests/AI/StockfishIntegrationTests.cs — depends: 5, 6 — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის. ქცევა ზუსტად API Contract-ის „StockfishAI protocol“-ის და Error model-ის მიხედვით.
  - `StockfishAITests` (`FakeUciTransport`-ით, scripted პასუხებით):
    - handshake-ის რიგი: `uci` → `uciok` → setoption-ები → `isready` → `readyok`;
    - `go` ბრძანება შეესაბამება სირთულეს;
    - `bestmove e1g1` ბრუნდება როგორც Move `CastleKingside` flag-ით;
    - არასწორი bestmove → ChessAIException;
    - timeout (ms-ის timeout-ებით) → ChessAIException `TimeoutException` inner-ით;
    - გაუქმება → OperationCanceledException და `stop` გაგზავნილია;
    - ერთდროული გამოძახება → InvalidOperationException;
    - Dispose → `quit` + transport.Dispose; შემდეგ გამოძახება → ObjectDisposedException;
    - არარსებული გზა → StockfishNotFoundException.
  - `StockfishIntegrationTests.BestMoveFromStartPosition_RequiresStockfish`:
    - როცა `STOCKFISH_PATH` დაყენებულია: Easy, start FEN → დასაშვები სვლა 10 s-ში; Dispose-ის შემდეგ `HasExited == true`;
    - როცა არ არის დაყენებული: იწერება `SKIPPED: STOCKFISH_PATH not set` და ტესტი ბრუნდება. report-ში ეს ტესტი მოინიშნება როგორც „შეუმოწმებელი“.
  - `System.Threading.Channels` და `Process.Kill(bool)` არ გამოიყენება (netstandard2.1-ში არ არის).

- [x] 8. USER: Stockfish-ის ჩამოტვირთვა და დადება — owner: USER — files: Assets/StreamingAssets/Stockfish/stockfish.exe (git-ში არ ინახება) — depends: 7 — done when: ფაილი სწორ ადგილასაა და integration ტესტი გადის SKIPPED-ის გარეშე
    1. ბრაუზერში გახსენი ოფიციალური საიტი **https://stockfishchess.org/download/** (სხვა საიტიდან არ ჩამოტვირთო).
    2. აირჩიე **Windows** და ჩამოტვირთე **x86-64-avx2** ვერსია (თანამედროვე პროცესორების უმეტესობაზე მუშაობს). თუ მოგვიანებით თამაში იტყვის, რომ engine ვერ გაეშვა ან გაითიშა, ჩამოტვირთე უბრალო **x86-64** ვერსია.
    3. ჩამოტვირთულ `.zip`-ზე: მარჯვენა click → **Extract All** → **Extract**.
    4. გახსნილ საქაღალდეში იპოვე ფაილი, რომლის სახელიც იწყება `stockfish-windows-x86-64` და მთავრდება `.exe`-ით.
    5. გადაარქვი სახელი **`stockfish.exe`**: მარჯვენა click → Rename. თუ გაფართოებები არ ჩანს, ჩაწერე უბრალოდ `stockfish`.
    6. დააკოპირე იგი საქაღალდეში `D:\yvelaperi\saitebi\chess\chess3D\chess3D\Assets\StreamingAssets\Stockfish\` (იქ, სადაც `README.md` დევს).
    7. (არასავალდებულო) ორჯერ დააჭირე `stockfish.exe`-ს. თუ Windows SmartScreen გააფრთხილებს: **More info → Run anyway** (მხოლოდ იმიტომ, რომ ოფიციალური საიტიდანაა). შავ ფანჯარაში ჩაწერე `uci` და Enter: ბოლოს უნდა გამოჩნდეს `uciok`. შემდეგ ჩაწერე `quit` და Enter.
    8. integration ტესტი PowerShell-ში (სამი ბრძანება, თითო ცალკე):
       `cd D:\yvelaperi\saitebi\chess\chess3D\chess3D`
       `$env:STOCKFISH_PATH = "D:\yvelaperi\saitebi\chess\chess3D\chess3D\Assets\StreamingAssets\Stockfish\stockfish.exe"`
       `dotnet test Tests/ChessCore.Tests --filter "FullyQualifiedName~StockfishIntegration" --logger "console;verbosity=detailed"`
       შედეგი: `Passed` და არცერთი `SKIPPED` ხაზი.
    9. ახსნა: `.exe` git-ში არ მიდის (`.gitignore` გამორიცხავს), რადგან დიდია და GPLv3 ლიცენზიისაა. ყველა კომპიუტერზე ეს ნაბიჯი ხელით კეთდება.

- [x] 9. Game: ველი, ბადე, ფიგურები, GameController-ის ჩონჩხი — owner: task-executor — files: Assets/Scripts/Game/BoardCoordinates.cs, Assets/Scripts/Game/SquareRef.cs, Assets/Scripts/Game/BoardView.cs, Assets/Scripts/Game/PiecesView.cs, Assets/Scripts/Game/GameController.cs, Assets/Scripts/ChessSmokeTest.cs (delete), Assets/Scripts/ChessSmokeTest.cs.meta (delete), Tests/GameCompileCheck/GameCompileCheck.csproj — depends: 2, 5 — done when:
  - `dotnet build Tests/GameCompileCheck` გადის 0 შეცდომით (Game სკრიპტების კომპილაცია Unity-ის DLL-ებით, Unity Editor-ის გარეშე; იხ. Decision Log „Game-ის კომპილაციის შემოწმება“).
  - კოდი API Contract → Chess.Game-ის მიხედვით: coordinate mapping, ფიგურების ფორმები, inspector ველების ზუსტი სახელები.
  - `BoardView.Awake` ქმნის ველს და 64 tile-ს (`Tile_a1` … `Tile_h8`, SquareRef-ით); `ShowSelection`/`ClearHighlights` მზადაა.
  - `PiecesView.Sync(Position)`.
  - `GameController` (ამ ეტაპზე): ველები `boardView`, `piecesView`, `startFen`; Start-ში GameState → Sync; R (`Keyboard.current.rKey`) → ახალი GameState → Sync.
  - ChessSmokeTest.cs და მისი .meta წაშლილია.
  - Grep `Assets/Scripts/Game`-ში: `UnityEngine\.Input\b|\bInput\.(Get|mouse|any)` → 0 შედეგი.
  - Game-ში არ არის ჭადრაკის წესები (მხოლოდ GameState/Position-ის გამოძახება).
  - `dotnet test Tests/ChessCore.Tests` კვლავ გადის.
  - საბოლოო შემოწმება: USER task 10.

- [ ] 10. USER: სცენის აწყობა (ერთი ბრძანებით) + ველი და ფიგურები — owner: USER — files: Assets/Scenes/MainScene.unity, Assets/Materials/** (ქმნის Tools → Setup Chess Scene) — depends: 9, 19 — done when: Play-ში ჩანს ველი, ბადე და 32 ფიგურა სწორ ადგილებზე; Console = 0 შეცდომა
    1. გადადი Unity-ის ფანჯარაზე და დაელოდე, სანამ სკრიპტები დაკომპილირდება (ქვედა მარჯვენა კუთხის პროგრესის ზოლი ქრება). Console → **Clear** → 0 წითელი შეცდომა.
    2. ზედა მენიუ: **Tools → Setup Chess Scene**. თუ სხვა სცენაში შენახული ცვლილებები გაქვს, Unity შეგეკითხება შენახვას — დააჭირე **Save**. ბრძანება თავად გახსნის `MainScene`-ს და გააკეთებს task 10, 12, 14, 16-ის მთელ აწყობას (მასალები, Main Camera + CameraRig, `ChessGame` ყველა კომპონენტით და კავშირით, GameController-ის პარამეტრები) და შეინახავს სცენას.
    3. Console-ში გამოჩნდება ჩანაწერი `[ChessSceneSetup] ...` სიით, რა შეიქმნა და რა განახლდა. ბრძანება შეგიძლია მეორედაც გაუშვა — დუბლიკატებს არ ქმნის, მხოლოდ აახლებს.
    4. (დამწყებისთვის) დააჭირე Hierarchy-ში `ChessGame`-ს და Inspector-ში ნახე კომპონენტები: BoardView, PiecesView, SquarePicker, PromotionPrompt, SelectionController, StatusHud, GameController — ველები უკვე შევსებულია. **Main Camera**-ზე დევს CameraRig.
    5. დააჭირე **Play**-ს (▶ ზედა ცენტრში). უნდა ჩანდეს: მწვანე ველი, ოდნავ შესამჩნევი 8x8 ბადე და 32 ფიგურა. თეთრი ფიგურები კამერასთან ახლოსაა; თეთრი დედოფალი (cylinder + sphere) d1-ზეა, მეფის (ყველაზე მაღალი, „ჯვრით“) მარცხნივ.
    6. Play-ში დააჭირე **R**-ს: ფიგურები თავიდან ჩნდება, Console-ში შეცდომა არ ჩნდება.
    7. ისევ დააჭირე ▶-ს Play-ის გასაჩერებლად. მნიშვნელოვანი: Play-ის დროს Inspector-ში გაკეთებული ცვლილებები გაჩერებისას იკარგება. აწყობა ყოველთვის Play-ს გარეშე გააკეთე.
    8. თუ რამე არ ემთხვევა, გადაუღე screenshot-ი და Console-ის ტექსტი გადაეცი გუნდს.

- [x] 11. Game: მონიშვნა, განათება, სვლა, გარდაქმნა (hot-seat) — owner: task-executor — files: Assets/Scripts/Game/SquarePicker.cs, Assets/Scripts/Game/SelectionController.cs, Assets/Scripts/Game/PromotionPrompt.cs, Assets/Scripts/Game/GameController.cs (extends task 9) — depends: 9 — done when:
  - კოდი API Contract-ის მიხედვით.
  - `SquarePicker`: `Mouse.current`, raycast → `GetComponentInParent<SquareRef>()`.
  - `SelectionController`: წესები API Contract-ის ცხრილიდან; მონიშნავს მხოლოდ `SideToMove`-ის ფიგურას; click-ებს უგულებელყოფს, როცა `!Interactable` ან `PromotionPrompt.IsOpen`.
  - `PromotionPrompt`: OnGUI 4 ღილაკი + Cancel + Esc.
  - `GameController`-ს დაემატა `selection` ველი, `MoveChosen` → `TryMakeMove` → Sync + Bind; R restart-ს არ აკეთებს, როცა PromotionPrompt ღიაა.
  - Grep legacy Input → 0 შედეგი. `dotnet test` გადის.
  - საბოლოო შემოწმება: USER task 12.

- [ ] 12. USER: სვლების შემოწმება — owner: USER — files: Assets/Scenes/MainScene.unity — depends: 10, 11 — done when: ყველა ქვემოთ ჩამოთვლილი შემოწმება გადის; Console = 0 შეცდომა
    1. აწყობა უკვე გაკეთებულია (task 10 → **Tools → Setup Chess Scene**). Console **Clear** → 0 შეცდომა.
    2. ამ task-ში ორივე მხარეს შენ თამაშობ: Hierarchy → `ChessGame` → **GameController** → მოხსენი მონიშვნა **Play Against AI**-ს (Play-ს გარეშე). task-ის ბოლოს (ნაბიჯი 8) დააბრუნე ✓.
    3. Play → დააჭირე თეთრ პაიკს e2-ზე: e2 გაყვითლდება, e3 და e4 გამწვანდება. დააჭირე e4-ს: პაიკი გადავა. ახლა შავის სვლაა (ორივე მხარეს შენ თამაშობ): დააჭირე e7-ს, შემდეგ e5-ს.
    4. დააჭირე საკუთარ ფიგურას, შემდეგ სხვა საკუთარ ფიგურას: მონიშვნა გადადის. დააჭირე ცარიელ ველს (დაფის გარეთ): მონიშვნა ქრება. მოწინააღმდეგის ფიგურაზე დაჭერით არაფერი ხდება.
    5. გააჩერე Play. **Start Fen**-ში ჩაწერე `r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1` → Play → დააჭირე მეფეს e1-ზე: g1 და c1 მწვანეა → დააჭირე g1-ს: მეფე g1-ზე, ეტლი f1-ზე (როქი).
    6. გააჩერე. Start Fen = `8/P6k/8/8/8/8/8/K7 w - - 0 1` → Play → a7 → a8: ჩნდება ფანჯარა 4 ღილაკით → **Knight** → a8-ზე მხედარია. გაიმეორე და დააჭირე **Cancel**-ს: პაიკი a7-ზე რჩება.
    7. გააჩერე. Start Fen = `4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1` → Play → e5 → d6: შავი პაიკი d5-დან ქრება (en passant).
    8. გააჩერე, **Start Fen** გაასუფთავე (ცარიელი) და **Play Against AI** ისევ მონიშნე ✓. **Ctrl+S**.

- [x] 13. Game: CameraRig — სარდლის და ტაქტიკური ხედი (TAB) — owner: task-executor — files: Assets/Scripts/Game/CameraRig.cs, Assets/Scripts/Game/GameController.cs (extends task 11) — depends: 11 — done when:
  - `CameraRig` API Contract-ის და Decision Log-ის („კამერა“) მიხედვით: ველების სახელები და ნაგულისხმევი მნიშვნელობები, TAB (`Keyboard.current.tabKey`), exponential lerp, `returnToCommanderAfterMove`.
  - `GameController`-ს დაემატა `cameraRig` და `humanSide`; ყოველი Sync-ის შემდეგ `SetCommander(state.Position.FindKing(humanSide), humanSide)`; ადამიანის სვლის შემდეგ `NotifyHumanMoveMade()`.
  - Grep legacy Input → 0 შედეგი. `dotnet test` გადის.
  - საბოლოო შემოწმება: USER task 14.

- [ ] 14. USER: კამერის შემოწმება — owner: USER — files: Assets/Scenes/MainScene.unity — depends: 12, 13 — done when: ქვემოთ ჩამოთვლილი შემოწმებები გადის; Console = 0 შეცდომა
    1. აწყობა უკვე გაკეთებულია (**Tools → Setup Chess Scene**: CameraRig Main Camera-ზე, GameController → Camera Rig მიბმულია, Human Side = White). Console **Clear** → 0 შეცდომა.
    2. Hierarchy → **Main Camera** → Inspector-ში დარწმუნდი, რომ **Camera Rig** კომპონენტი ჩანს (ნაგულისხმევი მნიშვნელობებით).
    3. Play: კამერა თეთრი მეფის (e1) უკან და ზემოთ დგას და შავისკენ იყურება (სარდლის ხედი).
    4. დააჭირე **TAB**-ს: კამერა გლუვად ადის დაფის ზემოთ (ტაქტიკური ხედი). კიდევ ერთხელ TAB: ბრუნდება.
    5. ტაქტიკურ ხედში გააკეთე სვლა: კამერა თავად ბრუნდება სარდლის ხედზე.
    6. გააჩერე, მოხსენი მონიშვნა **Return To Commander After Move**-ს → Play → ტაქტიკურ ხედში სვლის შემდეგ კამერა ადგილზე რჩება. დააბრუნე მონიშვნა.
    7. სარდლის ხედშიც შეგიძლია ფიგურაზე დაჭერა და სვლა.
    8. გააჩერე. **Human Side** = `Black` → Play: კამერა შავი მეფის (e8) უკანაა და თეთრისკენ იყურება. დააბრუნე `White`. **Ctrl+S**.

- [x] 15. Game: AI-ის რიგი, StatusHud, Stockfish-ის fallback, restart — owner: task-executor — files: Assets/Scripts/Game/StatusHud.cs, Assets/Scripts/Game/GameController.cs (extends task 13) — depends: 7, 13 — done when:
  - `GameController`-ს დაემატა `hud`, `playAgainstAI`, `difficulty`, `stockfishRelativePath`, `stockfishPathOverride` და მისი ნაკადი ზუსტად API Contract → „GameController-ის ნაკადი“-ის მიხედვით: generation მთვლელი, `CancellationTokenSource`, input-ის დაბლოკვა AI-ის სვლისას, try/catch ყოველ async void-ში.
  - `StockfishNotFoundException`/`ChessAIException` → HUD-ის შეცდომის ხაზი + `Debug.LogException` + hot-seat.
  - `OnDestroy` → Cancel + Dispose AI.
  - `StatusHud`: ტექსტები API Contract-იდან.
  - გზა: `stockfishPathOverride` თუ ცარიელი არ არის, სხვა შემთხვევაში `Path.Combine(Application.streamingAssetsPath, stockfishRelativePath)`.
  - Grep legacy Input → 0 შედეგი. `dotnet test` გადის.
  - საბოლოო შემოწმება: USER task 16.

- [ ] 16. USER: საბოლოო აწყობა და MVP-ის მიღება — owner: USER — files: Assets/Scenes/MainScene.unity — depends: 8, 14, 15, 17 — done when: ყველა შემოწმება გადის; Console = 0 შეცდომა; Play-ის შემდეგ stockfish.exe პროცესი აღარ არსებობს
    1. აწყობა უკვე გაკეთებულია (**Tools → Setup Chess Scene**: StatusHud + Hud კავშირი). Console **Clear** → 0 შეცდომა.
    2. შეამოწმე GameController (ბრძანება ამას აყენებს; თუ ხელით შეცვალე, ხელახლა გაუშვი **Tools → Setup Chess Scene**): **Play Against AI** ✓, **Difficulty** = `Easy`, **Human Side** = `White`, **Stockfish Relative Path** = `Stockfish/stockfish.exe`, **Stockfish Path Override** ცარიელი.
    3. Play: ზედა-მარცხენა კუთხეში წერია `White to move`. გააკეთე სვლა: სტატუსი იცვლება (`AI thinking...` შეიძლება ძალიან სწრაფად გაქრეს), შავი პასუხობს 1–2 წამში.
    4. **Difficulty** = `Master` (Play-ს გარეშე) → Play → სვლის შემდეგ, სანამ `AI thinking...` წერია, დააჭირე ფიგურებს: არაფერი ხდება (input დაბლოკილია).
    5. AI-ის ფიქრისას დააჭირე **R**-ს: თამაში თავიდან იწყება, ძველი AI-ის სვლა ახალ დაფაზე არ ჩნდება, Console-ში შეცდომა არაა.
    6. მატი: Start Fen = `6k1/5ppp/8/8/8/8/5PPP/R5K1 w - - 0 1` → Play → a1 → a8 → სტატუსი `Checkmate - White wins`; ფიგურები აღარ ინიშნება.
    7. პატი: Start Fen = `7k/8/8/5Q2/8/8/8/K7 w - - 0 1` → Play → f5 → f7 → `Stalemate - draw`.
    8. მასალის ნაკლებობა: Start Fen = `7k/8/8/8/8/8/1q6/K7 w - - 0 1` → Play → სტატუსი აჩვენებს `Check!` → a1 → b2 → `Draw - insufficient material`.
    9. Stockfish-ის გარეშე: **Stockfish Path Override** = `C:\nope\stockfish.exe` → Play → ჩანს შეცდომა `Stockfish not found ...` და ორივე მხარეს მაუსით თამაშობ. გაასუფთავე Override და Start Fen. **Ctrl+S**.
    10. **Human Side** = `Black` → Play → თეთრი (AI) პირველი აკეთებს სვლას. დააბრუნე `White`.
    11. Play-ის გაჩერების შემდეგ გახსენი Task Manager (Ctrl+Shift+Esc) → **Details** → სიაში `stockfish.exe` არ უნდა იყოს.
    12. თუ ყველაფერი გავიდა, MVP მზადაა. გაუშვი `/finish-check`.

### Finish-check-ის task-ები (2026-10-25)
- [x] 17. [should-fix] Game: გარდაქმნა ჩუმი არჩევის გარეშე + Stockfish-ის Dispose domain reload-ზე — owner: task-executor — files: Assets/Scripts/Game/GameController.cs, Assets/Scripts/Game/SelectionController.cs — depends: 15 — done when:
  - `SelectionController`: თუ გარდაქმნის სვლაა და `promotionPrompt` მიბმული არ არის → `Debug.LogError("PromotionPrompt is not assigned on SelectionController")`, სვლა არ იგზავნება, მონიშვნა იხსნება (ჩუმად მხედრის/დედოფლის არჩევა აღარ ხდება — API Contract: „ავტომატური დედოფალი არ არის“).
  - `GameController.OnDisable`: AI-ის გაუქმება + Dispose (`_cts.Cancel()`, `_ai?.Dispose()`, `_ai = null`), რომ Play-ის დროს სკრიპტის recompile-ზე (domain reload: OnDisable იძახება, OnDestroy — არა) `stockfish.exe` ობლად არ დარჩეს; `OnDestroy` იგივეს უსაფრთხოდ (idempotent) აკეთებს. reload-ის შემდეგ თამაში hot-seat-ში გრძელდება ან R-ზე თავიდან იწყება NRE-ს გარეშე (`_state == null` შემთხვევები დაცულია: Update/OnMoveChosen/Refresh).
  - Restart-ის comment გასწორებულია: AI იცვლება, რადგან გაუქმებული ძებნა (`stop` + ≤ 2 s) ჯერ კიდევ `_busy`-ს იკავებს და ახალი `GetBestMoveAsync` `InvalidOperationException`-ს მიიღებდა.
  - იმავე ფაილებში (nit-ები): HUD-ის შეცდომის ტექსტში ორმაგი წერტილი აღარ არის (`ex.Message.TrimEnd('.')`); `FallBackToHotSeat`-ში `failed.Dispose()` გადატანილია `Task.Run`-ში (main thread-ის ≥1 s გაყინვის თავიდან ასაცილებლად); `Refresh`-ში `piecesView` null-check.
  - `dotnet build Tests/GameCompileCheck` → 0 შეცდომა, 0 warning; `dotnet test Tests/ChessCore.Tests` გადის; legacy Input grep → 0.
  - საბოლოო შემოწმება: USER task 16.
- [x] 18. [should-fix] Core: en passant ველის თანმიმდევრულობა FEN-ში + გამეორების გასაღების ტესტი — owner: task-executor — files: Assets/Scripts/Core/Position.cs, Tests/ChessCore.Tests/Core/FenTests.cs, Tests/ChessCore.Tests/Core/GameStateTests.cs — depends: 5 — done when:
  - ბაგი: `4k3/8/8/3PN3/8/8/8/4K3 w - e6 0 1`-ში Core აგენერირებს `d5e6`-ს EnPassant-ად და MakeMove შლის **თეთრ** მხედარს e5-ზე. გასწორება: `Position.FromFen` → `FormatException` (`Invalid FEN: en passant square '<sq>' is inconsistent ...`), თუ ep უჯრა ან მის უკან მდგომი უჯრა (შავის სვლისას/თეთრის სვლისას შესაბამისად) ცარიელი არ არის, ან ep უჯრის წინ არ დგას მოწინააღმდეგის პაიკი, რომელმაც ორმაგი სვლა გააკეთა (თეთრის სვლისას: შავი პაიკი 5-ე რიგზე იმავე file-ზე; შავის სვლისას: თეთრი პაიკი მე-4 რიგზე).
  - ჯერ ტესტი, რომელიც ძველ კოდზე ვარდება: `FenTests`-ში ზემოთ მოცემული FEN → FormatException; ასევე ep, რომლის უკანაც ფიგურაა. არსებული სწორი FEN-ები (`4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1`, perft პოზიციები, ToFen round-trip ორმაგი სვლის შემდეგ) კვლავ გადის.
  - `GameStateTests.UsableEnPassantSquare_DistinguishesPositions` შეიცვალა მნიშვნელოვანი ტესტით: (a) პოზიცია, რომელიც პირველად გამოყენებადი ep-ით ჩნდება, შემდეგ კი ორჯერ ep-ის გარეშე → ჯერ კიდევ InProgress (ep-იანი არ ითვლება იგივედ), და მხოლოდ შემდეგი გამეორებისას ThreefoldRepetition; (b) შეკრული პაიკის შემთხვევა (`8/8/8/KPp4r/...`-ის მსგავსი): გამოუსადეგარი ep გასაღებს არ ცვლის. ტესტი ჩავარდებოდა, თუ ep გასაღებიდან საერთოდ ამოვარდებოდა ან ყოველთვის ჩაიწერებოდა.
  - `dotnet test Tests/ChessCore.Tests` გადის.

### მომხმარებლის მოთხოვნით დამატებული (2026-10-25)
- [x] 19. Editor: სცენის ავტომატური აწყობა (Tools → Setup Chess Scene) — owner: task-executor — files: Assets/Editor/ChessSceneSetup.cs, Assets/Editor/Chess.Editor.asmdef, Tests/EditorCompileCheck/EditorCompileCheck.csproj — depends: 15, 17 — done when:
  - Editor-only asmdef `Chess.Editor` (includePlatforms ["Editor"], references Chess.Core, Chess.AI, Chess.Game); namespace `Chess.Editor`.
  - მენიუ **Tools → Setup Chess Scene** (`[MenuItem("Tools/Setup Chess Scene")]`): ჯერ `EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()` (უარზე — გაუქმება), შემდეგ ხსნის `Assets/Scenes/MainScene.unity`-ს და აკეთებს:
    - `Assets/Materials/` + 6 მასალა URP/Lit-ით (task 10-ის ცხრილი): `M_Ground` 8A9A5B opaque; `M_Tile` FFFFFF α40 transparent; `M_TileLegal` 3CDC3C α160 transparent; `M_TileSelected` FFD700 α180 transparent; `M_WhitePiece` EDE6D6 opaque; `M_BlackPiece` 2B2B2B opaque. არსებული მასალა ხელახლა არ იქმნება — ფერი/პარამეტრები ახლდება. Transparent სწორად: `_Surface`=1, blend SrcAlpha/OneMinusSrcAlpha, `_ZWrite`=0, keyword `_SURFACE_TYPE_TRANSPARENT`, RenderType tag Transparent, renderQueue 3000 (Inspector-ში Surface Type = Transparent ჩანს).
    - Main Camera (tag MainCamera, ან სახელით „Main Camera“; თუ არ არის — იქმნება Camera-თი და MainCamera tag-ით): `GameObjectUtility.RemoveMonoBehavioursWithMissingScript`; `CameraRig` (თუ არ არის).
    - root `ChessGame` (თუ არ არის — იქმნება, Transform reset): კომპონენტები BoardView, PiecesView, SquarePicker, PromotionPrompt, SelectionController, StatusHud, GameController — თითო ერთხელ (GetComponent ?? AddComponent).
    - ველები `SerializedObject`-ით, API Contract-ის ზუსტი სახელებით: BoardView (groundMaterial, tileMaterial, legalTargetMaterial, selectedMaterial; groundSize არ იცვლება), PiecesView (whiteMaterial, blackMaterial), SquarePicker.raycastCamera = Main Camera, SelectionController (boardView, picker, promotionPrompt), GameController (boardView, piecesView, selection, cameraRig, hud; playAgainstAI = true, difficulty = Easy, humanSide = White, stockfishRelativePath = `Stockfish/stockfish.exe`, stockfishPathOverride = "", startFen = ""). ვერ ნაპოვნი property → `Debug.LogError` სახელით (ჩუმად არ გამოტოვდება).
    - სცენის მონიშვნა dirty + `EditorSceneManager.SaveScene`; `AssetDatabase.SaveAssets()`.
    - ბოლოს ერთი `Debug.Log("[ChessSceneSetup] ...")` სიით: Created: … / Updated: … / Removed missing scripts: N.
  - მეორედ გაშვებისას დუბლიკატები არ იქმნება (ერთი `ChessGame`, თითო კომპონენტი ერთხელ, მასალები არ დუბლირდება).
  - `Tests/EditorCompileCheck/EditorCompileCheck.csproj` (verification-only, GameCompileCheck-ის ანალოგიით, + UnityEditor DLL-ები, ProjectReference GameCompileCheck/Core/AI build პროექტებზე) → `dotnet build Tests/EditorCompileCheck` 0 შეცდომა, 0 warning. `dotnet build Tests/GameCompileCheck` და `dotnet test Tests/ChessCore.Tests` კვლავ გადის.
  - საბოლოო შემოწმება: USER task 10 (ბრძანების გაშვება + Console ჩანაწერი + Play).

## Shared files
<!-- ფაილები, რომლებიც რამდენიმე task-ის files-ში შეიძლება იყოს (shared). წესი: მხოლოდ დამატება (ახალი dependency, ტიპი, route), სხვისი ჩანაწერის შეცვლა ან წაშლა არა; ერთ საერთო ფაილზე ორი task ერთდროულად არ მუშაობს. -->
- Stage 1-ში არცერთი. `Tests/ChessCore.Tests/ChessCore.Tests.csproj`-ს და `.gitignore`-ს მხოლოდ task 1 ეხება; ახალი NuGet პაკეტი სხვა task-ს არ სჭირდება.
- შენიშვნა: `Assets/Scripts/Game/GameController.cs`-ს რიგრიგობით ავრცობს task-ები 9 → 11 → 13 → 15. ისინი `depends`-ით ჯაჭვადაა დაკავშირებული და ერთდროულად არ სრულდება. ყოველი შემდეგი task მხოლოდ ამატებს და წინა ქცევას ინარჩუნებს.

## Backlog (needs user decision)
### ფაზა 2 — Art & Sound (კონცეფციით ~1.5 თვე)
- ქართული ისტორიული 3D მოდელები primitive-ების ნაცვლად (მეფე, ლაზიერი/მთავარსარდალი, ეტლი/კოშკი, მხედარი/აბჯროსანი ცხენოსანი, ოფიცერი/ეპისკოპოსი, პაიკი/მებრძოლი).
- კამპანიის რუკის UI: ქართლი, კახეთი, სვანეთი, იმერეთი, ხევსურეთი; რეგიონის „განთავისუფლება“ და პროგრესია; შენახვა.
- ხმოვანი ბრძანებები („ლაზიერი c5-ზე, შეტევაზე!“), ჯარისკაცების პასუხები, აუდიო მენეჯერი კოორდინატების მიხედვით.
- კუთხური დიალექტები (ხევსურული, კახური, სვანური).
- გარემოები: კახეთი (ალაზნის ველი, ქვევრები, გალავანი), სვანეთი (თოვლი, კოშკები); ველის ტიპები (ქვიშა, ბალახი, თოვლი).
- ქართული UI ტექსტი (TextMeshPro + ქართული font asset), uGUI მენიუები.
### ფაზა 3 — Polish & FX (კონცეფციით ~1 თვე)
- მოკვლის კინემატოგრაფიული სცენები (Fatal Captures, 2–3 წამი).
- განათება და post-processing.
- ბალანსის ტესტირება, სირთულის Elo-ს გაზომვა.
- დინამიური AI სირთულე რეგიონის მიხედვით (~800 → ~2200+).
- სარდლის ხედში ხელები/აბჯარი.
### გადადებული (Stage 1-დან)
- Mac/Linux Stockfish და პლატფორმის ავტომატური არჩევა; build-ის/installer-ის მომზადება; Stockfish-ის build-ში გავრცელება + GPLv3 compliance.
- სვლის ანიმაცია, მოჭრილი ფიგურების ჩვენება, ბოლო სვლის და შახის (წითელი) განათება.
- Undo, PGN export/import, საათები/დროის კონტროლი, დანებება, ფრეს შეთავაზება, claim-ის რეჟიმი (50/3-ჯერადი), 75 სვლის / 5-ჯერადი წესები.
- კამერის orbit/zoom მარჯვენა ღილაკით; hot-seat-ში კამერის მობრუნება მოთამაშის მიხედვით.
- Runtime მენიუ: სირთულის და ფერის არჩევა თამაშიდან (ახლა მხოლოდ inspector-ით); Stockfish full-strength preset.
- Unity Test Framework / Play-mode ავტომატური ტესტები.
- nit (finish-check, security): `StockfishAI.GetBestMoveAsync` engine-ს უგზავნის შემოსულ `fen`-ს და არა `state.ToFen()`-ს (ნორმალიზებული); `ProcessUciTransport.SendLine` არ უარყოფს `\r`/`\n`-ს. რეალური ზიანი არ არის (Game ყოველთვის `ToFen()`-ს აგზავნის).
- nit (finish-check, security): `StockfishAI`/`ProcessUciTransport` — `Path.GetFullPath` ერთხელ, რომ `File.Exists` და გაშვება ერთსა და იმავე ფაილს ეხებოდეს (მხოლოდ inspector-ის override-ზე).
- nit (finish-check): `StockfishAI` timeout-ები ყოველ ხაზზეა და არა მთელ ლოდინზე (engine, რომელიც მუდმივად `info`-ს ბეჭდავს, timeout-ს ვერ მიიღებს; preset-ებს არ ეხება) — deadline ერთხელ.
- nit (finish-check): `StockfishAI` — `GetBestMoveAsync`-ის ct shared init-ის დროს არ მოწმდება (გაუქმება init-ის დასრულებამდე ჩერდება); Game-ს არ ეხება.
- nit (finish-check): `StockfishAITests.GetBestMove_Cancel_DiscardsStaleBestmove...` იყენებს SearchTimeout 500 ms-ს — დატვირთვისას შეიძლება flaky იყოს; 30 s.
- შესამოწმებელი (finish-check): Unity-ის Mono პატივს სცემს თუ არა `StandardInputEncoding = new UTF8Encoding(false)`-ს (BOM-ის შემთხვევაში handshake timeout) — USER task 16 აჩვენებს.
- nit (fix round 1 re-review): `GameController.StopAi` არ იძახებს `UpdateHud()`/`UpdateInteractable()`-ს — inspector-ში კომპონენტის disable/enable AI-ის ფიქრისას ტოვებს „AI thinking...“-ს და დაბლოკილ input-ს R-მდე (მხოლოდ Editor-ის debug სცენარი).
- nit (fix round 1 re-review): domain reload-ის შემდეგ AI ჩუმად ქრება (hot-seat) — HUD-ზე შენიშვნა არ ჩანს (მაგ. `Scripts reloaded. Human vs human mode.`).
- nit: README.md არ არსებობს — გაშვების ინსტრუქცია ახლა CLAUDE.md-ის Commands-სა და PROGRESS.md-ის USER task-ებშია.
- nit: `EnsureNonEmptyCompile` target-ის მოშორება `Tests/ChessCore.Build` და `Tests/ChessAI.Build` csproj-ებიდან, როცა Core-სა და AI-ში რეალური `.cs` ფაილებია (ცარიელი Compile set-ის დროს SDK `TargetFrameworkAttribute`-ს არ აგენერირებს).

## Blockers
- (მოგვარდა `0387781`-ში) dotnet build/test-ის ნებართვები `.claude/settings.json`-ში.
- USER task-ები 10, 12, 14, 16 მომხმარებლის შესასრულებელია (2 და 8 დადასტურდა). აგენტის ყველა task (1, 3–7, 9, 11, 13, 15, 17, 18, 19) დასრულებულია. Stage Done გახდება USER task 16-ის წარმატებით გავლის შემდეგ.
- (მოგვარდა `de73a20`-ში) Game სკრიპტების `.meta` ფაილები.
- **Tools → Setup Chess Scene**-ის შემდეგ დაა-commit-ე `Assets/Scenes/MainScene.unity`, `Assets/Materials/**` (+ `.meta`).

## Run State
- Stage: Stage 1 — MVP
- Finish-check this stage: done
- Fix rounds this stage: 1/2
- Failed attempts: —

## Last Session
- Date: 2026-10-25
- Done: USER task 2 და 8 მომხმარებელმა დაადასტურა (Console 0 შეცდომა; Stockfish integration ტესტი Passed, skipped 0). task 19 — Editor-ის ბრძანება Tools → Setup Chess Scene; USER task-ები 10/12/14/16 გამარტივდა (აწყობა = ერთი ბრძანება, Play-შემოწმებები უცვლელი; task 12-ს დაემატა Play Against AI-ის დროებით მოხსნა).
- Verification: `dotnet test Tests/ChessCore.Tests` 284/284; `dotnet build Tests/GameCompileCheck` და `Tests/EditorCompileCheck` 0 warn/0 err; Unity-მ დააკომპილირა Chess.Core/AI/Game/Editor 0 `error CS`-ით (Editor.log, 15:18).
- Known issues / blockers: Setup ბრძანების გაშვება და Play-mode შეუმოწმებელია (USER task 10).
- Next: USER task 10 → 12 → 14 → 16.
