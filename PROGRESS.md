# Progress

## Current Stage
Stage 3 — ქართული ინტერფეისი

## Tasks
<!-- ფორმატი: - [ ] N. Title — owner: task-executor | USER — files: ... — depends: ... — done when: ...
     task-executor = სუფთა C# (Campaign), Unity სკრიპტები (Game) და Editor სკრიპტები; USER = ფონტის ჩამოტვირთვა/TMP Essentials (44) და Play-mode შემოწმება Unity Editor-ში (45, 46; ინსტრუქცია task-ის ქვეშაა).
     API-ს, ველების სახელების, ტექსტების, განლაგების და ქცევის ერთადერთი წყარო: PROJECT.md → ## API Contract → „Stage 3 — ქართული ინტერფეისი“ (+ Decision Log 2026-10-26).
     Game/Editor task-ები (37–43) dotnet-ით მხოლოდ კომპილაციაზე და grep-ით მოწმდება; საბოლოო შემოწმება = USER task-ები 45, 46.
     Stage 1–2-ის ქცევა უცვლელია: gameplay, წესები, კამპანიის პროგრესი, schema v1, AI, სცენების ნაკადი, Inspector ველების სახელები.
     grep-ები (Grep/rg, *.cs):
       G1 legacy Input: `UnityEngine\.Input\b|\bInput\.(Get|mouse|any)` — Assets/Scripts/Game → 0.
       G2 ქართული ასოები: `[\x{10D0}-\x{10FF}]` — Assets/Scripts და Assets/Editor → ფაილებიდან მხოლოდ Assets/Scripts/Campaign/TextTable.Georgian.cs.
       G3 ძველი UI სტრიქონები: `"(Back to map|Back to menu|Retry|Play again|Cancel|Campaign|Start|Start new|Back|Quit|Continue|New Campaign|Quick Game|Campaign Map|Georgian Battle Chess|Victory!|Defeat|Draw|Game over)"|you play|to move"|AI thinking|Checkmate - |Stalemate - |Draw - |Human vs human|Click: select|Promote pawn|Color: |Difficulty: ` — Assets/Scripts/Game და Assets/Editor → 0.
       G4 legacy Text / IMGUI: `UnityEngine\.UI\.Text\b|[<( ]Text[ >);,]|LegacyRuntime|OnGUI|\bGUI\.|GUIStyle|GUILayout` — Assets/Scripts/Game და Assets/Editor → 0.
     Stage 2-ის task-ები (20–34) — ## Stage 2 — Done, Stage 1-ის (1–19) — ## Stage 1 — Done (ფაილის ბოლოს). -->

- [x] 35. Campaign: სტრიქონების ცხრილი (`TextId`, `TextTable` ქართული + ინგლისური) და `GameText` — owner: task-executor — files: Assets/Scripts/Campaign/TextId.cs, Assets/Scripts/Campaign/TextTable.cs, Assets/Scripts/Campaign/TextTable.Georgian.cs, Assets/Scripts/Campaign/TextTable.English.cs, Assets/Scripts/Campaign/GameText.cs, Tests/ChessCore.Tests/Campaign/TextTableTests.cs, Tests/ChessCore.Tests/Campaign/GameTextTests.cs — depends: — — done when:
  - `dotnet build Tests/ChessCore.Tests` და `dotnet test Tests/ChessCore.Tests` გადის (ყველა ძველი ტესტი უცვლელად + ახლები).
  - ტიპები და ხელმოწერები ზუსტად API Contract → Stage 3 → „სტრიქონების ცხრილი“: `TextId` (73 მნიშვნელობა, ცხრილის რიგით) → TextId.cs; `TextTable` (`sealed partial`) → TextTable.cs (API, `AtlasCharacters`, `IsSupportedChar`, ვალიდაცია) + TextTable.Georgian.cs / TextTable.English.cs (მხოლოდ მონაცემები); `GameText` → GameText.cs. ყოველი ტექსტი ენიჭება ცხადი `TextId`-ით (არა პოზიციური მასივით); აკლია/ცარიელია → `InvalidOperationException` type init-ზე. `Format` = `string.Format(CultureInfo.InvariantCulture, ...)`. ფაილები UTF-8.
  - `TextTableTests`: `Georgian_IsActive` (`Assert.Same(TextTable.Georgian, TextTable.Active)`, LanguageCode `ka`/`en`); `EveryId_HasText_InBothTables` (`Enum.GetValues(typeof(TextId))`, 73); `Georgian_ExactTexts` და `English_ExactTexts` (Theory — contract-ის ცხრილის ყველა 73 სტრიქონი ზუსტად, `\n` და ორმაგი space-ების ჩათვლით); `Placeholders_MatchBetweenTables` (`{n}` ინდექსების სიმრავლე ერთნაირია; `Format` შესაბამისი რაოდენობის არგუმენტით არ ვარდება); `AllTexts_UseSupportedCharsOnly` (ორივე ცხრილი); `IsSupportedChar_Rules` (true: `' '`, `'a'`, `'~'`, `'\n'`, U+10D0, U+10F0, U+10FF; false: `'\t'`, `'\r'`, U+00A0, U+2013, U+2014, U+2019, U+00AB, U+10A0, U+10CF, U+1C90, U+2D00); `AtlasCharacters_AsciiThenMkhedruli` (სიგრძე 143, უნიკალური, [0] = `' '`, [94] = `'~'`, [95] = U+10D0, [142] = U+10FF, `'\n'` არ შედის); `Get_UndefinedId_Throws` (`(TextId)9999` → ArgumentOutOfRangeException); `Format_NullArgs_Throws`.
  - `GameTextTests`: `Status` Theory — (InProgress, White, false, false) → `თეთრის სვლაა`; (InProgress, Black, true, false) → `შავის სვლაა  შახი!`; (InProgress, White, false, true) და (InProgress, White, true, true) → `AI ფიქრობს...`; (Checkmate, Black, true, false) → `მატი - თეთრმა მოიგო`; (Checkmate, White, true, true) → `მატი - შავმა მოიგო`; Stalemate → `პატი - ფრე`; FiftyMoveRule → `ფრე - 50 სვლის წესი`; ThreefoldRepetition → `ფრე - სამჯერადი გამეორება`; InsufficientMaterial (aiThinking true-თიც) → `ფრე - მასალის ნაკლებობა`; `(GameStatus)99` → ArgumentOutOfRangeException. `SideName` (ორივე + `(Side)5` throw); `PieceName` (4 ფიგურა; King, Pawn, None → throw); `PromotionTitle(White)`, `QuickColorLabel`, `QuickDifficultyLabel`, `QuickGameInfo`, `StockfishNotFound`, `AiError` (`"Engine crashed."` → წერტილი ერთხელ), `WithDebugFen`, `HelpLine` — ზუსტად contract-ის „ზუსტი შედეგებით“; null არგუმენტები → ArgumentNullException; `AllOutputs_UseSupportedCharsOnly` (ყველა ზემოთ ჩამოთვლილი შედეგი).
  - Grep `Assets/Scripts/Campaign`-ში: `UnityEngine|\brecord\b|\binit;|System\.Text\.Json` → 0. `BuildTargetTests.CampaignReferencesOnlyCore` კვლავ გადის. `.meta` ხელით არ შექმნილა.

- [x] 36. Campaign: რეგიონები, `CampaignText`, `BattleRules`, `RecordBattle` — ტექსტები ცხრილიდან — owner: task-executor — files: Assets/Scripts/Campaign/CampaignRegions.cs, Assets/Scripts/Campaign/CampaignText.cs, Assets/Scripts/Campaign/BattleRules.cs, Assets/Scripts/Campaign/CampaignProgress.cs, Tests/ChessCore.Tests/Campaign/CampaignRegionsTests.cs, Tests/ChessCore.Tests/Campaign/CampaignTextTests.cs, Tests/ChessCore.Tests/Campaign/BattleRulesTests.cs, Tests/ChessCore.Tests/Campaign/CampaignProgressTests.cs — depends: 35 — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის. ხელმოწერები და ტექსტები ზუსტად API Contract → Stage 3 → „Chess.Campaign — Stage 3 ცვლილებები“ და „ზუსტი შედეგები“: `CampaignText.SaveFailedWarning` / `CorruptedSaveWarning` / `NotCountedMessage` და `BattleRules.NotCountedTitle` — `const` → static get-only property (`TextTable.Active`-იდან); `CampaignRegions` Name/Era ცხრილიდან (id, Elo, რიგი უცვლელი); `RecordBattle`-ის ლოგიკა და ფლაგები უცვლელია, მხოლოდ ტექსტის აწყობა იცვლება. exception-ების შეტყობინებები ინგლისურად რჩება.
  - ძველი ტესტების ყველა შემთხვევა რჩება, ინგლისური მოლოდინები შეიცვალა contract-ის ქართული ტექსტებით (შემოწმება არ წაშლილა): `CampaignRegionsTests.All_MatchContractTable` (ქართული Name/Era), `CampaignTextTests` (StateName სამივე, RegionLabel, MapStatus 0/2/სრული, BattleInfo, სამი property), `BattleRulesTests` (GetTitle სამივე, `NotCountedTitle == "თამაში დასრულდა"`), `CampaignProgressTests` (ყველა RecordBattle-ის Title/Message).
  - ახალი: `CampaignTextTests.AllFormattedOutputs_UseSupportedCharsOnly` — RegionLabel (5 რეგიონი × 3 მდგომარეობა), BattleInfo (5), MapStatus (0..5 გათავისუფლებული, კამპანიის რიგით), სამი property, RecordBattle-ის ყოველი შეტყობინება (ყოველი რეგიონი × Victory/Defeat/Draw × Available/Liberated, + counted false) → ყველა სიმბოლო `TextTable.IsSupportedChar`.
  - `CampaignSaveStore.cs`, `CampaignSaveSerializer.cs`, `JsonReader.cs`, `Region.cs` და მათი ტესტები უცვლელია და გადის (schema v1, id-ები).
  - G2 → მხოლოდ `TextTable.Georgian.cs`. Grep Campaign: `UnityEngine|\brecord\b|\binit;` → 0.

- [x] 37. TMP-ის მიერთება: asmdef-ები და compile-check პროექტები — owner: task-executor — files: Assets/Scripts/Game/Chess.Game.asmdef, Assets/Editor/Chess.Editor.asmdef, Tests/GameCompileCheck/GameCompileCheck.csproj, Tests/EditorCompileCheck/EditorCompileCheck.csproj — depends: — — done when:
  - `Chess.Game.asmdef` references = `["Chess.Core","Chess.AI","Chess.Campaign","Unity.InputSystem","UnityEngine.UI","Unity.TextMeshPro"]`; `Chess.Editor.asmdef` references = `["Chess.Core","Chess.AI","Chess.Campaign","Chess.Game","Unity.InputSystem","UnityEngine.UI","Unity.TextMeshPro"]`; სხვა ველები უცვლელია.
  - ორივე csproj: + `<Reference Include="../../Library/ScriptAssemblies/Unity.TextMeshPro.dll" Private="false" />`; სხვა უცვლელია (`Unity.TextMeshPro.Editor.dll` არ ემატება — საჭირო არ არის).
  - `dotnet build Tests/GameCompileCheck` და `dotnet build Tests/EditorCompileCheck` → 0 შეცდომა, 0 warning; `dotnet test Tests/ChessCore.Tests` გადის. `.cs` ფაილები არ შეცვლილა.

- [x] 38. Game UI: MainMenu, RegionButton, CampaignMap, BattlePanel → `TMP_Text` + ტექსტები ცხრილიდან; `DifficultyText` — owner: task-executor — files: Assets/Scripts/Game/UI/MainMenuController.cs, Assets/Scripts/Game/UI/RegionButton.cs, Assets/Scripts/Game/UI/CampaignMapController.cs, Assets/Scripts/Game/UI/BattlePanel.cs, Assets/Scripts/Game/UI/DifficultyText.cs — depends: 36, 37 — done when:
  - ზუსტად API Contract → Stage 3 → „Chess.Game — Stage 3“: ველების ტიპები `Text` → `TMP_Text` (სახელები უცვლელი, `[SerializeField] private`); `DifficultyText.Name`; MainMenu-ის `colorLabel`/`difficultyLabel` → `GameText.QuickColorLabel` / `QuickDifficultyLabel(DifficultyText.Name(...))`. ნაკადი, listener-ები, LogError-ები და ფერები უცვლელია.
  - `dotnet build Tests/GameCompileCheck` → 0 შეცდომა, 0 warning; `dotnet build Tests/EditorCompileCheck` → 0/0 (Editor ჯერ ძველ `Text`-ს აბამს — კომპილირდება; runtime-ზე ამას task 43 ასწორებს); `dotnet test Tests/ChessCore.Tests` გადის.
  - ამ 5 ფაილში: G1, G2, G3, G4 → 0; UI სტრიქონის literal არ არის (მხოლოდ log-ები).
  - საბოლოო შემოწმება: USER task 45.

