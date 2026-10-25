# Project: Georgian Battle Chess (chess3D)

`PROJECT.md` — მოთხოვნები, API Contract, Decision Log. `PROGRESS.md` — task-ები და სტატუსი. კონცეფცია: `docs/CONCEPT.md`.

## Stack
- Unity 6000.6.7f1, URP 17.6.0, Input System 1.20.1 (მხოლოდ ახალი Input System)
- C# 9 / netstandard2.1: `Chess.Core` (წესები), `Chess.AI` (Stockfish UCI), `Chess.Campaign` (რეგიონები, პროგრესი, JSON შენახვა); `Chess.Game` = Unity MonoBehaviour-ები + uGUI (legacy `Text`, TMP არა)
- ტესტები: xUnit 2.x, net10.0 (`Tests/`, Assets-ის გარეთ); Stockfish (UCI, ცალკე პროცესი, binary git-ში არ ინახება)

## Commands
- dev: Unity Hub-ში გახსენი პროექტი → `Assets/Scenes/MainMenu` → Play (USER); `MainScene` პირდაპირაც (Stage 1-ის ქცევა)
- scene setup (USER, Editor): Tools → Setup Chess Scene, შემდეგ Tools → Setup Campaign Scenes (Stage 2-დან)
- lint: — (none)
- typecheck: `dotnet build Tests/ChessCore.Tests`
- test: `dotnet test Tests/ChessCore.Tests`
- typecheck (Game, Unity-ის DLL-ებით): `dotnet build Tests/GameCompileCheck` (task 9-დან)
- typecheck (Editor სკრიპტები): `dotnet build Tests/EditorCompileCheck` (task 19-დან)
- build: Unity Editor → File → Build Profiles (ხელით, USER; Stage 1–2-ში არ არის საჭირო)
- migrations: —
- integration (არასავალდებულო): `$env:STOCKFISH_PATH="<abs path to stockfish.exe>"` და შემდეგ test ბრძანება

## Conventions
- Namespace-ები: `Chess.Core`, `Chess.AI`, `Chess.Campaign`, `Chess.Game`, `Chess.Editor`. დამოკიდებულება მხოლოდ ასე: Core ← AI ← Game და Core ← Campaign ← Game; AI და Campaign ერთმანეთს არ მიმართავენ (Elo → AI = `AIDifficulty.FromElo`, Game აერთებს).
- Core/AI/Campaign-ში არ გამოიყენება `UnityEngine`, NuGet (მათ შორის `System.Text.Json`) და netstandard2.1-ს გარეთ არსებული API (მაგ. `System.Threading.Channels`, `Process.Kill(bool)`). ახალი Campaign ფაილი ავტომატურად შედის `Tests/ChessCampaign.Build`-ში.
- `record` და `init` აკრძალულია (Unity-ს არ აქვს `IsExternalInit`). გამოიყენე `readonly struct` ან კლასი get-only property-ებით.
- Core/AI/Campaign-ში ყოველი `using` ცალსახად იწერება (Unity-ში implicit usings არ არის).
- Domain reload Play-ზე გათიშულია: ყოველი static მდგომარეობა Game-ში ნულდება `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`-ით.
- `ProjectSettings/EditorBuildSettings.asset` და `Assets/Scenes/*.unity`-ს მხოლოდ Editor ბრძანებები ცვლის (USER-ის გაშვებით), აგენტი — არასდროს. UI ტექსტები მხოლოდ PROJECT.md-ის API Contract-იდან.
- `.meta` ფაილებს ხელით არასდროს ქმნი, მათ Unity ქმნის. ფაილის წაშლისას მისი `.meta`-ც იშლება.
- არ შეეხო `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `ProjectSettings/`, `.claude/`-ს, Unity-ს მიერ გენერირებულ root `*.csproj`-ს და `chess3D.slnx`-ს.
- `Assets/`-ში არ ჩაწერო build-ის შედეგები ან დროებითი ფაილები (Unity ყველაფერს იმპორტირებს).
- Input მხოლოდ `UnityEngine.InputSystem`-ით (`Keyboard.current`, `Mouse.current`). `UnityEngine.Input` runtime-ზე exception-ს აგდებს.
- Game სკრიპტები თხელია: წესები, სვლის ვალიდაცია და სტატუსი მხოლოდ `GameState`-იდან. სვლა მხოლოდ `GameState.TryMakeMove`-ით.
- Inspector ველების სახელები უცვლელია, როგორც API Contract-შია (USER-ის ინსტრუქციები მათ სახელით ასახელებს).
- შეცდომები: არასწორი სვლა → `false`; არასწორი FEN → `FormatException`; AI → `ChessAIException` / `StockfishNotFoundException`.
- Unity კოდს dotnet ვერ ამოწმებს. Game task-ის შედეგი მოწმდება USER task-ით (Console = 0 შეცდომა + Play-mode შემოწმება).
- `STOCKFISH_PATH`-ის გარეშე integration ტესტი „შეუმოწმებელია“ და report-ში ასეც იწერება.
- კოდი, comments და identifiers ინგლისურად; ახსნა და USER ინსტრუქციები ქართულად.