- [x] 39. Game: `StatusHud` და `PromotionPrompt` — OnGUI → uGUI/TMP — owner: task-executor — files: Assets/Scripts/Game/StatusHud.cs, Assets/Scripts/Game/PromotionPrompt.cs — depends: 35, 37 — done when:
  - ზუსტად API Contract → Stage 3 → Chess.Game ცხრილის `StatusHud` და `PromotionPrompt` სტრიქონები: ახალი Inspector ველები (`statusText`, `errorText`; `panel`, `titleText`, `queenButton`, `rookButton`, `bishopButton`, `knightButton`, `cancelButton`), public API უცვლელი (`Show`, `IsOpen`, `LastClosedFrame`, `Open`), Esc = Cancel, `IsOpen = false` callback-მდე, `panel == null`-ზე `Open` → LogError + `onCancelled` (ფიგურა ჩუმად არ ირჩევა), `Close`-ში `EventSystem.current?.SetSelectedGameObject(null)`, listener-ები `Awake`/`OnDestroy`-ში, null ველზე NRE არა.
  - `SelectionController.cs` არ შეცვლილა (press/release-ის წესი — PROJECT.md Decision Log „HUD და გარდაქმნა uGUI-ზე“).
  - `dotnet build Tests/GameCompileCheck` 0/0; `dotnet build Tests/EditorCompileCheck` 0/0; `dotnet test` გადის. `OnGUI|\bGUI\.|GUIStyle|GUILayout` მთელ `Assets/Scripts/Game`-ში → 0; ამ ორ ფაილში G1, G2, G3, G4 → 0.
  - საბოლოო შემოწმება: USER task 46 (ნაბიჯები 5–6).

- [x] 40. Game: GameController-ის ტექსტები ცხრილიდან + სარდლის ხედის კამერა მაღლა/უკან — owner: task-executor — files: Assets/Scripts/Game/GameController.cs, Assets/Scripts/Game/CameraRig.cs — depends: 36, 38 — done when:
  - ზუსტად API Contract → Stage 3 → „GameController — Stage 3“ და `CameraRig` სტრიქონი: `CameraRig.cs`-ში იცვლება მხოლოდ სამი ნაგულისხმევი (`commanderEyeHeight` 3.0, `commanderBackOffset` 1.6, `commanderPitch` 30); მეფე არ იმალება, `PiecesView.cs` არ იცვლება, GameController-ში მეფის ხილვადობის ლოგიკა არ არის. `BuildStatus` → `GameText.Status`; `[DEBUG FEN]` → `GameText.WithDebugFen`; შეცდომები → `GameText.StockfishNotFound` / `AiError`; info/ღილაკების ტექსტები → `CampaignText`/`GameText`/`TextTable.Active`/`DifficultyText`.
  - Stage 1–2-ის ყველა Inspector ველი (სახელი, ტიპი, ნაგულისხმევი), ნაკადი, შედეგის დამუშავება, `Debug.Log*` ტექსტები (მათ შორის `[GameController] AI difficulty: ...` ფორმატი) და `CameraRig`-ის ველები/ნაგულისხმევები უცვლელია.
  - `dotnet build Tests/GameCompileCheck` 0/0 **და** `dotnet build Tests/GameCompileCheck -p:DefineConstants=UNITY_EDITOR` 0 შეცდომა (ბოლოს ისევ ჩვეულებრივი build); `dotnet build Tests/EditorCompileCheck` 0/0; `dotnet test Tests/ChessCore.Tests` გადის.
  - მთელ `Assets/Scripts/Game`-ში: G1, G3, G4 → 0; G2 → 0 (Game-ში ქართული ასო არ არის).
  - საბოლოო შემოწმება: USER task 46.

- [x] 41. Editor: `EditorUi` (საერთო TMP builder-ები) და **Tools → Setup Georgian Font** — owner: task-executor — files: Assets/Editor/EditorUi.cs, Assets/Editor/GeorgianFontSetup.cs — depends: 35, 37 — done when:
  - ზუსტად API Contract → Stage 3 → „Editor — Stage 3“: `GeorgianFontSetup` (const-ები, MenuItem, `Ensure(out bool charactersMissing)`, ნაბიჯები 1–6, log-ების ზუსტი ტექსტები, Dynamic → `TryAddCharacters(TextTable.AtlasCharacters)` → Static, atlas და material `AddObjectToAsset`-ით, fallback `LiberationSans SDF`, არსებული asset არ იცვლება) და `EditorUi` (Placement-ები, ფერები, `NewCanvas` Expand-ით, `MakeText`/`MakeButton` TMP-ით და auto-size-ით, `EnsureEventSystem`, `DeleteRoots`; `enableWordWrapping` არ გამოიყენება).
  - `ChessSceneSetup.cs` და `CampaignScenesSetup.cs` ამ task-ში არ იცვლება (ისინი ჯერ ძველ helper-ებს იყენებს; task 42/43).
  - `dotnet build Tests/EditorCompileCheck` → 0 შეცდომა, 0 warning; `dotnet build Tests/GameCompileCheck` 0/0; `dotnet test` გადის. ამ 2 ფაილში G2, G3, G4 → 0 (log-ები ინგლისურად).
  - საბოლოო შემოწმება: USER task 45 (ნაბიჯები 2–4).

- [x] 42. Editor: Setup Chess Scene → `HudCanvas` (HUD + გარდაქმნის ფანჯარა), EventSystem და სარდლის კამერის მნიშვნელობები — owner: task-executor — files: Assets/Editor/ChessSceneSetup.cs — depends: 39, 40, 41 — done when:
  - ზუსტად API Contract → Stage 3 → „ChessSceneSetup — Stage 3 დამატებები“ (ნაბიჯები 1–6) და „UI სტილი (Stage 3)“ → `HudCanvas`: font-ის შემოწმება სცენის გახსნამდე (null → LogError, არაფერი იცვლება), `EditorUi.EnsureEventSystem`, `HudCanvas` წაშლა+შექმნა, `HudPanel` (VerticalLayoutGroup; `StatusText`, `ErrorText`, `HelpText` = `GameText.HelpLine`), `PromotionPanel` (inactive; `TitleText` + 5 ღილაკი, label-ები `GameText.PieceName` / `Cancel`), `StatusHud`/`PromotionPrompt` ველების მიბმა `SerializedObject`-ით (ვერ ნაპოვნი → LogError + WITH ERRORS).
  - ნაბიჯი 6: `Main Camera`-ს `CameraRig`-ზე `SerializedObject`-ით `commanderEyeHeight` = 3.0, `commanderBackOffset` = 1.6, `commanderPitch` = 30 (ვერ ნაპოვნი → LogError + WITH ERRORS); `CameraRig`-ის სხვა ველებს არ ეხება.
  - Stage 1-ის ნაბიჯები, მასალები, `GameController`-ის ველები და ნაგულისხმევები უცვლელია; `battlePanel`-ს და `BattleCanvas`-ს არ ეხება.
  - `dotnet build Tests/EditorCompileCheck` 0/0; `dotnet build Tests/GameCompileCheck` 0/0. ამ ფაილში G2, G3, G4 → 0.
  - `git status --porcelain` — მხოლოდ ეს ფაილი (სცენებს USER-ის მიერ გაშვებული ბრძანება ცვლის).
  - საბოლოო შემოწმება: USER task 45 (ნაბიჯები 5, 7), 46.

- [ ] 43. Editor: Setup Campaign Scenes → TMP, ტექსტები ცხრილიდან, Expand, MainPanel layout, ბრძოლის ახალი ზომები — owner: task-executor — files: Assets/Editor/CampaignScenesSetup.cs — depends: 36, 38, 41 — done when:
  - ზუსტად API Contract → Stage 3 → „CampaignScenesSetup — Stage 3 ცვლილებები“ (1–5) და „UI სტილი (Stage 3)“: font-ის შემოწმება ნაბიჯ 1-ის შემდეგ; ყველა ტექსტი TMP (`EditorUi`), `LegacyRuntime.ttf` და `UnityEngine.UI.Text` აღარ არის; ლოკალური `Placement`/`Center`/.../`MakeText`/`MakeButton`/`NewCanvas`/`EnsureEventSystem` წაშლილია და `EditorUi`-ით ჩანაცვლებულია; ყველა Canvas Expand; `MainPanel` + `VerticalLayoutGroup`; რეგიონის label Bold auto 18–28, Wrap, 8px inset; `BattleCanvas` ახალი ზომებით; სტატიკური ტექსტები ცხრილიდან (mapping — ნაბიჯი 5).
  - სცენების სახელები, ობიექტების სახელები, ველების მიბმა, ConfirmPanel/QuickGamePanel/ResultPanel inactive, EventSystem, Build Settings და შემაჯამებელი log უცვლელია; `HudCanvas`/`StatusHud`/`PromotionPrompt`-ს არ ეხება.
  - report-ში გამოთვლა (reference 1920×1080, x/y პიქსელები): CampaignMap — `TitleText`, `StatusText`, `BackButton` არ კვეთს `MapArea`-ს; MainScene — `InfoText`, `LeaveButton`, `HudPanel`, `ResultPanel` ერთმანეთს არ კვეთს, `PromotionPanel` არ კვეთს `HudPanel`-ს; და ახსნა, რატომ ინარჩუნებს Expand ამას სხვა aspect-ზე.
  - `dotnet build Tests/EditorCompileCheck` 0/0; `dotnet build Tests/GameCompileCheck` 0/0; `dotnet test` გადის. მთელ `Assets/Editor`-ში: G2, G3, G4 → 0.
  - `git status --porcelain` — მხოლოდ ეს ფაილი.
  - საბოლოო შემოწმება: USER task 45, 46.

- [x] 44. (მომხმარებელმა შეასრულა და დააკომიტა `6d97068`-ში, 2026-10-26) USER: Noto Sans Georgian ფონტის ჩამოტვირთვა და TMP Essential Resources-ის იმპორტი — owner: USER — files: Assets/Fonts/NotoSansGeorgian-Regular.ttf, Assets/Fonts/OFL.txt, Assets/TextMesh Pro/** (+ `.meta`-ები, Unity ქმნის) — depends: — — done when: ფონტი და ლიცენზია `Assets/Fonts`-შია, `Assets/TextMesh Pro` არსებობს, Console = 0 წითელი შეცდომა
    **რატომ:** ქართული ასოების საჩვენებლად თამაშს სჭირდება ფონტის ფაილი (ის კოდში არ არის) და TextMeshPro-ს ძირითადი ფაილები (shader-ები, პარამეტრები). ეს task ნებისმიერ დროს შეგიძლია გააკეთო — სასურველია ახლავე.
    1. ბრაუზერში გახსენი **https://fonts.google.com/noto/specimen/Noto+Sans+Georgian** (Google Fonts — ოფიციალური საიტი; სხვა საიტიდან არ ჩამოტვირთო).
    2. ზედა-მარჯვნივ დააჭირე **Get font**, შემდეგ **Download all** (ზოგ ვერსიაში ღილაკს ჰქვია **Download family**). ჩამოიტვირთება `.zip` ფაილი (მაგ. `Noto_Sans_Georgian.zip`).
    3. `.zip`-ზე მარჯვენა click → **Extract All** → **Extract**.
    4. გახსნილ საქაღალდეში არის ფაილი `OFL.txt` (ლიცენზია) და საქაღალდე `static`. გახსენი `static` და იპოვე ზუსტად **`NotoSansGeorgian-Regular.ttf`** (არა `Bold`, `Condensed`, `Light` და არა ზედა საქაღალდის `...VariableFont...ttf`).
    5. Windows Explorer-ში გახსენი `D:\yvelaperi\saitebi\chess\chess3D\chess3D\Assets\` → ცარიელ ადგილას მარჯვენა click → **New → Folder** → სახელი ზუსტად **`Fonts`**.
    6. დააკოპირე `NotoSansGeorgian-Regular.ttf` და `OFL.txt` საქაღალდეში `Assets\Fonts\`. ფაილებს სახელი არ გადაარქვა.
    7. გადადი Unity-ზე და დაელოდე იმპორტს (ქვედა-მარჯვენა პროგრესის ზოლი). **Project** ფანჯარაში `Assets/Fonts`-ში ჩანს `NotoSansGeorgian-Regular` (ხატულა „Aa“) და `OFL`.
    8. ზედა მენიუ **Window → TextMeshPro → Import TMP Essential Resources**. იხსნება ფანჯარა **Import Unity Package** ფაილების სიით → ქვედა-მარჯვნივ **Import**. დაელოდე. Project-ში გაჩნდება საქაღალდე `Assets/TextMesh Pro` (შიგნით `Fonts`, `Resources`, `Shaders`, `Sprites`). თუ ამის ნაცვლად თავისით გაიხსნა ფანჯარა **TMP Importer** — დააჭირე მასში **Import TMP Essentials** და შემდეგ დახურე. **Import TMP Examples & Extras** არ არის საჭირო.
    9. Console (**Window → General → Console**) → **Clear** → წითელი შეცდომების მთვლელი = **0**.
    10. უთხარი გუნდს, რომ დაკომიტდეს: `Assets/Fonts.meta`, `Assets/Fonts/NotoSansGeorgian-Regular.ttf` (+ `.meta`), `Assets/Fonts/OFL.txt` (+ `.meta`), `Assets/TextMesh Pro.meta` და მთელი `Assets/TextMesh Pro/` საქაღალდე `.meta`-ებით. მათ გარეშე სხვა კომპიუტერზე ქართული ტექსტი არ გამოჩნდება. (ლიცენზია OFL ფონტის თამაშთან ერთად გავრცელების ნებას იძლევა, თუ `OFL.txt` თან ახლავს.)

- [ ] 45. USER: ფონტის asset, სცენების განახლება, მენიუ და რუკა ქართულად — owner: USER — files: Assets/Fonts/NotoSansGeorgian SDF.asset (ქმნის Tools → Setup Georgian Font), Assets/Scenes/MainMenu.unity, Assets/Scenes/CampaignMap.unity, Assets/Scenes/MainScene.unity, ProjectSettings/EditorBuildSettings.asset (ცვლის Tools → Setup Chess Scene / Setup Campaign Scenes) — depends: 40, 42, 43, 44 — done when: ყველა შემოწმება გადის; Console = 0 წითელი შეცდომა და 0 გაფრთხილება `was not found in the`
    1. გადადი Unity-ზე, დაელოდე კომპილაციას (ქვედა-მარჯვენა პროგრესის ზოლი ქრება). Console → **Clear** → 0 წითელი. **Play-ს ჯერ ნუ დააჭერ** — ჯერ სცენები უნდა განახლდეს (ნაბიჯები 2–7).
    2. ზედა მენიუ **Tools → Setup Georgian Font**. Console-ში: `[GeorgianFont] Created Assets/Fonts/NotoSansGeorgian SDF.asset: ...` და `[GeorgianFont] All UI text characters are covered.` შეიძლება იყოს ხაზიც `UI characters taken from fallback LiberationSans SDF: ...` — ესეც ნორმალურია (ლათინური ასოები სხვა ფონტიდან მოდის). წითელი ხაზი არ უნდა იყოს. თუ წითელია `Font file not found` → გაიმეორე task 44, ნაბიჯები 1–7; თუ `TMP Essential Resources are not imported` → task 44, ნაბიჯი 8.
    3. Project → `Assets/Fonts` → დააჭირე `NotoSansGeorgian SDF`-ს. Inspector-ში **Atlas Population Mode** = `Static`. Project-ში ამ ფაილის მარცხნივ პატარა ისარს (▸) დააჭირე: შიგნით ჩანს `NotoSansGeorgian SDF Atlas` და `NotoSansGeorgian SDF Material` (ე.ი. ასოების სურათი ფაილშია შენახული).
    4. ისევ **Tools → Setup Georgian Font** → ახლა `[GeorgianFont] Using existing Assets/Fonts/NotoSansGeorgian SDF.asset.` (ახალი ფაილი არ იქმნება).
    5. **Tools → Setup Chess Scene** (თუ შენახვას გკითხავს — **Save**) → Console: `[ChessSceneSetup] Done.` (`WITH ERRORS`-ის გარეშე); `Created:`-ში ჩანს `HudCanvas`.
    6. **Tools → Setup Campaign Scenes** → `[CampaignScenesSetup] Done.` (`WITH ERRORS`-ის გარეშე) და ბოლო ხაზი `Build scenes: MainMenu, CampaignMap, MainScene`.
    7. გაუშვი ორივე მეორედ: ჯერ **Tools → Setup Chess Scene**, შემდეგ **Tools → Setup Campaign Scenes**. Project → `Assets/Scenes` → ორჯერ დააჭირე `MainScene`-ს: Hierarchy-ში ზუსტად ერთი `HudCanvas`, ერთი `BattleCanvas`, ერთი `EventSystem`, ერთი `ChessGame`. დააჭირე `ChessGame`-ს → Inspector: **Status Hud**-ში **Status Text** და **Error Text** შევსებულია; **Promotion Prompt**-ში **Panel**, **Title Text** და 5 ღილაკი შევსებულია (არცერთი `None`). შემდეგ ორჯერ დააჭირე `Assets/Scenes/MainMenu`-ს.
    8. **`გაგრძელება`-ს დამალვის შესამოწმებლად შენახვა დროებით გადამალე:** Windows Explorer-ის მისამართის ზოლში ჩასვი `%USERPROFILE%\AppData\LocalLow\DefaultCompany\chess3D` → Enter. თუ იქ არის `campaign.json` → მარჯვენა click → **Rename** → `campaign.backup.json`. (ფაილი არ იშლება — ნაბიჯ 11-ში დავაბრუნებთ.)
    9. დააჭირე **Play**: მუქი ფონი, სათაური `ქართული საბრძოლო ჭადრაკი`, ღილაკები ზემოდან ქვემოთ `ახალი კამპანია`, `სწრაფი თამაში`, `გასვლა` — თანაბარი დაშორებით, **ცარიელი ადგილის გარეშე** (`გაგრძელება` არ ჩანს). ყველა ასო ქართულად კარგად იკითხება, კვადრატები (□) არსად ჩანს.
    10. `სწრაფი თამაში` → პანელი: სათაური `სწრაფი თამაში`, ღილაკები `ფერი: თეთრი`, `სირთულე: საშუალო`, `დაწყება`, `უკან`. დააჭირე `ფერი`-ს → `ფერი: შავი`. `სირთულე`-ს სამჯერ → `რთული` → `ოსტატი` → `მარტივი`. ტექსტი ღილაკს არ სცდება. `უკან` → მენიუ. **არ დააჭირო** `ახალი კამპანია`-ს. გააჩერე Play.
    11. Explorer-ში `campaign.backup.json` → **Rename** → ისევ `campaign.json`.
    12. **Play**: ახლა ჩანს `გაგრძელება` (`ახალი კამპანია`-ს ქვეშ). დააჭირე `ახალი კამპანია` → კითხვა `დავიწყოთ ახალი კამპანია? მიმდინარე პროგრესი დაიკარგება.` და ღილაკები `ახლის დაწყება` / `გაუქმება` → დააჭირე `გაუქმება` → მთავარი მენიუ.
    13. `გაგრძელება` → რუკა: სათაური `კამპანიის რუკა` მწვანე არის ზემოთ; 5 ღილაკი, თითოში 4 ხაზი: სახელი (`ქართლი`, `კახეთი`, `იმერეთი`, `სვანეთი`, `ხევსურეთი`), ეპოქა (მაგ. `V საუკუნე - მეფე ვახტანგ გორგასალი`), `Elo ...` და მდგომარეობა (`გათავისუფლებულია` / `ხელმისაწვდომია` / `ჩაკეტილია`); ბოლო ხაზი არ არის მოჭრილი. ქვემოთ: `გათავისუფლებულია N რეგიონი 5-დან. აირჩიე რეგიონი ბრძოლისთვის.`; ქვედა-მარცხნივ `მენიუში დაბრუნება`.
    14. **სხვადასხვა ეკრანის ფორმა:** Game ფანჯრის ზედა ზოლში არის ჩამოსაშლელი სია (ჩვეულებრივ წერია `Free Aspect`). აირჩიე `16:9 Aspect`, შემდეგ `4:3 Aspect`, შემდეგ ისევ `Free Aspect` და Game ფანჯრის კიდე მაუსით გაწიე, რომ ძალიან ფართო და დაბალი გახდეს. ყველა შემთხვევაში: სათაური რუკის (მწვანე არის) ზემოთაა, `მენიუში დაბრუნება` და სტატუსის ტექსტი — მის **ქვემოთ** და მწვანე არეს არ ფარავს.
    15. `მენიუში დაბრუნება` → მენიუ. გააჩერე Play. Console: 0 წითელი და არცერთი ყვითელი ხაზი სიტყვებით `was not found in the` (ასეთი ხაზი ნიშნავს ასოს, რომელიც ფონტში არ არის).
    16. უთხარი გუნდს, რომ დაკომიტდეს: `Assets/Fonts/NotoSansGeorgian SDF.asset` (+ `.meta`), `Assets/Scenes/*.unity`, `ProjectSettings/EditorBuildSettings.asset` (თუ შეიცვალა) და Unity-ის მიერ შექმნილი ახალი `.meta` ფაილები: `Assets/Scripts/Campaign/TextId.cs.meta`, `TextTable.cs.meta`, `TextTable.Georgian.cs.meta`, `TextTable.English.cs.meta`, `GameText.cs.meta`, `Assets/Scripts/Game/UI/DifficultyText.cs.meta`, `Assets/Editor/EditorUi.cs.meta`, `Assets/Editor/GeorgianFontSetup.cs.meta`. თუ რამე არ ემთხვევა — screenshot და Console-ის ტექსტი გადაეცი გუნდს.

- [ ] 46. USER: ბრძოლა ქართულად — HUD, გარდაქმნის ფანჯარა, სარდლის ხედი, კამპანიის შედეგები — owner: USER — files: Assets/Scenes/MainScene.unity (Start Fen / Debug Campaign Start Fen / Stockfish Path Override დროებით; ბოლოს ცარიელი) — depends: 45 — done when: ყველა შემოწმება გადის; Console = 0 წითელი შეცდომა და 0 `was not found in the`; ბოლოს სამივე ველი ცარიელია; `stockfish.exe` არ რჩება
    **შეხსენება:** Inspector-ის ველებს ყოველთვის Play-ის **გაჩერებულ** მდგომარეობაში ცვლი და შემდეგ **Ctrl+S** (სცენის შენახვა).
    1. Console → **Clear**. ღიაა MainMenu → **Play** → `სწრაფი თამაში` → `ფერი: შავი`, `სირთულე: მარტივი` → `დაწყება`.
    2. ზემოთ ცენტრში `სწრაფი თამაში - მარტივი - შენი ფერი: შავი`; ზედა-მარჯვნივ `მენიუში დაბრუნება`; ამ ზოლის **ქვემოთ**, მარცხნივ: სტატუსი (`AI ფიქრობს...` ან `შავის სვლაა`) და დახმარება `მაუსი: არჩევა/სვლა  TAB: ხედი  R: თავიდან`. სტატუსი ზედა ტექსტს არ ფარავს.
    3. **სარდლის ხედი:** კამერა შენი (შავი) მეფის უკან და მაღლაა. შენი მეფე **ჩანს** ეკრანის ქვედა ნაწილში და დაფას არ ფარავს — დაფა ბოლომდე (მოწინააღმდეგის მხარემდე) ჩანს. დააჭირე **TAB** → ტაქტიკური ხედი; ისევ **TAB** → სარდლის ხედი. სარდლის ხედში: დააჭირე პაიკს e7 → e5 (სვლა მუშაობს). შემდეგ, თეთრის პასუხის მერე, **დააჭირე შენს მეფეს** — ის მოინიშნება და დასაშვები უჯრა e7 ანათებს (მეფის არჩევა სარდლის ხედშიც მუშაობს). მეფის სვლა არ გააკეთო — დააჭირე ცარიელ ადგილს, მონიშვნა მოიხსნება. თუ ხედი უხერხულია (მეფე ზედმეტად დიდი/პატარაა ან დაფა ცუდად ჩანს) — screenshot გადაეცი გუნდს (მნიშვნელობები: Main Camera → **Camera Rig** → Commander Eye Height / Back Offset / Pitch).
    4. Game view-ში აირჩიე `4:3 Aspect`, შემდეგ `Free Aspect` და ფანჯარა გახადე ძალიან ფართო: ზედა ზოლი (ტექსტი + ღილაკი) და მარცხენა სტატუსი არცერთ ზომაზე ერთმანეთს არ ფარავს. `მენიუში დაბრუნება` → მენიუ. გააჩერე Play.
    5. **MainScene ცალკე + გარდაქმნა:** ორჯერ დააჭირე `Assets/Scenes/MainScene`-ს. Hierarchy → `ChessGame` → **Game Controller** → **Start Fen** = `8/P6k/8/8/8/8/8/K7 w - - 0 1` → **Ctrl+S** → **Play**. ზედა ზოლი (ტექსტი/ღილაკი) არ ჩანს; მარცხნივ ჩანს `თეთრის სვლაა` და დახმარების ხაზი.
    6. დააჭირე **TAB** (ტაქტიკური ხედი) → თეთრი პაიკი a7-ზე → a8: ეკრანის ცენტრში ფანჯარა `პაიკის გარდაქმნა (თეთრი)` და ღილაკები `ლაზიერი`, `ეტლი`, `კუ`, `მხედარი`, `გაუქმება`. დააჭირე `გაუქმება` → ფანჯარა ქრება, პაიკი a7-ზე რჩება. ისევ a7 → a8 → **Esc** → ფანჯარა ქრება. ისევ a7 → a8 → `მხედარი` → a8-ზე მხედარია, ფანჯარა ქრება და დაფაზე სხვა არაფერი შეცვლილა (ღილაკზე დაჭერამ დაფაზე არც სვლა, არც მონიშვნა არ გამოიწვია). შავი (AI) პასუხობს. გააჩერე Play.
    7. **პატი:** **Start Fen** = `7k/8/8/5Q2/8/8/8/K7 w - - 0 1` → **Ctrl+S** → Play → f5 → f7 → სტატუსი `პატი - ფრე`. გააჩერე Play.
    8. **Stockfish-ის გარეშე:** **Start Fen** = `6k1/5ppp/8/8/8/8/5PPP/R5K1 w - - 0 1`, **Stockfish Path Override** = `C:\nope\stockfish.exe` → **Ctrl+S** → Play → მარცხნივ, სტატუსის ქვეშ ყვითლად `Stockfish ვერ მოიძებნა: C:\nope\stockfish.exe. ორი მოთამაშის რეჟიმი.`, მის ქვეშ დახმარების ხაზი (ტექსტები ერთმანეთს არ ედება). a1 → a8 → `მატი - თეთრმა მოიგო`. გააჩერე Play. **Start Fen** და **Stockfish Path Override** გაასუფთავე → **Ctrl+S**.
    9. **კამპანია:** **Debug Campaign Start Fen** = `6k1/5ppp/8/8/8/8/5PPP/R5K1 w - - 0 1` → **Ctrl+S**. ორჯერ დააჭირე `Assets/Scenes/MainMenu`-ს → **Play** → `გაგრძელება` → `ქართლი`.
    10. ზემოთ `ქართლი - V საუკუნე - მეფე ვახტანგ გორგასალი - მოწინააღმდეგის Elo 800`, ზედა-მარჯვნივ `რუკაზე დაბრუნება`, მარცხენა სტატუსის ბოლოს `[DEBUG FEN]`. a1 → a8 → სტატუსი `მატი - თეთრმა მოიგო`; ცენტრში პანელი `გამარჯვება!` და `ქართლი კვლავ თავისუფალია.` (თუ ქართლი ჯერ არ იყო გათავისუფლებული — `ქართლი გათავისუფლდა! კახეთი ახლა ხელმისაწვდომია.`); მხოლოდ ღილაკი `რუკაზე დაბრუნება` (`თავიდან ცდა` არ ჩანს). პანელი მარცხენა სტატუსს არ ფარავს.
    11. `რუკაზე დაბრუნება` → რუკა. გააჩერე Play. **Debug Campaign Start Fen** = `7k/8/8/8/8/8/1q6/K7 w - - 0 1` → **Ctrl+S** → MainMenu → **Play** → `გაგრძელება` → `ქართლი` → სტატუსი `თეთრის სვლაა  შახი!  [DEBUG FEN]` → a1 → b2 → `ფრე - მასალის ნაკლებობა`; პანელი `ფრე` + `ქართლი კვლავ თავისუფალია.` (ან `ქართლი ჯერ კიდევ მტრის ხელშია.`), ღილაკები `თავიდან ცდა` და `რუკაზე დაბრუნება`. დააჭირე `თავიდან ცდა` → პანელი ქრება, პოზიცია თავიდან იწყება. `რუკაზე დაბრუნება` → `მენიუში დაბრუნება`. გააჩერე Play.
    12. MainScene → **Debug Campaign Start Fen** გაასუფთავე; დარწმუნდი, რომ **Start Fen** და **Stockfish Path Override** ცარიელია → **Ctrl+S**.
    13. Task Manager (Ctrl+Shift+Esc) → **Details**: `stockfish.exe` არ არის. Console: 0 წითელი, არცერთი `was not found in the`.
    14. უთხარი გუნდს: `git status`-ში `Assets/Fonts/NotoSansGeorgian SDF.asset` და `Assets/TextMesh Pro/**` **არ** უნდა იყოს შეცვლილი (Play-მ ფონტი არ უნდა შეცვალოს); `Assets/Scenes/MainScene.unity` დაკომიტდეს, თუ შეიცვალა. თუ რამე არ ემთხვევა — screenshot და Console-ის ტექსტი გადაეცი გუნდს.

## Shared files
<!-- ფაილები, რომლებიც რამდენიმე task-ის files-ში შეიძლება იყოს (shared). წესი: მხოლოდ დამატება (ახალი dependency, ტიპი, route), სხვისი ჩანაწერის შეცვლა ან წაშლა არა; ერთ საერთო ფაილზე ორი task ერთდროულად არ მუშაობს. -->
- Stage 3-ში არცერთი: ყოველ ფაილს ერთი task ეკუთვნის. `TextId.cs` / `TextTable*.cs` / `GameText.cs` — მხოლოდ 35 (ახალი ტექსტი სჭირდება? → ჯერ API Contract-ის ცხრილის განახლება და ცალკე task); asmdef-ები და compile-check csproj-ები — მხოლოდ 37; `GameController.cs` — მხოლოდ 40; `ChessSceneSetup.cs` — მხოლოდ 42; `CampaignScenesSetup.cs` — მხოლოდ 43. ახალი NuGet/Unity package არცერთ task-ს არ სჭირდება.
- რიგი (რეკომენდებული — თანმიმდევრული, რადგან `GameCompileCheck`/`EditorCompileCheck` მთელ Game/Editor საქაღალდეს აკომპილირებს და პარალელური ნახევრად დასრულებული ცვლილება სხვის build-ს ჩააგდებს): 35 → 36 → 37 → 38 → 39 → 40 → 41 → 42 → 43 → USER 45 → USER 46. task 37 შეიძლება 35/36-ის პარალელურად. USER 44 — შესრულებულია (`6d97068`).
- აგენტი არ ცვლის: `Assets/Scenes/*.unity`, `ProjectSettings/**`, `Assets/Fonts/**`, `Assets/TextMesh Pro/**`, `.meta` ფაილებს — მათ ქმნის/ცვლის USER ან USER-ის მიერ გაშვებული Editor ბრძანება (44, 45, 46).

## Backlog (needs user decision)
### ფაზა 2 — Art & Sound (კონცეფციით ~1.5 თვე)
- ქართული ისტორიული 3D მოდელები primitive-ების ნაცვლად (მეფე, ლაზიერი/მთავარსარდალი, ეტლი/კოშკი, მხედარი/აბჯროსანი ცხენოსანი, ოფიცერი/ეპისკოპოსი, პაიკი/მებრძოლი).
- კამპანიის რუკის ნამდვილი art (Stage 2-ში სქემატური ღილაკებია); რეგიონების ეპოქების საბოლოო ტექსტი.
- ხმოვანი ბრძანებები („ლაზიერი c5-ზე, შეტევაზე!“), ჯარისკაცების პასუხები, აუდიო მენეჯერი კოორდინატების მიხედვით.
- კუთხური დიალექტები (ხევსურული, კახური, სვანური).
- გარემოები: კახეთი (ალაზნის ველი, ქვევრები, გალავანი), სვანეთი (თოვლი, კოშკები); ველის ტიპები (ქვიშა, ბალახი, თოვლი).
### ფაზა 3 — Polish & FX (კონცეფციით ~1 თვე)
- მოკვლის კინემატოგრაფიული სცენები (Fatal Captures, 2–3 წამი).
- განათება და post-processing.
- ბალანსის ტესტირება, სირთულის Elo-ს გაზომვა (მათ შორის `AIDifficulty.FromElo`-ს ზოლები და რეგიონების Elo).
- სარდლის ხედში ხელები/აბჯარი.
### გადადებული (Stage 1-დან)
- Mac/Linux Stockfish და პლატფორმის ავტომატური არჩევა; build-ის/installer-ის მომზადება; Stockfish-ის build-ში გავრცელება + GPLv3 compliance.
- სვლის ანიმაცია, მოჭრილი ფიგურების ჩვენება, ბოლო სვლის და შახის (წითელი) განათება.
- Undo, PGN export/import, საათები/დროის კონტროლი, დანებება, ფრეს შეთავაზება, claim-ის რეჟიმი (50/3-ჯერადი), 75 სვლის / 5-ჯერადი წესები.
- კამერის orbit/zoom მარჯვენა ღილაკით; hot-seat-ში კამერის მობრუნება მოთამაშის მიხედვით.
- Stockfish full-strength preset.
- Unity Test Framework / Play-mode ავტომატური ტესტები.
- nit (finish-check, security): `StockfishAI.GetBestMoveAsync` engine-ს უგზავნის შემოსულ `fen`-ს და არა `state.ToFen()`-ს (ნორმალიზებული); `ProcessUciTransport.SendLine` არ უარყოფს `\r`/`\n`-ს. რეალური ზიანი არ არის (Game ყოველთვის `ToFen()`-ს აგზავნის).
- nit (finish-check, security): `StockfishAI`/`ProcessUciTransport` — `Path.GetFullPath` ერთხელ, რომ `File.Exists` და გაშვება ერთსა და იმავე ფაილს ეხებოდეს (მხოლოდ inspector-ის override-ზე).
- nit (finish-check): `StockfishAI` timeout-ები ყოველ ხაზზეა და არა მთელ ლოდინზე (engine, რომელიც მუდმივად `info`-ს ბეჭდავს, timeout-ს ვერ მიიღებს; preset-ებს არ ეხება) — deadline ერთხელ.
- nit (finish-check): `StockfishAI` — `GetBestMoveAsync`-ის ct shared init-ის დროს არ მოწმდება (გაუქმება init-ის დასრულებამდე ჩერდება); Game-ს არ ეხება.
- nit (finish-check): `StockfishAITests.GetBestMove_Cancel_DiscardsStaleBestmove...` იყენებს SearchTimeout 500 ms-ს — დატვირთვისას შეიძლება flaky იყოს; 30 s.
- nit (fix round 1 re-review): `GameController.StopAi` არ იძახებს `UpdateHud()`/`UpdateInteractable()`-ს — inspector-ში კომპონენტის disable/enable AI-ის ფიქრისას ტოვებს „AI thinking...“-ს და დაბლოკილ input-ს R-მდე (მხოლოდ Editor-ის debug სცენარი).
- nit (fix round 1 re-review): domain reload-ის შემდეგ AI ჩუმად ქრება (hot-seat) — HUD-ზე შენიშვნა არ ჩანს (მაგ. `Scripts reloaded. Human vs human mode.`).
- nit: README.md არ არსებობს — გაშვების ინსტრუქცია ახლა CLAUDE.md-ის Commands-სა და PROGRESS.md-ის USER task-ებშია.
- nit: `EnsureNonEmptyCompile` target-ის მოშორება `Tests/ChessCore.Build` და `Tests/ChessAI.Build` csproj-ებიდან, როცა Core-სა და AI-ში რეალური `.cs` ფაილებია (ცარიელი Compile set-ის დროს SDK `TargetFrameworkAttribute`-ს არ აგენერირებს).
### Stage 2-ის გარეთ დარჩენილი (გადაწყვეტილება საჭიროა)
- `SquarePicker` არ ამოწმებს, UI-ზე ხომ არ არის მაუსი (`EventSystem.IsPointerOverGameObject`) — `Back to map`-ზე დაჭერა დაფაზეც „დააჭერს“ (სცენა მაშინვე იცვლება, ზიანი არ არის). (Stage 3: გარდაქმნის ფანჯარას არ ეხება — `SquarePicker` press-ზე ირჩევს, ღილაკი release-ზე, ფანჯრის ღიაობისას click იგნორირდება; იხ. PROJECT.md Decision Log 2026-10-26.)
- ფერის არჩევა კამპანიაში; რამდენიმე save slot; კამპანიის დასრულების ეკრანი.
- nit (finish-check Stage 2, ორივე reviewer): `CampaignSaveStore.Load` — თუ ფაილი გაშვებისას სხვა პროცესს უჭირავს, წაკითხვაც და backup-ის გადატანაც ჩავარდება (ჩუმად), გაფრთხილება მაინც წერს „It was kept as campaign.corrupt.json“, ხოლო შემდეგი `New Campaign` (დადასტურების გარეშე, რადგან `Campaign == null`) ვალიდურ ფაილს გადააწერს. გამოსწორება: `TryBackUpCorruptFile` → bool; ჩავარდნისას სხვა გაფრთხილება / ჩაწერის დაბლოკვა. ძალიან იშვიათია.
- nit (finish-check Stage 2): `CampaignMapController.Start` — `regionButtons == null`-ზე ბრუნდება `backButton`-ის მიბმამდე (მხოლოდ არასწორად აწყობილ სცენაზე).
- nit (finish-check Stage 2, შესამოწმებელი): `InputSystemUIInputModule.AssignDefaultActions()` Editor-ში ქმნის არამუდმივ actions asset-ს — სცენის ფაილში reference შეიძლება null იყოს; runtime-ზე `OnEnable` აღადგენს. USER task 28-ის შემდეგ სცენის ფაილი გადასახედია.
- nit (fix round 1 re-review): `Save_TargetLocked_ThrowsAndKeepsOldSave` ძველ კოდზეც გადიოდა; `Save`-ის ტოტი „tmp რჩება, როცა target აღარ არსებობს“ პირდაპირ ტესტით არ მოწმდება (იდეა: `campaign.json` სახელის საქაღალდე → `File.Move` ვარდება → tmp რჩება). 64 KB-ის ზუსტი ზღვარი (65536 / 65537) არ ტესტირდება.
- nit (fix round 1 re-review): პირველი ჩაწერისას ნაწილობრივ ჩაწერილი tmp (მაგ. disk full) შემდეგ გაშვებაზე აღდგება და „damaged“ გაფრთხილებას აჩვენებს `NoSave`-ის ნაცვლად (უვნებელია; შესაძლო `tempWritten` flag).
### Stage 3-ის გარეთ დარჩენილი (გადაწყვეტილება საჭიროა)
- ენის არჩევა მენიუში (ქართული / English) — `TextTable.English` უკვე არსებობს; საჭიროა `Active`-ის არჩევა და შენახვა.
- ქართული Bold ფაილი (`NotoSansGeorgian-Bold.ttf`) synthetic bold-ის ნაცვლად, თუ სათაურები საკმარისად მკვეთრი არ იქნება.

## Blockers
- (მოგვარდა `0387781`-ში) dotnet build/test-ის ნებართვები `.claude/settings.json`-ში.
- (მოგვარდა) USER task-ები 10, 12, 14, 16 მომხმარებელმა დაადასტურა 2026-10-25.
- (მოგვარდა `de73a20`-ში) Game სკრიპტების `.meta` ფაილები.
- (მოგვარდა `a16b924`-ში) `Assets/Scenes/MainScene.unity` და `Assets/Materials/**` დაკომიტებულია.

## Run State
- Stage: Stage 3 — ქართული ინტერფეისი
- Finish-check this stage: not run
- Fix rounds this stage: 0/2
- Failed attempts: —
- Stage start commit: f09c5fa

## Last Session
- Date: 2026-10-26
- Status: **Stage 2 — Campaign: Done.** ყველა task (20–34) `[x]`, 0 ღია critical/should-fix. USER task-ები 28 და 29 მომხმარებელმა დაადასტურა 2026-10-26: რუკა (სათაური, ტექსტი), მოგება Kartli/Kakheti, Imereti UCI_Elo 1400, წაგება + Retry, ფრე, Stockfish not found → შედეგი არ ითვლება, Quick Game debug FEN-ს არ იყენებს, შენახვა Play-ებს შორის (მათ შორის `File.Replace` Unity Mono-ზე), დაზიანებული ფაილი → `campaign.corrupt.json` + გაფრთხილება, MainScene ცალკე Stage 1-ივით, `stockfish.exe` არ რჩება, Console = 0 შეცდომა. ხელახლა გენერირებული სცენები დაკომიტდა (MainScene-ში `startFen`, `debugCampaignStartFen`, `stockfishPathOverride` ცარიელია; მასალებში შინაარსობრივი ცვლილება არ იყო).
- Done (ისტორია): Stage 2 — Campaign, agent task-ები 20–27 და finish-check-ის 30–32 `[x]` (ბრენჩი `team/stage2-campaign`). finish-check: code-reviewer + security-auditor → 3 should-fix (შენახვის ჩავარდნისას ორივე ასლის დაკარგვა, `Continue` დამალვა, რეგიონის label-ის ჩამოჭრა) → გასწორდა fix round 1-ში, re-review: სამივე მოგვარებულია, ახალი critical/should-fix არ არის. USER task 28 გავიდა (სცენები და `.meta`-ები დაკომიტდა `bc04739`-ში). მომხმარებლის შენიშვნებით: T33 — Editor-only `debugCampaignStartFen` (`startFen` ახლა მხოლოდ Mode None-ში), T34 — რუკის სათაური MapArea-ს ზემოთ, რეგიონის label Bold 22–28, ღილაკი 400×190; task 29-ის ინსტრუქცია თავიდან დაიწერა (მოგება/წაგება/ფრე debug FEN-ით).
- Verification: `dotnet test Tests/ChessCore.Tests` 411/411; `dotnet build Tests/ChessCore.Tests`, `Tests/GameCompileCheck`, `Tests/EditorCompileCheck` — 0 warning / 0 error; legacy Input grep → 0; Campaign-ში UnityEngine/record/init → 0. STOCKFISH_PATH integration არ გაშვებულა.
- Known issues / blockers: არცერთი. ვიზუალური წვრილმანები და nit-ები Backlog-შია. T33/T34 ცალკე reviewer-ით არ შემოწმებულა (diff ორკესტრატორმა გადახედა; Play-mode-ში USER task 29-მა დაადასტურა).
- Next: Stage 3-ის არჩევა მომხმარებლის გადაწყვეტილებაა (Backlog → ფაზა 2 Art & Sound / ფაზა 3 Polish & FX / ვიზუალური წვრილმანები).
- 2026-10-26 (plan): Stage 3 — ქართული ინტერფეისი დაიგეგმა (`/auto plan:`), ბრენჩი `team/stage3-georgian-ui`. task-ები 35–46 (agent 35–43, USER 44–46). გაგრძელება: `/auto continue` (task 35-იდან; USER task 44 შეიძლება ახლავე).

## Stage 2 — Done
<!-- ფორმატი: - [ ] N. Title — owner: task-executor | USER — files: ... — depends: ... — done when: ...
     task-executor = სუფთა C#, Unity სკრიპტები (Game, UI) და Editor სკრიპტი; USER = Play-mode შემოწმება Unity Editor-ში (დეტალური ინსტრუქცია task-ის ქვეშაა).
     API-ს, ველების სახელების, ტექსტების და ქცევის ერთადერთი წყარო: PROJECT.md → ## API Contract და ## Business Rules → Stage 2.
     Game/Editor task-ები (24–27) dotnet-ით მხოლოდ კომპილაციაზე მოწმდება; საბოლოო შემოწმება = USER task-ები 28, 29.
     Stage 1-ის task-ები (1–19) — ფაილის ბოლოს, ## Stage 1 — Done. -->

- [x] 20. Campaign: assembly, build პროექტი, რეგიონების მონაცემები — owner: task-executor — files: Assets/Scripts/Campaign/Chess.Campaign.asmdef, Assets/Scripts/Campaign/Region.cs, Assets/Scripts/Campaign/CampaignRegions.cs, Tests/ChessCampaign.Build/ChessCampaign.Build.csproj, Tests/ChessCore.Tests/ChessCore.Tests.csproj, Tests/ChessCore.Tests/BuildTargetTests.cs, Tests/ChessCore.Tests/Campaign/CampaignRegionsTests.cs — depends: — — done when:
  - `dotnet build Tests/ChessCore.Tests` და `dotnet test Tests/ChessCore.Tests` გადის (ყველა ძველი ტესტი + ახლები).
  - `Chess.Campaign.asmdef`: name/rootNamespace `Chess.Campaign`, references `["Chess.Core"]`, `noEngineReferences: true`, დანარჩენი ველები როგორც `Chess.AI.asmdef`-ში.
  - `ChessCampaign.Build.csproj` = `ChessAI.Build`-ის ანალოგი: netstandard2.1, LangVersion 9.0, Nullable disable, ImplicitUsings disable, EnableDefaultCompileItems false, AssemblyName/RootNamespace `Chess.Campaign`, `<Compile Include="../../Assets/Scripts/Campaign/**/*.cs" />`, ProjectReference მხოლოდ `../ChessCore.Build/ChessCore.Build.csproj` (`EnsureNonEmptyCompile` target არ არის საჭირო — წყაროები უკვე არსებობს).
  - `ChessCore.Tests.csproj`: დაემატა მხოლოდ ProjectReference `../ChessCampaign.Build/ChessCampaign.Build.csproj`.
  - `BuildTargetTests`: `AssemblyTargetsNetStandard21`-ს დაემატა `[InlineData("Chess.Campaign")]`; ახალი ტესტი `CampaignReferencesOnlyCore` — `Assembly.Load("Chess.Campaign").GetReferencedAssemblies()`-ის სახელები ⊆ {`Chess.Core`, `netstandard`} (ე.ი. არც `Chess.AI`, არც `UnityEngine*`). არსებული ტესტები უცვლელია.
  - `Region`, `CampaignRegions` ზუსტად API Contract → Chess.Campaign-ის და Business Rules → „რეგიონები“ ცხრილის მიხედვით.
  - `CampaignRegionsTests`: `All_HasFiveRegionsInCampaignOrder` (id-ები `kartli, kakheti, imereti, svaneti, khevsureti`); `All_MatchContractTable` (Name, Era, Elo ზუსტად ცხრილიდან); `Get_UnknownOrNull_Throws`; `TryGet_And_IndexOf`; `Region_InvalidArguments_Throw` (`""`, `"Kartli"`, `"kartli1"`, ცარიელი name/era, elo 499 და 3501).
  - Grep `Assets/Scripts/Campaign`-ში: `UnityEngine|\brecord\b|\binit;` → 0. `.meta` ხელით არ შექმნილა.

- [x] 21. Campaign: პროგრესის წესები, ბრძოლის შედეგი, ტექსტები — owner: task-executor — files: Assets/Scripts/Campaign/CampaignProgress.cs, Assets/Scripts/Campaign/BattleRules.cs, Assets/Scripts/Campaign/CampaignText.cs, Tests/ChessCore.Tests/Campaign/CampaignProgressTests.cs, Tests/ChessCore.Tests/Campaign/BattleRulesTests.cs, Tests/ChessCore.Tests/Campaign/CampaignTextTests.cs — depends: 20 — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის. ტიპები: `RegionState` + `CampaignProgress` → CampaignProgress.cs; `BattleOutcome`, `BattleRules`, `BattleSummary` → BattleRules.cs; `CampaignText` → CampaignText.cs. ხელმოწერები და ტექსტები ზუსტად API Contract-ის და Business Rules → `RecordBattle` ცხრილის მიხედვით.
  - `CampaignProgressTests`:
    - `NewCampaign_OnlyFirstRegionAvailable` (kartli Available, დანარჩენი Locked, LiberatedCount 0, IsComplete false);
    - `FromLiberated_StatesFollowRule`: `{"kakheti"}` → kartli Available, kakheti Liberated, imereti Available, svaneti Locked, khevsureti Locked;
    - `LiberatedRegionIds_AreInCampaignOrder`: `["imereti","kartli"]` → `["kartli","imereti"]`;
    - `FromLiberated_InvalidInput_Throws`: null კოლექცია/ელემენტი → ArgumentNullException; უცნობი ან გამეორებული id → ArgumentException;
    - `Victory_OnAvailable_LiberatesAndOpensNext`: kartli → Title `Victory!`, Message `Kartli is liberated! Kakheti is now open.`, CanRetry false, ProgressChanged true, kakheti Available;
    - `Victory_WhenNextAlreadyLiberated_NoOpenSentence`: `FromLiberated({"kakheti"})` + kartli Victory → Message `Kartli is liberated!`;
    - `Victory_OnLastRegion_CompletesCampaign`: პირველი 4 Liberated + khevsureti Victory → Message `Khevsureti is liberated! All regions are liberated! Georgia is free.`, IsComplete true;
    - `Victory_OnLiberated_NoChange` (`Kartli remains free.`, ProgressChanged false, CanRetry false);
    - `DefeatOrDraw_OnAvailable_NoChange` (Title `Defeat` / `Draw`, Message `Kartli is still under enemy control.`, CanRetry true, ProgressChanged false);
    - `DefeatOrDraw_OnLiberated_RemainsFree`;
    - `NotCounted_AnyOutcome_NoChange` (Title `Game over`, Message = `CampaignText.NotCountedMessage`, CanRetry true, ProgressChanged false; Victory-საც კი არ ათავისუფლებს);
    - `RecordBattle_LockedRegion_ThrowsInvalidOperation` (state უცვლელი); `RecordBattle_UnknownRegion_ThrowsArgument`.
  - `BattleRulesTests`: `GetOutcome` Theory 6 შემთხვევა (WhiteWins/White → Victory, WhiteWins/Black → Defeat, BlackWins/White → Defeat, BlackWins/Black → Victory, Draw/White და Draw/Black → Draw); `GameResult.None` → ArgumentException; `GetTitle` სამივე; `NotCountedTitle == "Game over"`.
  - `CampaignTextTests`: `StateName` სამივე; `RegionLabel(kartli, Available)` == `"Kartli\n5th century - King Vakhtang Gorgasali\nElo 800\nAvailable"`; `MapStatus` ახალზე == `Liberated 0 of 5. Choose a region to fight for.`, სრულზე == `All regions are liberated! Georgia is free.`; `BattleInfo(kartli)` == `Kartli - 5th century - King Vakhtang Gorgasali - enemy Elo 800`; სამი const ზუსტი ტექსტით.

- [x] 22. Campaign: JSON serializer და save store (დაზიანებული ფაილის დამუშავება) — owner: task-executor — files: Assets/Scripts/Campaign/JsonReader.cs, Assets/Scripts/Campaign/CampaignSaveSerializer.cs, Assets/Scripts/Campaign/CampaignSaveStore.cs, Tests/ChessCore.Tests/Campaign/CampaignSaveSerializerTests.cs, Tests/ChessCore.Tests/Campaign/CampaignSaveStoreTests.cs — depends: 21 — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის. ქცევა ზუსტად API Contract → Chess.Campaign, Data → Schema v1, Business Rules → „შენახვა“ და Error model-ის მიხედვით. `CampaignSaveFormatException` → CampaignSaveSerializer.cs; `CampaignLoadStatus`, `CampaignLoadResult` → CampaignSaveStore.cs; `JsonReader` internal.
  - `CampaignSaveSerializerTests`:
    - `Serialize_New` == `{"version":1,"liberated":[]}`; `Serialize_TwoRegions` == `{"version":1,"liberated":["kartli","kakheti"]}` (კამპანიის რიგით, მიუხედავად შეყვანის რიგისა);
    - `RoundTrip_AllSubsets` (Theory/ციკლი 32 ქვესიმრავლეზე: `Deserialize(Serialize(p))`-ის LiberatedRegionIds == p-ის);
    - `Deserialize_AcceptsValidVariants`: whitespace/ახალი ხაზები, გასაღებების სხვა რიგი, უცნობი top-level გასაღები (`"extra":{"a":[1,2,null,true]}`), escape-ები (`"\u006bartli"` == kartli), `["kakheti"]` (არა-prefix);
    - `Deserialize_Invalid_ThrowsFormatException` (Theory; ყოველი → `CampaignSaveFormatException`, შეტყობინება იწყება `Invalid campaign save: `): `""`, `"   "`, `"{"`, `"null"`, `"[]"`, `"hello"`, `{"liberated":[]}` (version არ არის), `{"version":"1","liberated":[]}`, `{"version":2,"liberated":[]}`, `{"version":1.5,"liberated":[]}`, `{"version":1e0,"liberated":[]}`, `{"version":1}`, `{"version":1,"liberated":"kartli"}`, `{"version":1,"liberated":[1]}`, `{"version":1,"liberated":["rome"]}`, `{"version":1,"liberated":["kartli","kartli"]}`, `{"version":1,"version":1,"liberated":[]}`, `{"version":1,"liberated":[]} x`, დაუსრულებელი სტრიქონი, არასწორი escape `"\q"`, raw control char სტრიქონში, trailing comma `[ "kartli", ]`;
    - `Deserialize_DeepNesting_ThrowsFormatException`: `{"version":1,"liberated":[],"x":` + 100 000 `[` → `CampaignSaveFormatException` (არა StackOverflow, ტესტის პროცესი არ კვდება);
    - `Deserialize_Null_ThrowsArgumentNull`.
  - `CampaignSaveStoreTests` (თითო ტესტს თავისი საქაღალდე `Path.Combine(Path.GetTempPath(), "chess3d-tests", Guid)`, `Dispose`-ში იშლება; Assets-ში არაფერი იქმნება):
    - `Load_NoFile_ReturnsNoSave` (Progress ახალი, Warning null);
    - `Save_ThenLoad_ReturnsLoaded`; `Save_Twice_OverwritesExistingFile` (მეორე ჩაწერა არსებულ ფაილზე, `File.Replace`-ის გზა); `Save_CreatesMissingDirectory`; `Save_LeavesNoTempFile`; `Save_WritesUtf8WithoutBom` (პირველი ბაიტი `{`);
    - `Load_FileWithBom_ReturnsLoaded`;
    - `Load_Corrupted_ReturnsNewProgressWarningAndBackup`: `hello` → Status Corrupted, Progress ახალი, Warning == `CampaignText.CorruptedSaveWarning`, `campaign.json` აღარ არსებობს, `campaign.corrupt.json`-ის შინაარსი == `hello`;
    - `Load_Corrupted_OverwritesOldBackup`; `Load_EmptyFile_IsCorrupted`;
    - `Load_LockedFile_IsCorruptedAndDoesNotThrow` (ფაილი გახსნილია `FileShare.None`-ით ტესტის დროს; Status Corrupted, exception არ არის);
    - `Constructor_InvalidDirectory_Throws`; `Save_Null_ThrowsArgumentNull`.
  - Grep `Assets/Scripts/Campaign`-ში: `UnityEngine|System\.Text\.Json|Newtonsoft|\brecord\b` → 0.

- [x] 23. AI: `AIDifficulty.FromElo` (Elo → Skill Level/depth ან UCI_Elo) — owner: task-executor — files: Assets/Scripts/AI/AIDifficulty.cs, Tests/ChessCore.Tests/AI/AIDifficultyEloTests.cs — depends: — — done when:
  - `dotnet test Tests/ChessCore.Tests` გადის; არსებული `AIDifficultyTests`, `UciProtocolTests`, `StockfishAITests` უცვლელია და გადის.
  - `AIDifficulty`-ს დაემატა მხოლოდ `public const int MinUciElo = 1320;` და `public static AIDifficulty FromElo(int elo)` — ცხრილი Business Rules → „Elo → AI პარამეტრები“. კონსტრუქტორი, property-ები და `FromPreset` უცვლელია.
  - `AIDifficultyEloTests`:
    - `FromElo_Bands` (Theory, საზღვრები): 500, 800, 999 → (0, 1, 100, null); 1000, 1199 → (3, 2, 100, null); 1200, 1319 → (6, 4, 100, null); 1320, 1599 → (20, null, 300, elo); 1600, 1999 → (20, null, 500, elo); 2000, 3500 → (20, null, 1000, elo);
    - `FromElo_OutOfRange_Throws`: 499, 3501, int.MinValue → ArgumentOutOfRangeException;
    - `FromElo_MatchesPresets`: 800/1400/1800/2200 = Easy/Medium/Hard/Master (ყველა property);
    - `FromElo_Below1320_SendsSkillAndDepth`: `BuildDifficultyCommands(FromElo(1100), options{Skill Level 0..20, UCI_LimitStrength, UCI_Elo min 1320 max 3190})` == [`setoption name Skill Level value 3`, `setoption name UCI_LimitStrength value false`]; `BuildGo` == `go depth 2`;
    - `FromElo_AtOrAbove1320_SendsUciElo`: `FromElo(1800)` → [`setoption name Skill Level value 20`, `setoption name UCI_LimitStrength value true`, `setoption name UCI_Elo value 1800`]; `BuildGo` == `go movetime 500`.

- [x] 24. Game: `GameSession` + asmdef/compile-check-ის განახლება — owner: task-executor — files: Assets/Scripts/Game/Session/GameSession.cs, Assets/Scripts/Game/Chess.Game.asmdef, Tests/GameCompileCheck/GameCompileCheck.csproj — depends: 22 — done when:
  - `Chess.Game.asmdef` references = `["Chess.Core","Chess.AI","Chess.Campaign","Unity.InputSystem","UnityEngine.UI"]`; სხვა ველები უცვლელია.
  - `GameCompileCheck.csproj`: + `<Reference Include="../../Library/ScriptAssemblies/UnityEngine.UI.dll" Private="false" />` და + ProjectReference `../ChessCampaign.Build/ChessCampaign.Build.csproj`; სხვა უცვლელია.
  - `GameSession.cs` (`LaunchMode`, `SceneNames`, `GameSession`) ზუსტად API Contract → „Chess.Game — Stage 2“-ის მიხედვით, მათ შორის: static-ების reset `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`-ით (ყველა ველი, `Store`-ის და „უკვე ჩატვირთულია“ flag-ის ჩათვლით); `EnsureLoaded` ერთხელ Play-ზე + log `[GameSession] Campaign save file: <path>`; `TrySaveCampaign` იჭერს მხოლოდ `IOException`/`UnauthorizedAccessException`-ს; `StartCampaignBattle` Locked/null-ზე → InvalidOperationException; სცენების სახელები მხოლოდ `SceneNames`-იდან.
  - GameSession-ში არ არის კამპანიის წესები (მდგომარეობა მხოლოდ `CampaignProgress.GetState`-იდან).
  - `dotnet build Tests/GameCompileCheck` → 0 შეცდომა, 0 warning; `dotnet build Tests/EditorCompileCheck` კვლავ გადის; `dotnet test Tests/ChessCore.Tests` გადის; legacy Input grep (`UnityEngine\.Input\b|\bInput\.(Get|mouse|any)` `Assets/Scripts/Game`-ში) → 0.
  - საბოლოო შემოწმება: USER task 28, 29.

- [x] 25. Game UI: MainMenuController, RegionButton, CampaignMapController (uGUI) — owner: task-executor — files: Assets/Scripts/Game/UI/MainMenuController.cs, Assets/Scripts/Game/UI/RegionButton.cs, Assets/Scripts/Game/UI/CampaignMapController.cs — depends: 24 — done when:
  - Inspector ველების სახელები, ტიპები, ტექსტები, ფერები და ნაკადი ზუსტად API Contract → „Chess.Game — Stage 2“-ის მიხედვით (`[SerializeField] private`; ღილაკები `onClick.AddListener`-ით `Awake`/`Start`-ში; null ველზე `Debug.LogError` სახელით, NRE არა).
  - MainMenu: `Continue` მხოლოდ `GameSession.Campaign != null`-ზე; დადასტურება მხოლოდ არსებული კამპანიისას; Quick Game ნაგულისხმევი White/Medium, ციკლი Easy → Medium → Hard → Master → Easy; `Quit` → log + `Application.Quit()`; `warningText` წესი Contract-იდან.
  - CampaignMap: მდგომარეობები მხოლოდ `GameSession.Campaign.GetState`-იდან, ტექსტები მხოლოდ `CampaignText`-იდან; Locked → `interactable = false`; `Campaign == null` → `StartNewCampaign()`.
  - UI სკრიპტებში არ არის საკუთარი პროგრესის/განბლოკვის ლოგიკა და hard-coded რეგიონების სია (მხოლოდ `CampaignRegions`/ღილაკის `regionId`).
  - `dotnet build Tests/GameCompileCheck` → 0 შეცდომა, 0 warning; `dotnet test Tests/ChessCore.Tests` გადის; legacy Input grep → 0.
  - საბოლოო შემოწმება: USER task 28, 29.

- [x] 26. Game: BattlePanel + GameController-ის კამპანია/Quick Game ინტეგრაცია — owner: task-executor — files: Assets/Scripts/Game/UI/BattlePanel.cs, Assets/Scripts/Game/GameController.cs — depends: 23, 24 — done when:
  - `BattlePanel` ზუსტად Contract-ის ცხრილით (ველები, events, `ShowInGame`/`ShowResult`/`HideResult`/`HideAll`; `leaveButton` → `BackClicked`).
  - `GameController`: Stage 1-ის ყველა Inspector ველი (სახელი, ტიპი, ნაგულისხმევი) და ნაკადი უცვლელია; დაემატა მხოლოდ `[SerializeField] private BattlePanel battlePanel;` და „Stage 2 — GameController-ის დამატებები“: launch პარამეტრები `Start`-ში `CreateAi`-მდე; `Mode == None` → Stage 1-ის ქცევა + `battlePanel?.HideAll()`; AI-ის log ხაზი ზუსტი ფორმატით; შედეგი მუშავდება ერთხელ პარტიაზე, მხოლოდ წარმატებული სვლის შემდეგ (ადამიანის და AI-ის); `counted = playAgainstAI && _ai != null`; `RecordBattle` + `TrySaveCampaign` + `SaveFailedWarning`; Retry = R; Back → `GoToCampaignMap`/`GoToMainMenu`; events-ის გამოწერა `OnEnable`/`OnDisable`-ში.
  - GameController-ში არ არის Elo-ს ზოლები, განბლოკვის წესი ან შედეგის ტექსტები (მხოლოდ `AIDifficulty.FromElo`, `BattleRules`, `CampaignProgress.RecordBattle`, `CampaignText`); გამონაკლისი: Quick Game-ის info ხაზი და ღილაკების label-ები Contract-იდან.
  - `dotnet build Tests/GameCompileCheck` → 0 შეცდომა, 0 warning; `dotnet build Tests/EditorCompileCheck` გადის (ChessSceneSetup-ის ველები უცვლელია); `dotnet test Tests/ChessCore.Tests` გადის; legacy Input grep → 0.
  - საბოლოო შემოწმება: USER task 29 (მათ შორის MainScene standalone).

- [x] 27. Editor: Tools → Setup Campaign Scenes (MainMenu, CampaignMap, MainScene-ის BattleCanvas, Build Settings) — owner: task-executor — files: Assets/Editor/CampaignScenesSetup.cs, Assets/Editor/Chess.Editor.asmdef, Tests/EditorCompileCheck/EditorCompileCheck.csproj — depends: 25, 26 — done when:
  - `Chess.Editor.asmdef` references = `["Chess.Core","Chess.AI","Chess.Campaign","Chess.Game","Unity.InputSystem","UnityEngine.UI"]`; სხვა უცვლელია.
  - `EditorCompileCheck.csproj`: + `../../Library/ScriptAssemblies/UnityEngine.UI.dll`, + `../../Library/ScriptAssemblies/Unity.InputSystem.dll` (Private="false"), + ProjectReference `../ChessCampaign.Build/ChessCampaign.Build.csproj`.
  - `CampaignScenesSetup` ზუსტად API Contract → „Editor: Assets/Editor/CampaignScenesSetup.cs“ ნაბიჯებით 1–10: სცენების შექმნა/გახსნა, Main Camera, ერთი EventSystem (`InputSystemUIInputModule` + `AssignDefaultActions()`), გენერირებული Canvas-ების წაშლა+შექმნა, ობიექტების სახელები, `RegionButton`-ები `CampaignRegions.All`-ის რიგით და anchor-ებით, ყველა ველი `SerializedObject`-ით (ვერ ნაპოვნი → LogError), `GameController.battlePanel`, MainScene-ში `ChessGame`-ის არარსებობისას LogError და გამოტოვება, `EditorBuildSettings.scenes` = [MainMenu, CampaignMap, MainScene], ბოლოს MainMenu ღიაა, ერთი შემაჯამებელი log.
  - ფერები, ზომები, font size-ები და Canvas Scaler ზუსტად API Contract → „UI სტილი (Stage 2)“-ის მიხედვით.
  - `ChessSceneSetup.cs` არ შეცვლილა.
  - `dotnet build Tests/EditorCompileCheck` და `dotnet build Tests/GameCompileCheck` → 0 შეცდომა, 0 warning; `dotnet test Tests/ChessCore.Tests` გადის.
  - `git status --porcelain` აჩვენებს მხოლოდ ამ task-ის 3 ფაილს (აგენტს `Assets/Scenes/**` და `ProjectSettings/**` არ შეუცვლია — მათ USER-ის მიერ გაშვებული ბრძანება ცვლის).
  - საბოლოო შემოწმება: USER task 28.

### Finish-check-ის task-ები (Stage 2, 2026-10-25)
- [x] 30. (should-fix) შენახვა: ჩაწერის ჩავარდნისას ორივე ასლი არ უნდა დაიკარგოს — owner: task-executor — files: Assets/Scripts/Campaign/CampaignSaveStore.cs, Tests/ChessCore.Tests/Campaign/CampaignSaveStoreTests.cs — depends: 22 — done when:
  - პრობლემა: `Save`-ში fallback (`File.Delete` + `File.Move`) ეშვება `IOException`-ზეც (contract: მხოლოდ `PlatformNotSupportedException`); თუ Delete-ის შემდეგ Move ჩავარდება, გარე catch შლის tmp-საც → დისკზე აღარც ძველი, აღარც ახალი შენახვა (code-reviewer + security-auditor). იგივე ხდება Win32 `ReplaceFile` 1176-ზე (`IOException`, ჩანაცვლებული ფაილი უკვე აღარ არსებობს).
  - ქცევა ზუსტად PROJECT.md → Business Rules → Stage 2 → „შენახვა“ (განახლებული): fallback მხოლოდ `PlatformNotSupportedException`-ზე; ჩავარდნისას tmp იშლება მხოლოდ თუ `campaign.json` არსებობს; `Load`: `campaign.json` არ არის, `campaign.json.tmp` არის → `File.Move(tmp, campaign.json)` და შემდეგ ჩვეულებრივი წაკითხვა (აღდგენის შეცდომა → NoSave/Corrupted წესით, exception არ გადის); ფაილი > 64 KB → Corrupted (წაკითხვის გარეშე); BOM-ის შემოწმება `'﻿'` escape-ით (უხილავი ლიტერალის ნაცვლად).
  - ახალი ტესტები (ჯერ ვარდება ძველ კოდზე, სადაც შესაძლებელია): `Save_TargetLocked_ThrowsAndKeepsOldSave` (`campaign.json` გახსნილია `FileShare.None`-ით → `Save` აგდებს `IOException`-ს; ბლოკის მოხსნის შემდეგ `Load` → Loaded ძველი შინაარსით; tmp არ რჩება); `Load_OnlyTempFileExists_RecoversIt` (მხოლოდ ვალიდური `campaign.json.tmp` → Status Loaded, `campaign.json` არსებობს, tmp აღარ არის); `Load_OnlyCorruptTempFile_IsCorrupted`; `Load_FileOver64KB_IsCorrupted`; `Load_LockedFile_...` დამატებით ამოწმებს, რომ ორიგინალი ფაილის შინაარსი უცვლელია.
  - `dotnet test Tests/ChessCore.Tests` გადის (ყველა ძველი + ახალი); `dotnet build Tests/GameCompileCheck` 0/0.
- [x] 31. (should-fix) `Continue` უნდა იმალებოდეს და არა ითიშებოდეს — owner: task-executor — files: Assets/Scripts/Game/UI/MainMenuController.cs — depends: 25 — done when:
  - `Start`-ში `continueButton.gameObject.SetActive(GameSession.Campaign != null)` (`interactable` აღარ გამოიყენება), PROJECT.md → „MainMenuController-ის ნაკადი“ (განახლებული) და Core Flow 6. მომხმარებლის მოთხოვნა: „Continue (ჩანს მხოლოდ შენახვისას)“.
  - `dotnet build Tests/GameCompileCheck` 0 შეცდომა / 0 warning; legacy Input grep → 0. საბოლოო შემოწმება: USER task 28 (ნაბიჯი 7, 14), 29 (ნაბიჯი 14).
- [x] 32. (should-fix, სავარაუდო) რეგიონის ღილაკის label-ში მდგომარეობის ხაზი არ უნდა ჩამოიჭრას — owner: task-executor — files: Assets/Editor/CampaignScenesSetup.cs — depends: 27 — done when:
  - პრობლემა: `RegionLabel` 4 ხაზია (ეპოქის ხაზი 280px-ში იკეცება → 5 ხაზი ≈150px), ღილაკი 280×130, font 26, Truncate → ბოლო ხაზი (`Available`/`Locked`/`Liberated`) არ ჩანს (code-reviewer, font metrics-ით; Unity-ში არ დადასტურებულა).
  - ღილაკი 380×170, label: `resizeTextForBestFit = true`, `resizeTextMinSize = 14`, `resizeTextMaxSize = 24`, horizontal Wrap — PROJECT.md → „UI სტილი (Stage 2)“ (განახლებული) და Editor ნაბიჯი 5. anchor-ები უცვლელია (MapArea 1500×720-ში 380×170 ღილაკები ერთმანეთს არ ფარავს — შეამოწმე გამოთვლით და report-ში ჩაწერე).
  - `dotnet build Tests/EditorCompileCheck` და `Tests/GameCompileCheck` 0/0. საბოლოო შემოწმება: USER task 28 (ნაბიჯი 13).

- [x] 28. USER: სცენების აწყობა ერთი ბრძანებით + მენიუ და Quick Game — owner: USER — files: Assets/Scenes/MainMenu.unity, Assets/Scenes/CampaignMap.unity, Assets/Scenes/MainScene.unity, ProjectSettings/EditorBuildSettings.asset (ქმნის/ცვლის Tools → Setup Campaign Scenes) — depends: 27, 30, 31, 32 — done when: ყველა შემოწმება გადის; Console = 0 შეცდომა; Play-ის შემდეგ `stockfish.exe` არ რჩება
    1. გადადი Unity-ის ფანჯარაზე (ან Unity Hub → **Projects** → **chess3D**). დაელოდე, სანამ ქვედა მარჯვენა კუთხის პროგრესის ზოლი (compiling) გაქრება. Console (**Window → General → Console**) → **Clear** → წითელი შეცდომების მთვლელი = **0**.
    2. **Project** ფანჯარაში გახსენი `Assets/Scripts/Campaign`. დააჭირე `Chess.Campaign`-ს (ფაზლის ხატულა) → Inspector-ში **No Engine References** მონიშნულია, ხოლო **Assembly Definition References**-ში მხოლოდ `Chess.Core` წერია.
    3. ზედა მენიუ: **Tools → Setup Campaign Scenes**. თუ Unity გკითხავს შენახვას — **Save**. ბრძანება თავად შექმნის `MainMenu` და `CampaignMap` სცენებს, MainScene-ში დაამატებს ბრძოლის პანელს და Build Settings-ში ჩაწერს სცენების სიას.
    4. Console-ში უნდა გამოჩნდეს `[CampaignScenesSetup] Done.` (სიტყვების `WITH ERRORS` გარეშე) და ბოლო ხაზი `Build scenes: MainMenu, CampaignMap, MainScene`. თუ წითელი შეცდომაა `ChessGame not found in MainScene` — ჯერ გაუშვი **Tools → Setup Chess Scene**, შემდეგ ისევ **Tools → Setup Campaign Scenes**.
    5. გაუშვი **Tools → Setup Campaign Scenes** მეორედ. ახლა ღიაა MainMenu სცენა: **Hierarchy**-ში უნდა იყოს ზუსტად ერთი `Main Camera`, ერთი `EventSystem` და ერთი `MainMenuCanvas` (დუბლიკატები არ არის).
    6. ზედა მენიუ **File → Build Profiles** → მარცხნივ **Scene List**: სიაში თანმიმდევრობით `Scenes/MainMenu` (0), `Scenes/CampaignMap` (1), `Scenes/MainScene` (2), სამივე მონიშნული ✓. `SampleScene` სიაში არ არის. დახურე ფანჯარა (არაფერი შეცვალო).
    7. დააჭირე **Play** (▶). უნდა ჩანდეს მუქი ფონი, სათაური `Georgian Battle Chess` და ღილაკები `New Campaign`, `Quick Game`, `Quit`. `Continue` არ ჩანს (თუ ადრე უკვე გქონდა შენახული კამპანია, ჩანს — ესეც სწორია).
    8. Console-ში იპოვე ხაზი `[GameSession] Campaign save file: C:\Users\...\AppData\LocalLow\DefaultCompany\chess3D\campaign.json`. დააკოპირე ეს გზა Notepad-ში — task 29-ში დაგჭირდება.
    9. დააჭირე `Quit`: Console-ში ჩნდება `[MainMenu] Quit requested`. Editor არ იხურება — ეს ნორმალურია (Quit მუშაობს მხოლოდ build-ში).
    10. დააჭირე `Quick Game`: ჩნდება პანელი `Color: White` და `Difficulty: Medium`. დააჭირე `Color`-ს → `Color: Black`. დააჭირე `Difficulty`-ს სამჯერ → `Hard` → `Master` → `Easy`. დააჭირე `Back` → მთავარი მენიუ. ისევ `Quick Game` → აირჩიე `Color: Black`, `Difficulty: Easy` → `Start`.
    11. იტვირთება ბრძოლის სცენა: ზემოთ ცენტრში წერია `Quick Game - Easy - you play Black`, ზედა-მარჯვნივ ღილაკი `Back to menu`; კამერა შავი მეფის უკანაა; თეთრი (AI) პირველი აკეთებს სვლას 1–2 წამში. Console-ში: `[GameController] AI difficulty: skill 0, depth 1, movetime 100 ms, UCI_Elo off`.
    12. გააკეთე 1–2 სვლა, შემდეგ დააჭირე `Back to menu` → ბრუნდება მთავარი მენიუ, Console-ში წითელი შეცდომა არ არის.
    13. დააჭირე `New Campaign` → იხსნება რუკა: სათაური `Campaign Map`, 5 ყუთი. `Kartli` ოქროსფერია და წერია `Available`; დანარჩენი 4 ნაცრისფერია, წერია `Locked` და დაჭერაზე არ რეაგირებს. ქვემოთ: `Liberated 0 of 5. Choose a region to fight for.`
    14. დააჭირე `Back to menu` → ახლა მენიუში ჩანს `Continue`.
    15. გააჩერე Play (▶) და ისევ დააჭირე Play: `Continue` კვლავ ჩანს (შენახვა დისკზეა). დააჭირე `Continue` → იგივე რუკა.
    16. `Back to menu` → `New Campaign` → ჩნდება კითხვა `Start a new campaign? Current progress will be lost.` → დააჭირე `Cancel` → მთავარი მენიუ.
    17. გააჩერე Play. გახსენი Task Manager (Ctrl+Shift+Esc) → **Details** → `stockfish.exe` სიაში არ არის.
    18. ბრძანებამ შექმნა/შეცვალა ფაილები `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/CampaignMap.unity`, `Assets/Scenes/MainScene.unity` და `ProjectSettings/EditorBuildSettings.asset` (+ `.meta`-ები). Unity-მ ასევე შექმნა ახალი სკრიპტების `.meta` ფაილები: `Assets/Scripts/Campaign/*.meta` (+ `Assets/Scripts/Campaign.meta`), `Assets/Scripts/Game/UI.meta` და `Assets/Scripts/Game/UI/*.meta`, `Assets/Scripts/Game/Session.meta` და `Assets/Scripts/Game/Session/*.meta`, `Assets/Editor/CampaignScenesSetup.cs.meta`. უთხარი გუნდს, რომ **ყველა** ეს ფაილი სცენებთან ერთად დაკომიტდეს (`.meta`-ს გარეშე სხვა კომპიუტერზე UI კომპონენტები „Missing Script“ გახდება). თუ რამე არ ემთხვევა — screenshot და Console-ის ტექსტი გადაეცი გუნდს.

### USER task 28-ის შენიშვნები (2026-10-25)
- [x] 33. Game: Editor-only `debugCampaignStartFen` კამპანიის ტესტისთვის; `startFen` მხოლოდ Mode None-ში — owner: task-executor — files: Assets/Scripts/Game/GameController.cs — depends: 26 — done when:
  - ქცევა ზუსტად PROJECT.md → „Stage 2 — GameController-ის დამატებები“ (`startFen`/`debugCampaignStartFen` პუნქტი), Business Rules → Stage 2 (კამპანიის საწყისი პოზიცია) და Decision Log „USER task 28-ის შენიშვნები“:
    - ახალი `[SerializeField] private string debugCampaignStartFen = "";` (ველი `#if`-ის გარეშე; Inspector-ში „Debug Campaign Start Fen“); მისი წაკითხვა/გამოყენება მხოლოდ `#if UNITY_EDITOR` ბლოკებში — build-ში ეს გზა არ კომპილირდება;
    - FEN-ის არჩევა `CreateState`-ში: Mode None → `startFen` (როგორც Stage 1); Mode Campaign → Editor-ში `debugCampaignStartFen` (თუ ცარიელი არ არის), სხვა შემთხვევაში საწყისი პოზიცია; Mode QuickGame → ყოველთვის საწყისი პოზიცია. restart/Retry იგივე FEN-ს იყენებს. არასწორი FEN → `Debug.LogError` + საწყისი პოზიცია;
    - debug FEN აქტიურია → ერთხელ `Start`-ში `Debug.LogWarning("[GameController] DEBUG FEN active: " + fen)` და `UpdateHud`-ში სტატუსს ბოლოში `"  [DEBUG FEN]"` (შედეგის პანელის ტექსტში — არა); შედეგი ჩვეულებრივ ითვლება.
  - Stage 1-ის Inspector ველები და Mode None-ის ქცევა უცვლელია.
  - `dotnet build Tests/GameCompileCheck` 0/0 (UNITY_EDITOR-ის გარეშე = build-ის ვარიანტი) **და** `dotnet build Tests/GameCompileCheck -p:DefineConstants=UNITY_EDITOR` 0 შეცდომა (Editor-ის ვარიანტი; ბოლოს ისევ ჩვეულებრივი build); `dotnet build Tests/EditorCompileCheck` 0/0; legacy Input grep → 0. საბოლოო შემოწმება: USER task 29.
- [x] 34. Editor: CampaignMap-ის სათაური რუკის ზემოთ, რეგიონის ტექსტი უფრო დიდი — owner: task-executor — files: Assets/Editor/CampaignScenesSetup.cs — depends: 32 — done when:
  - ზუსტად PROJECT.md → „UI სტილი (Stage 2)“ (განახლებული) და Business Rules → Stage 2 რეგიონების ცხრილის anchor-ები: `MapArea` 1500×660 center (0, -50); `TitleText` (Top, center y -70, სიმაღლე 90) იქმნება `MapArea`-ს შემდეგ; რეგიონის ღილაკი 400×190; label Bold, best fit min 22 / max 28, Wrap, Truncate, 8px inset; anchor-ები kartli (0.47, 0.42), kakheti (0.80, 0.38), imereti (0.17, 0.45), svaneti (0.30, 0.78), khevsureti (0.66, 0.74).
  - report-ში გამოთვლა: 5 ღილაკის rect-ები `MapArea`-ში ერთმანეთს არ ფარავს და `MapArea`-ს არ სცდება; `TitleText`, `StatusText`, `BackButton` არ ფარავს `MapArea`-ს 1920×1080-ზე.
  - სხვა სცენები/ღილაკები უცვლელია. `dotnet build Tests/EditorCompileCheck` და `Tests/GameCompileCheck` 0/0. საბოლოო შემოწმება: USER task 29 (ნაბიჯი 1–2).

- [x] 29. USER: კამპანიის ბრძოლები, შენახვა, დაზიანებული ფაილი, MainScene ცალკე — owner: USER — files: Assets/Scenes/*.unity (ხელახლა გენერირდება Tools → Setup Campaign Scenes-ით), Assets/Scenes/MainScene.unity (Debug Campaign Start Fen / Stockfish Path Override დროებით; ბოლოს ცარიელი) — depends: 33, 34 — done when: ყველა შემოწმება გადის; Console = 0 წითელი შეცდომა; ბოლოს Debug Campaign Start Fen, Start Fen და Stockfish Path Override ცარიელია
    **სასარგებლო:** ველი **Debug Campaign Start Fen** (MainScene → `ChessGame` → **Game Controller**) მუშაობს მხოლოდ Editor-ში და მხოლოდ კამპანიის ბრძოლაში: თუ მასში FEN წერია, ყოველი კამპანიის ბრძოლა ამ პოზიციით იწყება, ზედა-მარცხენა სტატუსში კი ჩანს `[DEBUG FEN]`. ველის შეცვლის შემდეგ ყოველთვის **Ctrl+S** (სცენა უნდა შეინახო, თორემ ცვლილება არ იმოქმედებს). ცვლილება Play-ის **გაჩერებულ** მდგომარეობაში გააკეთე.
    1. დაელოდე კომპილაციას, Console → **Clear** → 0 წითელი. ზედა მენიუ **Tools → Setup Campaign Scenes** → Console: `[CampaignScenesSetup] Done.` (`WITH ERRORS`-ის გარეშე).
    2. ღიაა MainMenu. **Play** → `New Campaign` (თუ კითხვა გამოჩნდა — `Start new`). რუკაზე: სათაური `Campaign Map` მთლიანად ჩანს რუკის მწვანე არის **ზემოთ**; 5 ღილაკი ერთმანეთს არ ფარავს; ღილაკზე 4 ხაზი (სახელი, ეპოქა, `Elo ...`, `Available`/`Locked`) კარგად იკითხება და ბოლო ხაზი არ არის მოჭრილი. გააჩერე Play.
    3. Project → `Assets/Scenes` → ორჯერ დააჭირე `MainScene`-ს. Hierarchy → `ChessGame` → Inspector → **Game Controller**: **Battle Panel**-ში წერია `BattleCanvas`; **Start Fen** ცარიელია. **Debug Campaign Start Fen**-ში ჩაწერე `6k1/5ppp/8/8/8/8/5PPP/R5K1 w - - 0 1` (თეთრი მატს სვამს ერთ სვლაში) → **Ctrl+S**.
    4. ორჯერ დააჭირე `Assets/Scenes/MainMenu`-ს → **Play** → `Continue` (ან `New Campaign`) → `Kartli`.
    5. ბრძოლის სცენა: ზემოთ `Kartli - 5th century - King Vakhtang Gorgasali - enemy Elo 800`, ზედა-მარჯვნივ `Back to map`, ზედა-მარცხენა სტატუსში `[DEBUG FEN]`. Console: ყვითელი `[GameController] DEBUG FEN active: ...` და `[GameController] AI difficulty: skill 0, depth 1, movetime 100 ms, UCI_Elo off`.
    6. **მოგება:** დააჭირე თეთრ ეტლს a1-ზე, შემდეგ a8-ს. სტატუსი `Checkmate - White wins`. ცენტრში პანელი: `Victory!` და `Kartli is liberated! Kakheti is now open.`; ჩანს მხოლოდ `Back to map` (`Retry` არ არის).
    7. `Back to map` → `Kartli` მწვანეა (`Liberated`), `Kakheti` ოქროსფერი (`Available`); ქვემოთ `Liberated 1 of 5. ...`.
    8. დააჭირე `Kakheti` → ზემოთ `... enemy Elo 1100`; Console: `skill 3, depth 2, movetime 100 ms, UCI_Elo off`. ისევ a1 → a8 → `Victory!`, `Kakheti is liberated! Imereti is now open.` → `Back to map`.
    9. დააჭირე `Imereti` → Console: `skill 20, depth -, movetime 300 ms, UCI_Elo 1400`. სვლის გარეშე დააჭირე `Back to map` → რუკა; `Imereti` ისევ `Available`.
    10. **წაგება:** გააჩერე Play. MainScene → **Debug Campaign Start Fen** = `6r1/8/8/8/8/8/P4k2/7K w - - 0 1` → **Ctrl+S**. MainMenu → **Play** → `Continue` → `Imereti`. გააკეთე ნებისმიერი სვლა (მაგ. პაიკი a2 → a3). შავი (AI) პასუხობს მატით (`Rh8`): სტატუსი `Checkmate - Black wins`; პანელი `Defeat` და `Imereti is still under enemy control.`; ღილაკები `Retry` და `Back to map`. (თუ AI-მ მატი არ დასვა — დააჭირე `Retry`/**R** და სცადე თავიდან.)
    11. დააჭირე `Retry` → პანელი ქრება, იგივე პოზიცია თავიდან იწყება (`[DEBUG FEN]` ისევ ჩანს). დააჭირე `Back to map` → `Imereti` ისევ `Available`.
    12. **ფრე:** გააჩერე Play. **Debug Campaign Start Fen** = `7k/8/8/8/8/8/1q6/K7 w - - 0 1` → **Ctrl+S**. MainMenu → **Play** → `Continue` → `Imereti` → დააჭირე მეფეს a1-ზე, შემდეგ b2-ს (დედოფალს ჭრის) → `Draw - insufficient material`; პანელი `Draw` და `Imereti is still under enemy control.`; ღილაკები `Retry` და `Back to map`.
    13. დააჭირე `Back to map` → `Kartli` (უკვე განთავისუფლებული) → a1 → b2 → პანელი `Draw` და `Kartli remains free.` → `Back to map` → `Kartli` ისევ `Liberated`.
    14. **AI არ არის = არ ითვლება:** გააჩერე Play. MainScene → **Stockfish Path Override** = `C:\nope\stockfish.exe`, **Debug Campaign Start Fen** = `6k1/5ppp/8/8/8/8/5PPP/R5K1 w - - 0 1` → **Ctrl+S**. MainMenu → **Play** → `Continue` → `Imereti` → ზედა-მარცხნივ ყვითელი `Stockfish not found ...` → a1 → a8 → პანელი `Game over` და `The AI opponent was not active, so this result does not count.` → `Back to map` → `Imereti` ისევ `Available`. გააჩერე Play, **Stockfish Path Override** გაასუფთავე → **Ctrl+S**.
    15. **Quick Game debug FEN-ს არ იყენებს:** MainMenu → **Play** → `Quick Game` → `Start`: დაფაზე ჩვეულებრივი საწყისი პოზიციაა (32 ფიგურა), `[DEBUG FEN]` არ ჩანს. `Back to menu`.
    16. `Continue`: `Kartli` და `Kakheti` — `Liberated`, `Imereti` — `Available` (პროგრესი შენახულია, მათ შორის მეორე ჩაწერა არსებულ ფაილზე). გააჩერე Play.
    17. **დაზიანებული ფაილი:** Windows Explorer-ის მისამართის ზოლში ჩასვი `%USERPROFILE%\AppData\LocalLow\DefaultCompany\chess3D` → Enter. `campaign.json`-ზე მარჯვენა click → **Open with → Notepad**: შინაარსი `{"version":1,"liberated":["kartli","kakheti"]}`. წაშალე ყველაფერი, ჩაწერე `hello` → **Ctrl+S** → დახურე Notepad.
    18. MainMenu → **Play**: ყვითლად წერია `The saved campaign was damaged and could not be loaded. It was kept as campaign.corrupt.json. Start a new campaign.`, `Continue` არ ჩანს, Console-ში წითელი შეცდომა არ არის. Explorer-ში ახლა არის `campaign.corrupt.json` (შიგნით `hello`). დააჭირე `New Campaign` → კითხვის გარეშე იხსნება რუკა, `Kartli` = `Available`. გააჩერე Play.
    19. **MainScene ცალკე (Stage 1-ის ქცევა):** MainScene → **Debug Campaign Start Fen** გაასუფთავე → **Ctrl+S**. MainScene ღიაა → **Play**: ზემოთ ცენტრში ტექსტი და `Back to ...` ღილაკი **არ** ჩანს, `[DEBUG FEN]` არ ჩანს; AI პასუხობს (Console: `skill 0, depth 1`); **R** თავიდან იწყებს; TAB მუშაობს. გააჩერე Play.
    20. Task Manager (Ctrl+Shift+Esc) → **Details**: `stockfish.exe` არ არის.
    21. შეამოწმე, რომ GameController-ში **Debug Campaign Start Fen**, **Start Fen** და **Stockfish Path Override** ცარიელია და სცენა შენახულია (**Ctrl+S**). ნაბიჯ 1-მა ხელახლა შექმნა სცენები — უთხარი გუნდს, რომ `Assets/Scenes/*.unity` დაკომიტდეს. თუ რამე არ ემთხვევა — screenshot და Console-ის ტექსტი გადაეცი გუნდს.

### Stage 2-ის Shared files შენიშვნა (არქივი)
- Stage 2-ში არცერთი: ყოველ ფაილს ერთი task ეკუთვნის. `Tests/ChessCore.Tests/ChessCore.Tests.csproj` და `BuildTargetTests.cs` — მხოლოდ task 20; `Chess.Game.asmdef` და `GameCompileCheck.csproj` — მხოლოდ 24; `Chess.Editor.asmdef` და `EditorCompileCheck.csproj` — მხოლოდ 27; `GameController.cs` — მხოლოდ 26; `AIDifficulty.cs` — მხოლოდ 23. ახალი NuGet პაკეტი არცერთ task-ს არ სჭირდება.
- რიგი: 20 → 21 → 22 → 24 → (25 ∥ 26) → 27 → USER 28 → USER 29. task 23 შეიძლება 20–22-ის პარალელურად (ფაილები არ ემთხვევა). Campaign-ის ტესტები `Tests/ChessCore.Tests/Campaign/`-ში ცალ-ცალკე ფაილებშია.
- `ProjectSettings/EditorBuildSettings.asset` და `Assets/Scenes/*.unity` აგენტი არ ცვლის — მხოლოდ USER-ის მიერ გაშვებული Editor ბრძანებები (task 28/29).

## Stage 1 — Done
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

- [x] 10. USER: სცენის აწყობა (ერთი ბრძანებით) + ველი და ფიგურები — owner: USER — files: Assets/Scenes/MainScene.unity, Assets/Materials/** (ქმნის Tools → Setup Chess Scene) — depends: 9, 19 — done when: Play-ში ჩანს ველი, ბადე და 32 ფიგურა სწორ ადგილებზე; Console = 0 შეცდომა
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

- [x] 12. USER: სვლების შემოწმება — owner: USER — files: Assets/Scenes/MainScene.unity — depends: 10, 11 — done when: ყველა ქვემოთ ჩამოთვლილი შემოწმება გადის; Console = 0 შეცდომა
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

- [x] 14. USER: კამერის შემოწმება — owner: USER — files: Assets/Scenes/MainScene.unity — depends: 12, 13 — done when: ქვემოთ ჩამოთვლილი შემოწმებები გადის; Console = 0 შეცდომა
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

- [x] 16. USER: საბოლოო აწყობა და MVP-ის მიღება — owner: USER — files: Assets/Scenes/MainScene.unity — depends: 8, 14, 15, 17 — done when: ყველა შემოწმება გადის; Console = 0 შეცდომა; Play-ის შემდეგ stockfish.exe პროცესი აღარ არსებობს
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

### Stage 1-ის Shared files შენიშვნა (არქივი)
- `Assets/Scripts/Game/GameController.cs`-ს რიგრიგობით ავრცობდა task-ები 9 → 11 → 13 → 15 (`depends`-ით ჯაჭვი).
