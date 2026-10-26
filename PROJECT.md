# Georgian Battle Chess (chess3D)

## Overview
3D ჭადრაკი Unity 6-ზე, კონცეფციით „ქართული საბრძოლო ჭადრაკი“ (`docs/CONCEPT.md`): ჩვეულებრივი დაფის ნაცვლად რეალური საბრძოლო ველია. ველზე ჩანს ოდნავ შესამჩნევი 8x8 ბადე, რომელიც ფიგურის არჩევისას ანათებს დასაშვებ უჯრებს. მოთამაშეს აქვს „სარდლის ხედი“ (პირველი პირი, მეფის პოზიციიდან) და ტაქტიკური ხედი (TAB). მოწინააღმდეგეა Stockfish, სირთულით დაახლოებით 800-დან 2200+ Elo-მდე.

**Stage 1 (MVP)** ამოწმებს მთავარ ტექნიკურ საფუძველს: სწორი წესები, 3D ველი და ბადე, სვლა მაუსით, ორი კამერა და Stockfish-ის მიერთება. გრაფიკა დროებითია (Unity primitive-ები). **სტატუსი: Done.**

**Stage 2 (Campaign)** ამატებს თამაშის „გარსს“: მთავარი მენიუ, კამპანიის სქემატური რუკა 5 ქართული რეგიონით (ქართლი, კახეთი, იმერეთი, სვანეთი, ხევსურეთი), ბრძოლა რეგიონის Elo-ს მიხედვით, რეგიონის განთავისუფლება და პროგრესის შენახვა JSON ფაილში. Quick Game = ერთი პარტია ფერის და სირთულის არჩევით. ვიზუალი ისევ მარტივია (uGUI ღილაკები, ინგლისური ტექსტი); რუკის art და ქართული font — Stage 3.

> დამწყებისთვის: პროექტი ოთხ ნაწილად იყოფა. **Core** არის „ჭადრაკის ტვინი“ (წესები, სვლები, FEN) Unity-ის გარეშე, ამიტომ მის შემოწმებას ტესტებით Unity-ის გახსნა არ სჭირდება. **AI** ელაპარაკება Stockfish პროგრამას. **Campaign** (Stage 2) არის „კამპანიის ტვინი“: რეგიონები, განბლოკვის წესები და შენახვა — ისიც Unity-ის გარეშე და ტესტებით შემოწმებადი. **Game** არის Unity-ის ნაწილი: ხატავს ველს, ფიგურებს და მენიუებს და კითხულობს მაუსს და კლავიატურას. Game თავად არასდროს წყვეტს, სვლა დასაშვებია თუ არა ან რეგიონი გაიხსნა თუ არა; ამას ყოველთვის Core-ს ან Campaign-ს ეკითხება.

## Users
- ერთი ადამიანი, თავის Windows PC-ზე (ლოკალურად, ინტერნეტის გარეშე). როლები, ანგარიშები ან ავტორიზაცია არ არსებობს.

## Current Stage
Stage 2 — Campaign (Done, 2026-10-26; Stage 3 — მომხმარებლის არჩევანი)

## Scope
### In scope (Stage 1) — Done
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

### In scope (Stage 2)
- `Chess.Campaign` (სუფთა C#, netstandard2.1, დამოკიდებულია მხოლოდ Core-ზე): 5 რეგიონი (სახელი, ეპოქა, Elo), მდგომარეობები Locked / Available / Liberated, ბრძოლის შედეგის წესები და ტექსტები, JSON შენახვა/ჩატვირთვა საკუთარი მინიმალური JSON reader-ით, დაზიანებული ფაილის დამუშავება. სრულად იტესტება `dotnet test`-ით.
- `Chess.AI`: `AIDifficulty.FromElo(int)` — Elo < 1320 → Skill Level + depth; Elo ≥ 1320 → UCI_LimitStrength + UCI_Elo. არსებული preset-ები და API უცვლელია.
- `Chess.Game` (Unity, uGUI):
  - სცენები `MainMenu`, `CampaignMap`, `MainScene` (არსებული); მონაცემები სცენებს შორის — static `GameSession`;
  - მთავარი მენიუ: `New Campaign`, `Continue` (მხოლოდ მოქმედი შენახვისას), `Quick Game` (ფერი + სირთულე), `Quit`;
  - კამპანიის რუკა: 5 ღილაკი სქემატურ რუკაზე (სახელი, ეპოქა, Elo, მდგომარეობა);
  - ბრძოლის პანელი MainScene-ში: ინფო ხაზი, `Back to map`/`Back to menu`, შედეგი, `Retry`/`Play again`;
  - `GameController` launch პარამეტრებს კითხულობს მხოლოდ მაშინ, თუ ისინი არსებობს; MainScene პირდაპირ Play-ით Stage 1-ის ქცევას ინარჩუნებს.
- Editor: **Tools → Setup Campaign Scenes** — ქმნის/აახლებს MainMenu-ს, CampaignMap-ს, MainScene-ის ბრძოლის UI-ს და Build Settings-ის სცენების სიას (idempotent).

### Out of scope (Stage 2)
- რუკის ნამდვილი art, რეგიონების გარემოები (ველის ტიპები), 3D მოდელები, ხმა, დიალექტები, cutscene-ები — Backlog.
- ქართული UI ტექსტი / TextMeshPro + ქართული font (Stage 3).
- რამდენიმე save slot, პარტიის შუაში შენახვა, cloud save, undo, PGN, საათები, დანებება.
- პარამეტრების მენიუ (ხმა, გრაფიკა, keybinding), Elo-ს გაზომვა და ბალანსი.
- კამპანიაში ფერის არჩევა (კამპანიაში ადამიანი ყოველთვის თეთრია).
- build / installer, Stockfish-ის გავრცელება.

## Core Flows
1. **თამაშის დაწყება:** Play → ველი, ბადე და 32 ფიგურა → კამერა სარდლის ხედშია (ადამიანის მეფის უკან) → Stockfish-ის ინიციალიზაცია ფონურად.
2. **ადამიანის სვლა:** დააჭერ საკუთარ ფიგურას → ფიგურის უჯრა მოინიშნება და დასაშვები უჯრები ანათებს → დააჭერ დასაშვებ უჯრას → სვლა სრულდება → (თუ ვარიანტი ჩართულია) კამერა სარდლის ხედს უბრუნდება.
   - თუ სვლა გარდაქმნაა, ჯერ ჩნდება არჩევანი Queen/Rook/Bishop/Knight.
3. **AI-ის სვლა:** input იბლოკება → სტატუსი „AI thinking...“ → Stockfish აბრუნებს სვლას → Core ამოწმებს და ასრულებს → input იხსნება.
4. **თამაშის დასრულება:** მატი, პატი, 50 სვლა, სამჯერადი გამეორება ან მასალის ნაკლებობა → სტატუსი აჩვენებს შედეგს → სვლები აღარ მიიღება → R იწყებს ახალ თამაშს.
5. **Stockfish არ არის / გაფუჭდა:** სტატუსი აჩვენებს მკაფიო შეცდომას → თამაში გრძელდება hot-seat რეჟიმში (ორივე მხარეს მაუსით ათამაშებ).

### Stage 2
6. **მთავარი მენიუ:** Play `MainMenu`-დან → `GameSession.EnsureLoaded()` კითხულობს `campaign.json`-ს (ერთხელ Play-ზე) → ჩანს `New Campaign`, `Continue` (მხოლოდ მაშინ, თუ `GameSession.Campaign != null`), `Quick Game`, `Quit`. დაზიანებული ფაილისას ჩანს ყვითელი გაფრთხილება და `Continue` დამალულია.
7. **ახალი კამპანია:** `New Campaign` → თუ კამპანია უკვე არსებობს, დადასტურების პანელი (`Start new` / `Cancel`) → ცარიელი პროგრესი იწერება ფაილში → `CampaignMap`.
8. **გაგრძელება:** `Continue` → `CampaignMap` (პროგრესი მეხსიერებიდან).
9. **რუკა:** 5 ღილაკი (სახელი, ეპოქა, Elo, მდგომარეობა) + სტატუსის ხაზი + `Back to menu`. Locked ღილაკი გათიშულია; Available ან Liberated → ბრძოლა.
10. **კამპანიის ბრძოლა:** `GameSession.StartCampaignBattle(id)` → `MainScene`: ადამიანი = თეთრი, AI = `AIDifficulty.FromElo(region.Elo)`; ზემოთ ინფო ხაზი, ზედა-მარჯვნივ `Back to map` → პარტია → დასრულებისას შედეგის პანელი:
    - გამარჯვება → რეგიონი Liberated → შენახვა → შემდეგი რეგიონი იხსნება → ღილაკი `Back to map`;
    - წაგება / ფრე → `Retry` (იგივე ბრძოლა თავიდან) ან `Back to map`.
11. **Quick Game:** მენიუში ფერი (`Color: White/Black`) + სირთულე (`Difficulty: Easy/Medium/Hard/Master`) → `Start` → `MainScene`; დასრულებისას `Play again` / `Back to menu`; არაფერი ინახება.
12. **MainScene პირდაპირ (Editor-ში):** `GameSession.Mode == None` → Stage 1-ის ქცევა (inspector-ის ველები, ინფო ხაზის, ღილაკების და შედეგის პანელის გარეშე).
13. **დაზიანებული შენახვა:** ფაილი გადაირქმევა `campaign.corrupt.json`-ად → მენიუ აჩვენებს გაფრთხილებას → `New Campaign` იწყებს ახალს (დადასტურების გარეშე, რადგან მოქმედი კამპანია არ არის).

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

### Stage 2 — Campaign
**რეგიონები (კამპანიის რიგი = ცხრილის რიგი; ეპოქის ტექსტი placeholder-ია, იხ. Decision Log):**

| # | Id | Name | Era | Elo | რუკაზე (anchor x, y) |
|---|---|---|---|---|---|
| 1 | `kartli` | Kartli | `5th century - King Vakhtang Gorgasali` | 800 | (0.47, 0.42) |
| 2 | `kakheti` | Kakheti | `16th century - King Levan` | 1100 | (0.80, 0.38) |
| 3 | `imereti` | Imereti | `17th century - King Alexander III` | 1400 | (0.17, 0.45) |
| 4 | `svaneti` | Svaneti | `12th century - King David the Builder` | 1800 | (0.30, 0.78) |
| 5 | `khevsureti` | Khevsureti | `18th century - King Erekle II` | 2200 | (0.66, 0.74) |

**მდგომარეობის წესი (გამოითვლება, არ ინახება):**
- `Liberated` — id არის განთავისუფლებულების სიაში;
- `Available` — არ არის Liberated და (პირველი რეგიონია **ან** წინა რეგიონი Liberated-ია);
- `Locked` — დანარჩენი.

განთავისუფლება მუდმივია: შემდგომი წაგება ან ფრე მას არ აუქმებს.

**ბრძოლის წესები:**
- კამპანიაში ადამიანი ყოველთვის თეთრია, AI — შავი, სირთულე = `AIDifficulty.FromElo(region.Elo)`. საწყისი პოზიცია სტანდარტულია. `startFen` მოქმედებს მხოლოდ `Mode == None`-ში (MainScene პირდაპირ). ტესტისთვის კამპანიაში — მხოლოდ Editor-ში `debugCampaignStartFen` (იხ. „Stage 2 — GameController-ის დამატებები“); Quick Game ყოველთვის სტანდარტული პოზიციიდან იწყება.
- Locked რეგიონზე ბრძოლა შეუძლებელია (`InvalidOperationException`). Liberated რეგიონის თავიდან თამაში შეიძლება; შედეგი პროგრესს არ ცვლის.
- შედეგი (`BattleRules.GetOutcome`): ადამიანის მხარის მოგება = `Victory`, მოწინააღმდეგის = `Defeat`, ნებისმიერი ფრე (პატი, 50 სვლა, გამეორება, მასალის ნაკლებობა) = `Draw`.
- შედეგი **ითვლება** მხოლოდ მაშინ, თუ პარტიის დასრულების მომენტში AI მოწინააღმდეგე აქტიურია (`playAgainstAI && _ai != null`). hot-seat-ზე გადასვლის შემდეგ (Stockfish არ არის / გაფუჭდა / domain reload) შედეგი არ ითვლება.
- შედეგი მუშავდება ერთხელ პარტიაზე (მხოლოდ წარმატებული სვლის შემდეგ, რომელმაც თამაში დაასრულა). R / `Retry` იწყებს იმავე ბრძოლას თავიდან.
- პარტიის შუაში `Back to map` / `Back to menu` → გასვლა შედეგის ჩაწერის გარეშე.
- `New Campaign`, როცა კამპანია არსებობს → დადასტურება; `Start new` გადააწერს შენახვას ცარიელი პროგრესით.

**`CampaignProgress.RecordBattle(regionId, outcome, counted)` → `BattleSummary` (ზუსტი ტექსტები):**

| შემთხვევა | Title | Message | CanRetry | ProgressChanged |
|---|---|---|---|---|
| `counted == false` (ნებისმიერი outcome) | `Game over` | `The AI opponent was not active, so this result does not count.` | true | false |
| Victory, რეგიონი Available იყო | `Victory!` | `{Name} is liberated!` + (თუ შემდეგი რეგიონი არსებობს და ახლა Available-ია: ` {NextName} is now open.`) + (თუ ყველა Liberated: ` All regions are liberated! Georgia is free.`) | false | true |
| Victory, რეგიონი უკვე Liberated | `Victory!` | `{Name} remains free.` | false | false |
| Defeat / Draw, რეგიონი Available | `Defeat` / `Draw` | `{Name} is still under enemy control.` | true | false |
| Defeat / Draw, რეგიონი Liberated | `Defeat` / `Draw` | `{Name} remains free.` | true | false |

**Quick Game:** ფერი White/Black (ნაგულისხმევი White), სირთულე = `DifficultyPreset` (ნაგულისხმევი Medium, ციკლი Easy → Medium → Hard → Master → Easy). შედეგი არ ინახება. Title = `BattleRules.GetTitle(outcome)`, Message = სტატუსის ტექსტი (მაგ. `Checkmate - White wins`); ღილაკები `Play again` / `Back to menu`.

**Elo → AI პარამეტრები (`AIDifficulty.FromElo`, ზუსტი მნიშვნელობები; Elo მიახლოებითია და გაზომილი არ არის):**

| Elo | SkillLevel | Depth | MoveTimeMs | Elo property (UCI_LimitStrength + UCI_Elo) | `go` |
|---|---|---|---|---|---|
| 500–999 | 0 | 1 | 100 | null (LimitStrength false) | `go depth 1` |
| 1000–1199 | 3 | 2 | 100 | null | `go depth 2` |
| 1200–1319 | 6 | 4 | 100 | null | `go depth 4` |
| 1320–1599 | 20 | null | 300 | elo | `go movetime 300` |
| 1600–1999 | 20 | null | 500 | elo | `go movetime 500` |
| 2000–3500 | 20 | null | 1000 | elo | `go movetime 1000` |

< 500 ან > 3500 → `ArgumentOutOfRangeException`. შედეგად `FromElo(800)` = Easy, `FromElo(1400)` = Medium, `FromElo(1800)` = Hard, `FromElo(2200)` = Master (ყველა property ემთხვევა).

**შენახვა:**
- schema version = 1. ფაილი იწერება: New Campaign-ზე და ყოველ `ProgressChanged == true`-ზე.
- ჩაწერა ატომურია: `campaign.json.tmp` → `File.Replace(tmp, campaign.json, null)` (თუ ფაილი არსებობს) ან `File.Move` (თუ არ არსებობს); თუ `File.Replace` აგდებს `PlatformNotSupportedException`-ს → `File.Delete` + `File.Move`. ნებისმიერი ჩაწერის შეცდომისას `campaign.json.tmp` იშლება მხოლოდ მაშინ, თუ `campaign.json` ჯერ კიდევ არსებობს (ერთი ასლი ყოველთვის რჩება დისკზე). `Load`: თუ `campaign.json` არ არსებობს, მაგრამ `campaign.json.tmp` არსებობს → ჯერ `File.Move(tmp, campaign.json)` (წინა ჩაწერის აღდგენა), შემდეგ ჩვეულებრივი წაკითხვა. ფაილი > 64 KB → დაზიანებულად ითვლება (წაკითხვის გარეშე).
- დაზიანებული ფაილი = ნებისმიერი JSON/schema შეცდომა ან კითხვის I/O შეცდომა → `campaign.corrupt.json`-ად გადატანა (არსებული backup იცვლება; გადატანის შეცდომა იგნორირდება) → ახალი (ცარიელი) პროგრესი → გაფრთხილება: `The saved campaign was damaged and could not be loaded. It was kept as campaign.corrupt.json. Start a new campaign.` თამაში არასდროს ვარდება შენახვის გამო.
- ჩაწერის შეცდომა (`IOException` / `UnauthorizedAccessException`) → `Debug.LogException` + ტექსტი `Warning: progress could not be saved.` (პროგრესი მეხსიერებაში რჩება).

## Data
**Stage 1:** პარტიის მონაცემები მხოლოდ მეხსიერებაშია.
- `Position`: 64 უჯრა, სვლის რიგი, როქის უფლებები, en passant უჯრა, halfmove clock, fullmove number.
- `GameState`: მიმდინარე `Position`, სვლების ისტორია, გამეორების მთვლელი, სტატუსი და შედეგი.
- `AIDifficulty`: Skill Level, depth/movetime, Elo.

**Stage 2:** ერთი ლოკალური ფაილი — კამპანიის პროგრესი. ბაზა არ არის.
- `Region` (static მონაცემი კოდში): Id, Name, Era, Elo. `CampaignProgress`: განთავისუფლებული რეგიონების id-ების სიმრავლე (დანარჩენი გამოითვლება).
- ფაილი: `Path.Combine(Application.persistentDataPath, "campaign.json")` → Windows-ზე `%USERPROFILE%\AppData\LocalLow\DefaultCompany\chess3D\campaign.json`. იმავე საქაღალდეში: `campaign.json.tmp` (დროებითი), `campaign.corrupt.json` (დაზიანებულის backup).
- კოდირება: UTF-8 BOM-ის გარეშე; წაკითხვისას BOM დასაშვებია.
- Schema v1 (writer წერს ზუსტად ასე, compact, კამპანიის რიგით):
  ```json
  {"version":1,"liberated":["kartli","kakheti"]}
  ```
  - `version` — მთელი რიცხვის literal (წილადის/exponent-ის გარეშე), სავალდებულო, == 1;
  - `liberated` — სტრიქონების მასივი, სავალდებულო, თითო = ცნობილი region id, გამეორების გარეშე, რიგი ნებისმიერი (მაგ. `["kakheti"]` დასაშვებია — მდგომარეობები წესით გამოითვლება);
  - სხვა top-level გასაღებები იგნორირდება; გასაღების გამეორება, root არა-ობიექტი, ზედმეტი ტექსტი JSON-ის შემდეგ, ჩადგმულობა > 64 → დაზიანებული.
  - ახალი კამპანია: `{"version":1,"liberated":[]}`.

## Integrations
- **Stockfish** (ლოკალური პროგრამა, GPLv3), UCI პროტოკოლით stdin/stdout-ზე. binary git-ში არ ინახება. ნაგულისხმევი გზაა `Assets/StreamingAssets/Stockfish/stockfish.exe`; inspector-ში შეიძლება შეცვლა. MVP-ში მხოლოდ Windows x86-64.
- ყველა გამოძახებას აქვს timeout. Stockfish-ის შეცდომა თამაშს არ აჩერებს (hot-seat fallback).

## Stack & Architecture
- **Complexity tier:** Tier 1 — ერთი desktop აპლიკაცია; backend, ბაზა და ქსელი არ არის.
- **Engine:** Unity 6000.6.7f1, URP 17.6.0, Input System 1.20.1. მხოლოდ ახალი Input System (`activeInputHandler: 1`).
- **ენა:** C# 9. Core და AI იწერება netstandard2.1 API-ზე (Unity-ის Api Compatibility Level = .NET Standard 2.1).
- **ტესტები:** xUnit 2.x, net10.0 (ერთადერთი დაყენებული SDK: .NET 10.0.402).
- **AI:** Stockfish, UCI, `System.Diagnostics.Process`.
- **UI (Stage 2):** uGUI (`com.unity.ugui` 2.6.0, უკვე `Packages/manifest.json`-შია): Canvas + legacy `UnityEngine.UI.Text` (`LegacyRuntime.ttf`), EventSystem + `InputSystemUIInputModule`. TextMeshPro არ გამოიყენება. MainScene-ის HUD და გარდაქმნის ფანჯარა რჩება IMGUI.
- **Storage (Stage 2):** ერთი ლოკალური JSON ფაილი `Application.persistentDataPath`-ში.
- **Frontend / Backend / Database / Auth / Hosting:** არ არის (ლოკალური თამაში).

### Assemblies და დამოკიდებულებები
```text
Chess.Core     (Assets/Scripts/Core,     noEngineReferences)  ← არაფერზეა დამოკიდებული
Chess.AI       (Assets/Scripts/AI,       noEngineReferences)  ← Chess.Core
Chess.Campaign (Assets/Scripts/Campaign, noEngineReferences)  ← Chess.Core            (Stage 2)
Chess.Game     (Assets/Scripts/Game,     UnityEngine)         ← Chess.Core, Chess.AI, Chess.Campaign, Unity.InputSystem, UnityEngine.UI
Chess.Editor   (Assets/Editor,           Editor-only)         ← Chess.Core, Chess.AI, Chess.Campaign, Chess.Game, Unity.InputSystem, UnityEngine.UI
```
მიმართულება მხოლოდ ასეთია. Core-მა არ იცის AI-ის, Campaign-ის ან Unity-ის შესახებ. AI-მა და Campaign-მა ერთმანეთის და Unity-ის შესახებ არ იციან. Elo → AI პარამეტრების გადაყვანა AI-შია (`AIDifficulty.FromElo`); Game აერთებს: `AIDifficulty.FromElo(CampaignRegions.Get(id).Elo)`.

### Folder tree
```text
Assets/
  Scenes/
    MainMenu.unity                    # Stage 2: Tools → Setup Campaign Scenes ქმნის
    CampaignMap.unity                 # Stage 2: Tools → Setup Campaign Scenes ქმნის
    MainScene.unity                   # თამაშის სცენა (Tools → Setup Chess Scene; ბრძოლის UI — Setup Campaign Scenes)
    SampleScene.unity                 # Unity-ის შაბლონი, არ გამოიყენება (Build Settings-იდან ამოღებულია)
  Materials/                          # Tools → Setup Chess Scene ქმნის (M_Ground, M_Tile, ...)
  StreamingAssets/Stockfish/
    README.md                         # სად დადო stockfish.exe
    stockfish.exe                     # USER დებს; git-ში არ ინახება
  Scripts/
    Core/      Chess.Core.asmdef     + *.cs   # წესები (namespace Chess.Core)
    AI/        Chess.AI.asmdef       + *.cs   # IChessAI, UCI, StockfishAI, AIDifficulty.FromElo (namespace Chess.AI)
    Campaign/  Chess.Campaign.asmdef + *.cs   # Stage 2: რეგიონები, პროგრესი, ტექსტები, JSON შენახვა (namespace Chess.Campaign)
      Region.cs, CampaignRegions.cs, CampaignProgress.cs, BattleRules.cs, CampaignText.cs,
      JsonReader.cs (internal), CampaignSaveSerializer.cs, CampaignSaveStore.cs
    Game/      Chess.Game.asmdef     + *.cs   # MonoBehaviour-ები (namespace Chess.Game)
      Session/GameSession.cs                  # Stage 2: static launch პარამეტრები + ნავიგაცია + შენახვა
      UI/MainMenuController.cs, UI/CampaignMapController.cs, UI/RegionButton.cs, UI/BattlePanel.cs   # Stage 2, uGUI
  Editor/
    Chess.Editor.asmdef, ChessSceneSetup.cs   # Tools → Setup Chess Scene
    CampaignScenesSetup.cs                    # Stage 2: Tools → Setup Campaign Scenes
Tests/
  ChessCore.Build/ChessCore.Build.csproj         # netstandard2.1, AssemblyName Chess.Core,     აკომპილირებს Assets/Scripts/Core/**
  ChessAI.Build/ChessAI.Build.csproj             # netstandard2.1, AssemblyName Chess.AI,       აკომპილირებს Assets/Scripts/AI/**
  ChessCampaign.Build/ChessCampaign.Build.csproj # Stage 2: netstandard2.1, AssemblyName Chess.Campaign, Assets/Scripts/Campaign/**, ProjectReference → ChessCore.Build
  ChessCore.Tests/ChessCore.Tests.csproj         # net10.0 xUnit, ProjectReference სამივეზე (ერთადერთი სატესტო პროექტი)
    BuildTargetTests.cs
    Core/*.cs                                    # Core-ის ტესტები
    AI/*.cs                                      # AI-ის ტესტები
    Campaign/*.cs                                # Stage 2: Campaign-ის ტესტები
  GameCompileCheck/GameCompileCheck.csproj       # Game-ის კომპილაცია Unity DLL-ებით (+ UnityEngine.UI.dll, + ChessCampaign.Build Stage 2-დან)
  EditorCompileCheck/EditorCompileCheck.csproj   # Editor-ის კომპილაცია (+ UnityEngine.UI.dll, Unity.InputSystem.dll, ChessCampaign.Build Stage 2-დან)
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
| Campaign: უცნობი region id | `ArgumentException` (`Unknown region id: '<id>'`); null → `ArgumentNullException` |
| Campaign: ბრძოლა Locked რეგიონზე | `InvalidOperationException` (`Region '<id>' is locked.`) — `CampaignProgress.RecordBattle` და `GameSession.StartCampaignBattle` |
| Campaign: არასწორი შენახვის შინაარსი (`CampaignSaveSerializer.Deserialize`) | `CampaignSaveFormatException : FormatException`, შეტყობინება `Invalid campaign save: <reason>`. სხვა ტიპის exception (მაგ. `InvalidCastException`, `StackOverflowException`) არასდროს |
| Campaign: `CampaignSaveStore.Load` | **არასდროს** აგდებს: ფაილი არ არის → `NoSave`; შინაარსის ან კითხვის I/O შეცდომა → `Corrupted` + backup + გაფრთხილება |
| Campaign: `CampaignSaveStore.Save` | `IOException` / `UnauthorizedAccessException` გადის ზემოთ; Game (`GameSession.TrySaveCampaign`) იჭერს → `Debug.LogException` + `Warning: progress could not be saved.` |

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
- en passant: `-`, ან მე-6 რიგის უჯრა (როცა თეთრის სვლაა) / მე-3 რიგის უჯრა (როცა შავის სვლაა). ep უჯრა და მის უკან მდგომი უჯრა ცარიელი უნდა იყოს, ხოლო წინ უნდა იდგეს მოწინააღმდეგის პაიკი, რომელმაც ორმაგი სვლა გააკეთა; სხვა შემთხვევაში `FormatException` (`en passant square '<sq>' is inconsistent with the position`). მომჭრელი პაიკის არსებობა არ მოითხოვება (ep იწერება ყოველი ორმაგი სვლის შემდეგ).
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

**Stage 2 — AI-ის გაფართოება (მხოლოდ დამატება, backward-compatible):**
```csharp
public sealed class AIDifficulty
{
    // ... ყველაფერი ზემოთ ჩამოთვლილი უცვლელია (კონსტრუქტორი, property-ები, FromPreset და preset-ების მნიშვნელობები)
    public const int MinUciElo = 1320;              // Stockfish-ის UCI_Elo მინიმუმი; ქვემოთ Skill Level + depth
    public static AIDifficulty FromElo(int elo);    // 500..3500, სხვა → ArgumentOutOfRangeException; ცხრილი: Business Rules → „Elo → AI პარამეტრები“
}
```
`UciProtocol`, `StockfishAI`, `IChessAI` არ იცვლება: `FromElo`-ს შედეგი ჩვეულებრივი `AIDifficulty`-ია და არსებული `BuildDifficultyCommands` / `BuildGo` მას სწორად აგზავნის (Elo < 1320 → `UCI_LimitStrength false` + `Skill Level`; ≥ 1320 → `UCI_LimitStrength true` + `UCI_Elo`).

### Chess.Campaign (namespace `Chess.Campaign`, Stage 2)
სუფთა C# (netstandard2.1, C# 9, `UnityEngine`-ის, NuGet-ის, `record`/`init`-ის გარეშე), asmdef `Chess.Campaign` (`noEngineReferences: true`, `references: ["Chess.Core"]`). Core-დან იყენებს მხოლოდ `GameResult`-ს და `Side`-ს.
```csharp
public sealed class Region
{
    public Region(string id, string name, string era, int elo);
    // id: non-empty, only 'a'..'z'; name, era: non-empty → else ArgumentException (null → ArgumentNullException)
    // elo: 500..3500 → else ArgumentOutOfRangeException
    public string Id { get; }
    public string Name { get; }
    public string Era { get; }
    public int Elo { get; }
}

public static class CampaignRegions
{
    public static IReadOnlyList<Region> All { get; }          // 5 regions, campaign order (Business Rules table)
    public static Region Get(string id);                      // null → ArgumentNullException; unknown → ArgumentException("Unknown region id: '<id>'")
    public static bool TryGet(string id, out Region region);  // false for null / unknown
    public static int IndexOf(string id);                     // -1 for null / unknown
}

public enum RegionState { Locked, Available, Liberated }

public sealed class CampaignProgress
{
    public static CampaignProgress CreateNew();               // nothing liberated
    public static CampaignProgress FromLiberated(IEnumerable<string> liberatedRegionIds);
    // null collection/element → ArgumentNullException; unknown or duplicate id → ArgumentException
    public IReadOnlyList<string> LiberatedRegionIds { get; }  // always in campaign order
    public int LiberatedCount { get; }
    public bool IsComplete { get; }                           // all regions liberated
    public RegionState GetState(string regionId);             // rule from Business Rules; unknown → ArgumentException
    public BattleSummary RecordBattle(string regionId, BattleOutcome outcome, bool counted);
    // the ONLY mutating member. unknown → ArgumentException; Locked → InvalidOperationException("Region '<id>' is locked.")
    // texts and flags exactly as in Business Rules → RecordBattle table
}

public enum BattleOutcome { Victory, Defeat, Draw }

public static class BattleRules
{
    public const string NotCountedTitle = "Game over";
    public static BattleOutcome GetOutcome(GameResult result, Side humanSide);
    // WhiteWins/BlackWins vs humanSide → Victory/Defeat; Draw → Draw; None → ArgumentException
    public static string GetTitle(BattleOutcome outcome);     // "Victory!" | "Defeat" | "Draw"
}

public sealed class BattleSummary
{
    public BattleSummary(string title, string message, bool canRetry, bool progressChanged);
    public string Title { get; }
    public string Message { get; }
    public bool CanRetry { get; }          // show Retry button
    public bool ProgressChanged { get; }   // caller must save
}

public static class CampaignText
{
    public const string SaveFailedWarning = "Warning: progress could not be saved.";
    public const string CorruptedSaveWarning =
        "The saved campaign was damaged and could not be loaded. It was kept as campaign.corrupt.json. Start a new campaign.";
    public const string NotCountedMessage = "The AI opponent was not active, so this result does not count.";
    public static string StateName(RegionState state);                  // "Locked" | "Available" | "Liberated"
    public static string RegionLabel(Region region, RegionState state); // "{Name}\n{Era}\nElo {Elo}\n{StateName}"
    public static string MapStatus(CampaignProgress progress);
    // not complete: "Liberated {n} of {total}. Choose a region to fight for."
    // complete:     "All regions are liberated! Georgia is free."
    public static string BattleInfo(Region region);                     // "{Name} - {Era} - enemy Elo {Elo}"
}

public sealed class CampaignSaveFormatException : FormatException
{
    public CampaignSaveFormatException(string reason);                  // Message = "Invalid campaign save: <reason>"
    public CampaignSaveFormatException(string reason, Exception inner);
}

public static class CampaignSaveSerializer
{
    public const int CurrentVersion = 1;
    public static string Serialize(CampaignProgress progress);   // null → ArgumentNullException; exact compact output, see Data
    public static CampaignProgress Deserialize(string json);     // null → ArgumentNullException; anything invalid → CampaignSaveFormatException
}
// internal static class JsonReader — minimal RFC 8259 reader: object, array, string (escapes \" \\ \/ \b \f \n \r \t \uXXXX;
// raw control chars < 0x20 rejected), number, true/false/null, whitespace. Max nesting depth 64 (iterative or depth-checked,
// never StackOverflow). Integer literals → long, others → double. Every error → CampaignSaveFormatException.

public enum CampaignLoadStatus { NoSave, Loaded, Corrupted }

public sealed class CampaignLoadResult
{
    // constructor internal
    public CampaignLoadStatus Status { get; }
    public CampaignProgress Progress { get; }   // never null; CreateNew() for NoSave / Corrupted
    public string Warning { get; }              // CampaignText.CorruptedSaveWarning when Corrupted, otherwise null
}

public sealed class CampaignSaveStore
{
    public const string FileName = "campaign.json";
    public const string TempFileName = "campaign.json.tmp";
    public const string BackupFileName = "campaign.corrupt.json";
    public CampaignSaveStore(string directory);   // null/whitespace → ArgumentException (null → ArgumentNullException)
    public string Directory { get; }
    public string FilePath { get; }               // Path.Combine(Directory, FileName)
    public string BackupPath { get; }             // Path.Combine(Directory, BackupFileName)
    public CampaignLoadResult Load();             // never throws (see Error model, Business Rules → შენახვა)
    public void Save(CampaignProgress progress);  // creates Directory if missing; UTF-8 without BOM; atomic (tmp + Replace/Move)
}
```

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
| `GameController` | `ChessGame` | კომპოზიცია და თამაშის ნაკადი: GameState, სვლები, AI-ის რიგი, input-ის დაბლოკვა, R = restart, შეცდომა → hot-seat, `OnDisable`/`OnDestroy`-ში AI-ის Dispose (domain reload-ზე OnDestroy არ იძახება; reload-ის შემდეგ — hot-seat) | `boardView`, `piecesView`, `selection`, `cameraRig`, `hud`, `humanSide` (Side, White), `playAgainstAI` (bool, true), `difficulty` (DifficultyPreset, Medium), `stockfishRelativePath` (string, `Stockfish/stockfish.exe`, StreamingAssets-ის მიმართ), `stockfishPathOverride` (string, ცარიელი = გამოიყენება relative), `startFen` (string, ცარიელი = საწყისი პოზიცია; Stage 2-დან მხოლოდ Mode None). Stage 2: + `battlePanel` (BattlePanel, null = პანელის გარეშე), + `debugCampaignStartFen` (string, ცარიელი; მხოლოდ Editor, მხოლოდ Campaign); ნაკადი — „Stage 2 — GameController-ის დამატებები“ |

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

### Chess.Game — Stage 2 (სცენები, სესია, uGUI)
**სცენები და Build Settings (რიგი სავალდებულოა):** 0 `Assets/Scenes/MainMenu.unity`, 1 `Assets/Scenes/CampaignMap.unity`, 2 `Assets/Scenes/MainScene.unity`. სიას მხოლოდ **Tools → Setup Campaign Scenes** წერს (USER-ის გაშვებით); აგენტი `ProjectSettings/`-ს არ ეხება.

**`GameSession` (`Assets/Scripts/Game/Session/GameSession.cs`):**
```csharp
public enum LaunchMode { None, Campaign, QuickGame }

public static class SceneNames
{
    public const string MainMenu = "MainMenu";
    public const string CampaignMap = "CampaignMap";
    public const string Battle = "MainScene";
}

public static class GameSession
{
    public static LaunchMode Mode { get; }                 // None = MainScene played directly (Stage 1 behavior)
    public static string RegionId { get; }                 // Campaign only, otherwise null
    public static Side HumanSide { get; }                  // Campaign: White; QuickGame: chosen
    public static DifficultyPreset QuickDifficulty { get; }// QuickGame only
    public static CampaignProgress Campaign { get; }       // null = no valid campaign in this Play session
    public static string LoadWarning { get; }              // CampaignText.CorruptedSaveWarning after a Corrupted load, otherwise null
    public static bool LastSaveFailed { get; }             // last TrySaveCampaign failed
    public static CampaignSaveStore Store { get; }         // lazy: new CampaignSaveStore(Application.persistentDataPath)

    public static void EnsureLoaded();
    // first call per Play session only: Debug.Log("[GameSession] Campaign save file: " + Store.FilePath);
    // r = Store.Load(); Loaded → Campaign = r.Progress; Corrupted → LoadWarning = r.Warning (Campaign stays null); NoSave → nothing
    public static void StartNewCampaign();                 // Campaign = CampaignProgress.CreateNew(); LoadWarning = null; TrySaveCampaign()
    public static bool TrySaveCampaign();                  // Campaign null → InvalidOperationException; Store.Save(Campaign);
                                                           // IOException / UnauthorizedAccessException → Debug.LogException, LastSaveFailed = true, false
    public static void StartCampaignBattle(string regionId);
    // Campaign null or region Locked → InvalidOperationException; Mode = Campaign, RegionId = id, HumanSide = White; LoadScene(Battle)
    public static void StartQuickGame(Side humanSide, DifficultyPreset difficulty); // Mode = QuickGame; LoadScene(Battle)
    public static void GoToMainMenu();                     // Mode = None, RegionId = null; LoadScene(MainMenu)
    public static void GoToCampaignMap();                  // Mode = None, RegionId = null; LoadScene(CampaignMap)
}
```
- ყველა static ველი ნულდება `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` მეთოდში. **აუცილებელია:** პროექტში Enter Play Mode Options ჩართულია domain reload-ის გარეშე (`ProjectSettings/EditorSettings.asset`: `m_EnterPlayModeOptions: 1`), ამიტომ static-ები Play-დან Play-მდე რჩება — reset-ის გარეშე MainScene-ის პირდაპირი Play წინა სესიის კამპანიის რეჟიმით ჩაირთვებოდა.
- სცენის ჩატვირთვა: `SceneManager.LoadScene(name)` (sync, `LoadSceneMode.Single`). DontDestroyOnLoad ობიექტი არ არის საჭირო.

**Components (uGUI; Inspector ველები `[SerializeField] private`, სახელები სავალდებულოა — Editor სკრიპტი მათ `SerializedObject.FindProperty`-ით ავსებს):**

| Component (ფაილი) | სად დევს | პასუხისმგებლობა | Inspector fields / public API |
|---|---|---|---|
| `MainMenuController` (`UI/MainMenuController.cs`) | `MainMenuCanvas` (MainMenu) | მენიუ, დადასტურება, Quick Game-ის არჩევანი | `mainPanel`, `confirmPanel`, `quickGamePanel` (GameObject); `newCampaignButton`, `continueButton`, `quickGameButton`, `quitButton`, `confirmYesButton`, `confirmNoButton`, `colorButton`, `difficultyButton`, `startQuickGameButton`, `backFromQuickGameButton` (Button); `warningText`, `colorLabel`, `difficultyLabel` (Text) |
| `RegionButton` (`UI/RegionButton.cs`) | თითო `Region_<id>` ღილაკზე (CampaignMap) | ერთი რეგიონის ჩვენება | `regionId` (string), `button` (Button), `label` (Text), `background` (Image). API: `string RegionId { get; }`, `void Show(Region region, RegionState state)`, `event Action<string> Clicked` |
| `CampaignMapController` (`UI/CampaignMapController.cs`) | `CampaignMapCanvas` (CampaignMap) | რუკის შევსება, ბრძოლის დაწყება | `regionButtons` (RegionButton[], 5, კამპანიის რიგით), `statusText` (Text), `backButton` (Button) |
| `BattlePanel` (`UI/BattlePanel.cs`) | `BattleCanvas` (MainScene) | ინფო ხაზი, გასვლის ღილაკი, შედეგის პანელი (მხოლოდ view) | `infoText` (Text), `leaveButton` (Button), `leaveLabel` (Text), `resultRoot` (GameObject), `titleText`, `messageText` (Text), `retryButton` (Button), `retryLabel` (Text), `backButton` (Button), `backLabel` (Text). API: `event Action RetryClicked`, `event Action BackClicked` (leaveButton-იც მას იძახებს), `ShowInGame(string info, string leaveLabel)`, `ShowResult(string title, string message, bool showRetry, string retryLabel, string backLabel)` (გასვლის ღილაკს მალავს), `HideResult()` (გასვლის ღილაკი ბრუნდება, თუ `ShowInGame` გამოძახებული იყო), `HideAll()`. `Awake`: ყველაფერი დამალულია, `onClick.AddListener` კოდით (persistent listener-ები არ გამოიყენება) |

**UI ტექსტები (ზუსტად, ინგლისურად):**
- MainMenu: სათაური `Georgian Battle Chess`; ღილაკები `New Campaign`, `Continue`, `Quick Game`, `Quit`; დადასტურება `Start a new campaign? Current progress will be lost.` + `Start new` / `Cancel`; Quick Game პანელი: სათაური `Quick Game`, `Color: White` / `Color: Black`, `Difficulty: Easy` / `Difficulty: Medium` / `Difficulty: Hard` / `Difficulty: Master`, `Start`, `Back`; `warningText` = `GameSession.LoadWarning` (როცა `Campaign == null`) და/ან `CampaignText.SaveFailedWarning` (როცა `LastSaveFailed`), ხაზით გამოყოფილი; სხვა შემთხვევაში ცარიელი.
- CampaignMap: სათაური `Campaign Map`; ღილაკის ტექსტი `CampaignText.RegionLabel`; `statusText` = `CampaignText.MapStatus(progress)` + (თუ `LastSaveFailed`) `"\n" + CampaignText.SaveFailedWarning`; `Back to menu`.
- MainScene (Campaign): `infoText` = `CampaignText.BattleInfo(region)`; გასვლა `Back to map`; შედეგი — `BattleSummary`, ღილაკები `Retry` / `Back to map`.
- MainScene (QuickGame): `infoText` = `Quick Game - {Easy|Medium|Hard|Master} - you play {White|Black}`; გასვლა `Back to menu`; შედეგი — Title `BattleRules.GetTitle(outcome)`, Message = სტატუსის ტექსტი; ღილაკები `Play again` / `Back to menu`.
- Quit: `Debug.Log("[MainMenu] Quit requested")` + `Application.Quit()` (Editor-ში მხოლოდ log).

**რეგიონის ღილაკის ფერი (`background.color`):** Locked `#5A5A5A` (`button.interactable = false`), Available `#C8962D`, Liberated `#3C8C3C`. ტექსტი თეთრი.

**UI სტილი (Stage 2, DESIGN.md-ის ნაცვლად; ყველაფერს `CampaignScenesSetup` აწყობს):**
- Canvas: Screen Space - Overlay; `CanvasScaler` Scale With Screen Size, reference 1920×1080, match 0.5. ყველა ზომა ქვემოთ reference პიქსელებშია.
- ფერები:
  - ფონი (კამერა, MainMenu/CampaignMap): `#1C1C22`;
  - პანელი (Confirm, QuickGame, Result): `#2A2A33`, alpha 0.95;
  - რუკის არე (`MapArea`): `#2F3A2A`;
  - ჩვეულებრივი ღილაკი: `#3A4A6B`; ღილაკის `ColorTint`: highlighted ×1.15, pressed ×0.85, disabled `#5A5A5A` alpha 0.5;
  - ტექსტი `#FFFFFF`; `warningText` `#FFB347`; რეგიონის ღილაკები — ზემოთ.
- ზომები:
  - მენიუს ღილაკი 400×80; ვერტიკალური სვეტი ცენტრში, დაშორება 20;
  - რეგიონის ღილაკი 400×190; label 4-ხაზიანია (`CampaignText.RegionLabel`: სახელი, ეპოქა, Elo, მდგომარეობა), `fontStyle` Bold, `resizeTextForBestFit` = true, min 22, max 28, `verticalOverflow` Truncate, 8px inset;
  - CampaignMap: `MapArea` 1500×660, center (0, -50) (y -380..280); `TitleText` ზედა კიდიდან 70 (center), სიმაღლე 90 და იქმნება `MapArea`-ს **შემდეგ** (ხატვის რიგით ზემოთაა); `StatusText` ქვედა კიდიდან 100, `BackButton` ქვედა-მარცხნივ — არცერთი არ ფარავს `MapArea`-ს;
  - BattleCanvas-ის ღილაკი (`LeaveButton`, `RetryButton`, `BackButton`) 260×70; `LeaveButton` ზედა-მარჯვნივ, დაშორება კიდიდან 24;
  - პანელები: Confirm 800×300, QuickGame 600×520, Result 700×360 — ცენტრში.
- ტექსტის ზომა (font size): სათაური 64; პანელის სათაური 40; ღილაკი 32; რეგიონის label best fit 22–28 (Bold); info/status/warning/message 28. ყველა ტექსტი LegacyRuntime.ttf, `alignment` MiddleCenter (`InfoText` UpperCenter), `horizontalOverflow` Wrap.

**MainMenuController-ის ნაკადი:** `Start` → `GameSession.EnsureLoaded()` → `continueButton` ჩანს (`gameObject.SetActive`) მხოლოდ თუ `Campaign != null`, სხვა შემთხვევაში დამალულია; ჩანს `mainPanel`, დამალულია `confirmPanel`, `quickGamePanel`; Quick Game-ის ნაგულისხმევი: White, Medium.
- `New Campaign`: `Campaign != null` → `confirmPanel` (`mainPanel` იმალება); სხვა შემთხვევაში `StartNewCampaign()` → `GoToCampaignMap()`. `Start new` → იგივე. `Cancel` → `mainPanel`.
- `Continue` → `GoToCampaignMap()`. `Quick Game` → `quickGamePanel`; `colorButton` ცვლის White↔Black; `difficultyButton` ციკლავს; `Start` → `StartQuickGame(side, preset)`; `Back` → `mainPanel`.

**CampaignMapController-ის ნაკადი:** `Start` → `EnsureLoaded()`; თუ `Campaign == null` (მაგ. CampaignMap პირდაპირ Play-ით) → `StartNewCampaign()` → ყოველი `RegionButton.Show(CampaignRegions.Get(id), Campaign.GetState(id))` → `Clicked` → `GameSession.StartCampaignBattle(id)`. უცნობი `regionId` ღილაკზე → `Debug.LogError` + ღილაკი გათიშულია. `backButton` → `GoToMainMenu()`.

**Stage 2 — GameController-ის დამატებები (Stage 1-ის ნაკადი უცვლელია):**
- `Start`-ში, `CreateAi`-მდე — launch პარამეტრები:
  - `Mode == Campaign`: `region = CampaignRegions.Get(GameSession.RegionId)`; `humanSide = White`; `playAgainstAI = true`; AI-ის სირთულე = `AIDifficulty.FromElo(region.Elo)`; `battlePanel.ShowInGame(CampaignText.BattleInfo(region), "Back to map")`.
  - `Mode == QuickGame`: `humanSide = GameSession.HumanSide`; `playAgainstAI = true`; სირთულე = `AIDifficulty.FromPreset(GameSession.QuickDifficulty)`; `ShowInGame("Quick Game - ...", "Back to menu")`.
  - `Mode == None`: inspector-ის მნიშვნელობები (Stage 1); `battlePanel?.HideAll()`.
  - `startFen` (თუ ცარიელი არ არის) მოქმედებს მხოლოდ `Mode == None`-ში. Campaign: Editor-ში (`#if UNITY_EDITOR`) `debugCampaignStartFen` — თუ ცარიელი არ არის, ბრძოლა ამ FEN-ით იწყება (ყოველ restart/Retry-ზეც), `Debug.LogWarning("[GameController] DEBUG FEN active: <fen>")`, HUD-ის სტატუსს ბოლოში ემატება `  [DEBUG FEN]` (მხოლოდ `UpdateHud`-ში, არა შედეგის ტექსტში); არასწორი FEN → `Debug.LogError` + საწყისი პოზიცია (როგორც startFen). შედეგი ჩვეულებრივ ითვლება. build-ში ეს ლოგიკა არ არსებობს (ველი სერიალიზდება, რომ serialization layout Editor-სა და build-ში ერთი იყოს). QuickGame: ყოველთვის საწყისი პოზიცია.
- `CreateAi` იყენებს launch სირთულეს, თუ ის არსებობს, სხვა შემთხვევაში `AIDifficulty.FromPreset(difficulty)`, და წერს `Debug.Log("[GameController] AI difficulty: skill {SkillLevel}, depth {Depth|-}, movetime {MoveTimeMs} ms, UCI_Elo {Elo|off}")`.
- ყოველი წარმატებული `TryMakeMove`-ის შემდეგ (ადამიანის და AI-ის): თუ `_state.IsGameOver` და შედეგი ამ პარტიაში ჯერ არ დამუშავებულა → `HandleGameOver()` (flag ნულდება `ResetGame`-ში, სადაც ასევე `battlePanel?.HideResult()`).
- `HandleGameOver`: `Mode == None` → არაფერი. `counted = playAgainstAI && _ai != null`; `outcome = BattleRules.GetOutcome(_state.Result, humanSide)`.
  - Campaign: `summary = GameSession.Campaign.RecordBattle(RegionId, outcome, counted)`; თუ `summary.ProgressChanged` და `!GameSession.TrySaveCampaign()` → message += `"\n" + CampaignText.SaveFailedWarning`; `battlePanel.ShowResult(summary.Title, message, summary.CanRetry, "Retry", "Back to map")`.
  - QuickGame: `ShowResult(BattleRules.GetTitle(outcome), <status text>, true, "Play again", "Back to menu")`.
  - `battlePanel == null` → პროგრესი მაინც იწერება, პანელი არ ჩანს.
- `battlePanel.RetryClicked` → იგივე, რაც R (`Restart`); `BackClicked` → Campaign: `GameSession.GoToCampaignMap()`, QuickGame: `GameSession.GoToMainMenu()`. გამოწერა `OnEnable`-ში, გაუქმება `OnDisable`-ში (როგორც `selection.MoveChosen`). სცენის დატოვებისას AI-ის Dispose არსებული `OnDisable`/`OnDestroy`-ით ხდება.

**Editor: `Assets/Editor/CampaignScenesSetup.cs`** (`Chess.Editor.CampaignScenesSetup`, `[MenuItem("Tools/Setup Campaign Scenes")]`, log prefix `[CampaignScenesSetup] `):
1. Play-ის დროს → `Debug.LogError` და გასვლა; `EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()` (უარზე — გაუქმება).
2. ყოველი სცენა: თუ ფაილი არ არსებობს → `EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)` + `SaveScene(scene, path)`; სხვა შემთხვევაში `OpenScene(path, Single)`. `.meta`-ს Unity ქმნის.
3. MainMenu და CampaignMap: `Main Camera` (Camera, Clear Flags = Solid Color `#1C1C22`, tag MainCamera, AudioListener) — თუ არ არის; `EventSystem` (EventSystem + `InputSystemUIInputModule` + `AssignDefaultActions()`; `StandaloneInputModule` იშლება, თუ არის) — ერთი.
4. გენერირებული UI root (`MainMenuCanvas`, `CampaignMapCanvas`, `BattleCanvas`) ყოველ გაშვებაზე **იშლება და თავიდან იქმნება** (ხელით ცვლილებები ამ ობიექტებში იკარგება). Canvas: Screen Space - Overlay, `CanvasScaler` Scale With Screen Size 1920×1080, match 0.5, `GraphicRaycaster`. ტექსტი: `UnityEngine.UI.Text`, font `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`, ცენტრში, თეთრი. ობიექტების სახელები = Inspector ველის სახელები PascalCase-ით (მაგ. `NewCampaignButton`, `ConfirmPanel`, `Region_kartli`). `ConfirmPanel`, `QuickGamePanel`, `ResultPanel` იქმნება inactive.
5. CampaignMap: `MapArea` (Image `#2F3A2A`) და მასში 5 `Region_<id>` ღილაკი (400×190, label Bold best fit 22–28 — იხ. „UI სტილი“) anchor-ებით Business Rules-ის ცხრილიდან (`CampaignRegions.All`-ის რიგით), თითოზე `RegionButton` (`regionId`, `button`, `label`, `background`); `regionButtons` მასივი იმავე რიგით.
6. MainScene: root `ChessGame` + `GameController` უნდა არსებობდეს; თუ არა → `Debug.LogError("[CampaignScenesSetup] ChessGame not found in MainScene. Run Tools > Setup Chess Scene first.")`, MainScene-ის ნაწილი გამოტოვდება (დანარჩენი გრძელდება, შედეგი „WITH ERRORS“). სხვა შემთხვევაში: `EventSystem` (როგორც 3) + `BattleCanvas` (sortingOrder 10; `InfoText` ზედა ცენტრში, `LeaveButton` ზედა-მარჯვნივ, `ResultPanel` ცენტრში: `TitleText`, `MessageText`, `RetryButton`, `BackButton`) + `GameController.battlePanel` მიბმა `SerializedObject`-ით. GameController-ის სხვა ველებს არ ეხება.
7. ყოველი ველი `SerializedObject.FindProperty`-ით; ვერ ნაპოვნი → `Debug.LogError` სახელით (როგორც `ChessSceneSetup`).
8. `EditorBuildSettings.scenes` = ზუსტად `[MainMenu, CampaignMap, MainScene]`, სამივე enabled.
9. სცენები ინახება; ბოლოს იხსნება MainMenu; ერთი `Debug.Log("[CampaignScenesSetup] Done[ WITH ERRORS (see above)].\nCreated: ...;\nUpdated: ...;\nBuild scenes: MainMenu, CampaignMap, MainScene")`.
10. მეორედ გაშვება დუბლიკატებს არ ქმნის: თითო სცენაში ერთი Main Camera, ერთი EventSystem, ერთი გენერირებული Canvas.

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
- **Campaign (Stage 2, კრიტიკული ლოგიკა — პროგრესი და შენახვა):** xUnit `Tests/ChessCore.Tests/Campaign/*.cs`: რეგიონების ცხრილი, მდგომარეობის წესი, `RecordBattle`-ის ყველა სტრიქონი ზუსტი ტექსტით, `GetOutcome` ცხრილი, serializer round-trip (ყველა 32 ქვესიმრავლე) და დაზიანებული შეყვანის Theory (მათ შორის ღრმა ჩადგმულობა StackOverflow-ის გარეშე), `CampaignSaveStore` დროებით საქაღალდეში (თითო ტესტს თავისი `Path.GetTempPath()/chess3d-tests/<guid>`, ბოლოს იშლება). `BuildTargetTests` ამოწმებს `Chess.Campaign`-ის netstandard2.1-ს და რომ ის მხოლოდ `Chess.Core`-ს (და `netstandard`-ს) მიმართავს.
- **AI (Stage 2):** `FromElo`-ს ზოლების საზღვრები და preset-ებთან ტოლობა; `BuildDifficultyCommands`/`BuildGo` `FromElo`-ს შედეგზე. არსებული ტესტები უცვლელად გადის.
- **Game/Editor (Stage 2):** `dotnet build Tests/GameCompileCheck` და `Tests/EditorCompileCheck` (0 შეცდომა, 0 warning) + USER task-ები (Play-mode).

## Realistic Scale
ერთი მოთამაშე, ერთი პარტია ერთდროულად, ერთი Stockfish პროცესი. წარმადობის მოთხოვნა: perft ტესტები ჯამში დაახლოებით 30 წამამდე, AI-ის სვლა 1–2 წამამდე.

## Security & Privacy
- პერსონალური მონაცემები, ქსელი და ავტორიზაცია არ არის.
- გარე პროცესის გაშვება: მხოლოდ `ProcessUciTransport`, `UseShellExecute = false`, არგუმენტების და shell-ის გარეშე. გზა მოდის inspector-იდან ან StreamingAssets-იდან და არა მომხმარებლის runtime input-იდან. Play-ის დასრულებისას პროცესი აუცილებლად კვდება (`OnDestroy` → Dispose → Kill), რომ ობოლი `stockfish.exe` არ დარჩეს.
- Stockfish GPLv3-ია. binary git-ში არ ინახება. build-ის გავრცელებამდე საჭიროა ლიცენზიის გადაწყვეტილება (Backlog).
- Stage 2 შენახვა: `campaign.json` შეიცავს მხოლოდ რეგიონების id-ებს (პერსონალური მონაცემები არ არის). ფაილი გარე შეყვანად ითვლება: reader არ იყენებს reflection-ს/ტიპების აღდგენას, ზღუდავს ჩადგმულობას (64) და ყოველ შეცდომას `CampaignSaveFormatException`-ად აქცევს; ფაილის გზა ფიქსირებულია (`persistentDataPath`), მომხმარებლის შეყვანიდან არ მოდის.

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

### 2026-10-25 — სცენის ავტომატური აწყობა Editor-ის სკრიპტით
- Decision: `Assets/Editor/ChessSceneSetup.cs` (asmdef `Chess.Editor`, მხოლოდ Editor) — მენიუ **Tools → Setup Chess Scene** ქმნის/აახლებს მასალებს, Main Camera-ს CameraRig-ს და `ChessGame`-ს ყველა კომპონენტით და კავშირით, idempotent-ად. ველები `SerializedObject.FindProperty`-ით ივსება (Game-ის `[SerializeField] private` ველები უცვლელია). Editor კოდის კომპილაცია მოწმდება `dotnet build Tests/EditorCompileCheck`-ით. USER task-ებში 10/12/14/16 აწყობის ნაბიჯები ჩანაცვლდა ამ ბრძანებით; Play-ის შემოწმებები უცვლელია.
- Reason: მომხმარებლის მოთხოვნა — ხელით აწყობა დამწყებისთვის შეცდომების წყაროა.
- Note: ბრძანება GameController-ს აყენებს Play Against AI ✓-ზე, ამიტომ task 12-ის hot-seat შემოწმებას დაემატა ერთი ნაბიჯი: შემოწმების დროს Play Against AI მოხსნა და ბოლოს დაბრუნება (თვითონ შემოწმებები უცვლელია).
- How to change: მნიშვნელობები `ChessSceneSetup.cs`-ში; ხელახლა გაშვება ხელით შეცვლილ ველებს (playAgainstAI, difficulty, humanSide, stockfish path, startFen) ნაგულისხმევზე აბრუნებს.

### 2026-10-25 — Stage 2 = Campaign; Tier უცვლელია
- Decision: Stage 2 = მენიუ + კამპანიის რუკა + რეგიონის Elo + შენახვა + Quick Game (მომხმარებლის მოთხოვნა). Tier 1 რჩება: ერთი ლოკალური ფაილი, ქსელი/ბაზა არ არის.
- Reason: შენახვა ერთი მოთამაშის ~50 ბაიტიანი პროგრესია; ბაზა ან SQLite არაფერს მატებს.
- How to change: რამდენიმე profile/slot ან cloud save → ახალი ეტაპის მოთხოვნით.

### 2026-10-25 — Chess.Campaign: ცალკე assembly, დამოკიდებულია მხოლოდ Core-ზე
- Decision: `Chess.Campaign` (noEngineReferences, → Chess.Core). Elo → AI პარამეტრები AI-შია (`AIDifficulty.FromElo`), არა Campaign-ში. Game აერთებს ორივეს. მიმართულება: Core ← AI ← Game, Core ← Campaign ← Game; AI და Campaign ერთმანეთს არ იცნობენ.
- Reason: Campaign = პროგრესის წესები და შენახვა, მას Stockfish-ის ცოდნა არ სჭირდება; Elo → UCI პარამეტრები Stockfish-ის სპეციფიკაა (1320 მინიმუმი) და AI-ს ეკუთვნის. Core-ზე დამოკიდებულება საჭიროა მხოლოდ `GameResult` + `Side` → `BattleOutcome`-ისთვის, რომ ეს წესი dotnet-ით დაიტესტოს (Game ვერ იტესტება). `BuildTargetTests` ამოწმებს, რომ Campaign AI-ს/Unity-ს არ მიმართავს.
- Alternatives considered: Campaign → AI → Core (Campaign აბრუნებს `AIDifficulty`-ს); Campaign კოდი Game-ში.
- Why rejected: პირველი ზედმეტ კავშირს ქმნის კამპანიასა და engine-ს შორის; მეორე კრიტიკულ ლოგიკას (პროგრესი, შენახვა) ტესტების გარეშე ტოვებს.

### 2026-10-25 — JSON: საკუთარი მინიმალური reader/writer Campaign-ში
- Decision: `CampaignSaveSerializer` წერს ფიქსირებულ compact JSON-ს ხელით, ხოლო კითხულობს internal `JsonReader`-ით (ზოგადი RFC 8259 ქვესიმრავლე, depth ≤ 64), შემდეგ schema-ს ამოწმებს. ფაილის I/O ასევე Campaign-შია (`CampaignSaveStore`, `System.IO`), Game მხოლოდ საქაღალდეს გადასცემს.
- Reason: netstandard2.1-ს `System.Text.Json` არ აქვს, NuGet Core/AI/Campaign-ში აკრძალულია. `JsonUtility` მხოლოდ Unity-შია, ე.ი. დაზიანებული ფაილის ქცევა dotnet-ით ვერ დაიტესტებოდა, და არასწორ ველებს ჩუმად default-ით ავსებს. schema ერთი რიცხვი და სტრიქონების მასივია — reader ~150 ხაზია.
- Alternatives considered: `JsonUtility` + DTO Game-ში; `System.Text.Json` NuGet-ით; არა-JSON ფორმატი.
- Why rejected: ტესტირებადობა / NuGet-ის აკრძალვა / მომხმარებელმა JSON მოითხოვა.
- How to change: schema-ს გაზრდისას (Stage 3+) — `version` 2 და მიგრაცია `Deserialize`-ში.

### 2026-10-25 — შენახვის ფორმატი: მხოლოდ განთავისუფლებული id-ები, version 1
- Decision: `{"version":1,"liberated":[...]}`. Locked/Available არ ინახება — გამოითვლება წესით. id-ები (არა ინდექსები/რაოდენობა). უცნობი top-level გასაღებები იგნორირდება; სხვა ყველაფერი მკაცრად მოწმდება.
- Reason: შეუძლებელია შეუთავსებელი მდგომარეობის შენახვა (მაგ. „Locked, მაგრამ Liberated“); id-ები გაუძლებს რეგიონების რიგის შეცვლას/ჩამატებას.
- Alternatives considered: `liberatedCount`; ყველა რეგიონის state-ის შენახვა.
- Why rejected: რაოდენობა რიგის შეცვლისას ტყუის; state-ების შენახვა წინააღმდეგობრივ მონაცემებს უშვებს.

### 2026-10-25 — დაზიანებული შენახვა: backup + ახალი კამპანია + გაფრთხილება
- Decision: ნებისმიერი შინაარსის ან კითხვის I/O შეცდომა → `campaign.corrupt.json` (გადაწერით) → ცარიელი პროგრესი, `Continue` დამალულია, მენიუში ზუსტი გაფრთხილება (`CampaignText.CorruptedSaveWarning`). `Load` არასდროს აგდებს. ჩაწერა ატომურია (tmp + `File.Replace`/`Move`).
- Reason: მომხმარებლის მოთხოვნა („თამაში არ უნდა გაფუჭდეს“); backup იაფია და ხელით აღდგენის საშუალებას იძლევა; ატომური ჩაწერა თავიდან იცილებს გაწყვეტილ ფაილს crash-ისას.
- Assumption: კითხვის I/O შეცდომაც (მაგ. ფაილი დაბლოკილია) დაზიანებად ითვლება — უმარტივესი და იშვიათი შემთხვევაა.
- Risk: `File.Replace` Unity-ის Mono-ზე dotnet ტესტებით არ მოწმდება → fallback `Delete + Move` `PlatformNotSupportedException`-ზე; რეალური ქცევა მოწმდება USER task-ში (მეორე ჩაწერა არსებულ ფაილზე).

### 2026-10-25 — Elo → AI: `AIDifficulty.FromElo`, ექვსი ზოლი
- Decision: Elo < 1320: Skill 0/3/6 + depth 1/2/4 (500–999 / 1000–1199 / 1200–1319); Elo ≥ 1320: Skill 20 + UCI_LimitStrength + UCI_Elo = elo, movetime 300/500/1000 (1320–1599 / 1600–1999 / ≥ 2000). `FromElo(800/1400/1800/2200)` = Easy/Medium/Hard/Master.
- Reason: Stockfish-ის UCI_Elo მინიმუმი 1320-ია; ქვემოთ სისუსტე მხოლოდ სიღრმით და Skill Level-ით მიიღწევა. preset-ებთან თანხვედრა ერთ შკალას ქმნის და ტესტით მოწმდება. API მხოლოდ ემატება — არსებული კოდი/ტესტები უცვლელია.
- Assumption: Elo-ები მიახლოებითია და გაზომილი არ არის (ბალანსი — Backlog).
- How to change: ზოლების ცხრილი `FromElo`-ში + ტესტები.

### 2026-10-25 — რეგიონების რიგი, Elo და ეპოქები
- Decision: Kartli 800 → Kakheti 1100 → Imereti 1400 → Svaneti 1800 → Khevsureti 2200 (მომხმარებლის ჩამოთვლის რიგი). ეპოქის ტექსტები placeholder-ია (კონცეფციიდან: კახეთი XVI ს., ბოლოს დავით აღმაშენებლის / ერეკლე II-ის ეპოქა).
- Reason: მომხმარებლის რიგი და დიაპაზონი ~800 → ~2200+; ორი რეგიონი 1320-ზე დაბლაა, რომ ორივე AI რეჟიმი გამოიყენებოდეს.
- Assumption: ეპოქები ქრონოლოგიურად არ არის დალაგებული — ეს კონტენტის გადაწყვეტილებაა და მხოლოდ `CampaignRegions`-ის ცხრილს ცვლის (შენახვაზე გავლენა არ აქვს, რადგან id-ები ინახება).

### 2026-10-25 — კამპანიაში ადამიანი თეთრია; შედეგი ითვლება მხოლოდ აქტიური AI-ით
- Decision: კამპანიის ბრძოლაში `humanSide = White`. გამარჯვება ითვლება მხოლოდ მაშინ, თუ დასრულებისას AI აქტიურია (`playAgainstAI && _ai != null`).
- Reason: ფერის არჩევა მოთხოვნაში მხოლოდ Quick Game-ისთვისაა. Stockfish-ის გარეშე თამაში hot-seat-ზე გადადის და მოთამაშეს შეუძლია თავად მოიგოს — ეს რეგიონს არ უნდა ათავისუფლებდეს.
- How to change: ფერის არჩევა რუკაზე → `GameSession.StartCampaignBattle`-ს პარამეტრი.

### 2026-10-25 — სცენებს შორის: static `GameSession` + static-ების reset
- Decision: static კლასი (DontDestroyOnLoad ობიექტის გარეშე) launch პარამეტრებით, კამპანიის პროგრესით და ნავიგაციით; ყველა static ნულდება `RuntimeInitializeOnLoadMethod(SubsystemRegistration)`-ში.
- Reason: სცენებს შორის 3–4 მნიშვნელობა გადადის — MonoBehaviour სინგლტონი ზედმეტია. პროექტში domain reload Play-ზე გათიშულია (`m_EnterPlayModeOptions: 1`), ამიტომ reset-ის გარეშე MainScene-ის პირდაპირი Play წინა სესიის კამპანიის რეჟიმს გააგრძელებდა.
- How to change: თუ სესიას Update/coroutine დასჭირდება → DontDestroyOnLoad კომპონენტი.

### 2026-10-25 — UI: uGUI + legacy Text, ციკლური ღილაკები
- Decision: Canvas + `UnityEngine.UI.Text` (`LegacyRuntime.ttf`), ღილაკების `onClick` კოდით (`AddListener`), Quick Game-ის არჩევანი ციკლური ღილაკებით (Dropdown/Toggle-ის გარეშე). EventSystem + `InputSystemUIInputModule` (`AssignDefaultActions`). MainScene-ის HUD და PromotionPrompt რჩება IMGUI.
- Reason: მომხმარებელმა uGUI მოითხოვა. TextMeshPro-ს სჭირდება TMP Essential Resources-ის იმპორტი (დამატებითი USER ნაბიჯი და Assets-ში ფაილები), ქართული font ისედაც Stage 3-შია. `Text` ugui 2.6.0-ში obsolete არ არის (შემოწმდა). persistent listener-ები Editor სკრიპტში idempotency-ს ართულებს.
- How to change: Stage 3 — TMP + ქართული font asset; ტექსტები `CampaignText`-სა და კომპონენტებშია თავმოყრილი.

### 2026-10-25 — Tools → Setup Campaign Scenes: გენერირებული UI თავიდან იქმნება
- Decision: ახალი Editor ბრძანება ქმნის MainMenu/CampaignMap სცენებს, MainScene-ში ამატებს `BattleCanvas`-ს და `EventSystem`-ს, აბამს `GameController.battlePanel`-ს და Build Settings-ს წერს ზუსტად `[MainMenu, CampaignMap, MainScene]`. გენერირებული Canvas-ები ყოველ გაშვებაზე იშლება და თავიდან იქმნება. `SampleScene` სიიდან ამოდის (ფაილი რჩება).
- Reason: დამწყებისთვის ერთი ბრძანება; წაშლა+შექმნა უმარტივესი idempotency-ია ხისთვის, რომელიც მთლიანად გენერირებულია. `ProjectSettings/EditorBuildSettings.asset`-ს მხოლოდ ეს ბრძანება ცვლის USER-ის გაშვებისას (აგენტები `ProjectSettings/`-ს არ ეხებიან).
- Trade-off: გენერირებულ Canvas-ებში ხელით ცვლილება ხელახალ გაშვებაზე იკარგება (Stage 3-ის art-მდე მისაღებია).
- Note: ბრძანება მოითხოვს, რომ MainScene უკვე აწყობილი იყოს (Tools → Setup Chess Scene); `ChessSceneSetup` `battlePanel`-ს არ ეხება.

### 2026-10-25 — MainScene: Stage 1-ის ქცევა რჩება; startFen ყველა რეჟიმში (ჩანაცვლდა — იხ. „USER task 28-ის შენიშვნები“)
- Decision: `GameSession.Mode == None` → არაფერი იცვლება (პანელი დამალულია). `startFen` (თუ ცარიელი არ არის) კამპანიასა და Quick Game-შიც გამოიყენება.
- Reason: მომხმარებლის მოთხოვნა (standalone). startFen USER-ს აძლევს საშუალებას, კამპანიის გამარჯვება/ფრე წამებში შეამოწმოს (მატი 1 სვლით); Setup Chess Scene მას ცარიელზე აბრუნებს.
- Risk: თუ startFen სცენაში დარჩა, კამპანია ამ პოზიციიდან დაიწყება — USER task-ები ბოლოს მის გასუფთავებას ითხოვს.

### 2026-10-25 — New Campaign-ის დადასტურება და ბრძოლიდან გასვლა
- Decision: `New Campaign` არსებული კამპანიისას ითხოვს დადასტურებას. ბრძოლის დროს ზედა-მარჯვნივ `Back to map` / `Back to menu` — გასვლა შედეგის გარეშე.
- Reason (ვარაუდი): პირველი იცავს პროგრესს შემთხვევითი დაკარგვისგან; მეორის გარეშე 2200 Elo-ს ბრძოლიდან გასვლა მხოლოდ პარტიის დასრულებით იქნებოდა შესაძლებელი. ორივე მცირე UI-ია.

### 2026-10-25 — Stage 2-ის ტესტები იმავე პროექტში; owner-ები
- Decision: Campaign-ის ტესტები `Tests/ChessCore.Tests/Campaign/`-შია (ახალი სატესტო პროექტის გარეშე); build პროექტი `Tests/ChessCampaign.Build`. Owner-ები: იხ. „Stage 2: owner-ები, დიზაინი, E2E“.
- Reason: ერთი `dotnet test` ბრძანება, verify.sh უცვლელია.

### 2026-10-25 — Stage 2: owner-ები, დიზაინი, E2E
- Decision: Stage 2-ის ყველა agent task-ის owner = `task-executor` (Campaign, AI, Game, UI, Editor), როგორც Stage 1-ში. designer/DESIGN.md არ გამოიყენება — UI-ის ფერები, Canvas Scaler და ზომები მოკლედ ჩაწერილია API Contract-ში („UI სტილი (Stage 2)“). browser-tester/Playwright არ გამოიყენება; Play-mode შემოწმება = USER task-ები 28, 29.
- Reason: მომხმარებლის მითითება — Unity C# არ არის web UI; სქემატური UI დროებითია (რუკის art და ქართული font — Stage 3).
- How to change: Stage 3-ში რეალური art-ისთვის შეიძლება DESIGN.md-ის შექმნა.

### 2026-10-25 — Setup Campaign Scenes: განლაგების დეტალები (T27)
- Decision: ConfirmPanel-ის ორი ღილაკი (`Start new` / `Cancel`) 350×80-ია (x = ±185), არა 400×80; სათაურის, სტატუსის და პანელების შიგნით ღილაკების ზუსტი პოზიციები Editor სკრიპტშია (contract მათ არ აკონკრეტებს). რეგიონის ღილაკების anchor-ები სკრიპტშია id-ის მიხედვით (`Region`-ს anchor არ აქვს).
- Reason: ორი 400 სიგანის ღილაკი 800 სიგანის პანელში არ ეტევა.
- How to change: `Assets/Editor/CampaignScenesSetup.cs` → ხელახლა Tools → Setup Campaign Scenes.

### 2026-10-25 — Finish-check Stage 2: შენახვის დაცვა, Continue დამალულია, რეგიონის label
- Decision: (1) ჩაწერის ჩავარდნისას tmp იშლება მხოლოდ თუ `campaign.json` არსებობს; `Load` აღადგენს დარჩენილ tmp-ს; fallback მხოლოდ `PlatformNotSupportedException`-ზე; > 64 KB ფაილი = დაზიანებული. (2) `Continue` იმალება (`SetActive`), არ ითიშება. (3) რეგიონის ღილაკი 380×170, label best fit 14–24.
- Reason: (1) წინა კოდში I/O ჩავარდნამ შეიძლება ძველი და ახალი შენახვა ერთად წაშალოს; (2) მომხმარებლის მოთხოვნა „ჩანს მხოლოდ შენახვისას“; (3) 4-ხაზიანი label 280×130-ში მდგომარეობის ხაზს კვეთდა.
- How to change: `CampaignSaveStore`, `MainMenuController`, `CampaignScenesSetup`.

### 2026-10-25 — USER task 28-ის შენიშვნები: debug FEN, რუკის სათაური, რეგიონის ტექსტი
- Decision:
  - `startFen` მოქმედებს მხოლოდ `Mode == None`-ში; კამპანიის ტესტისთვის ახალი ველი `debugCampaignStartFen` (მხოლოდ Editor-ში და მხოლოდ Campaign რეჟიმში; HUD-ზე `[DEBUG FEN]`); Quick Game — ყოველთვის საწყისი პოზიცია. ძველი გადაწყვეტილება „startFen ყველა რეჟიმში“ ჩანაცვლდა.
  - ველი თავად `#if UNITY_EDITOR`-ში **არ** არის, მხოლოდ მისი გამოყენება: MonoBehaviour-ის სერიალიზებული ველის `#if`-ში მოქცევა Editor-სა და build-ს შორის serialization layout-ის შეუსაბამობას იწვევს. build-ში ველის მნიშვნელობა რჩება, მაგრამ არაფერზე მოქმედებს.
  - CampaignMap: `MapArea` 1500×660 (0, -50); `TitleText` იქმნება მის შემდეგ (ზემოთ ხატავს). რეგიონის ღილაკი 400×190, label Bold best fit 22–28; anchor-ები: imereti 0.17, kartli 0.47, kakheti 0.80, khevsureti 0.66 (x), y უცვლელია.
- Reason: მომხმარებლის მოთხოვნა USER task 28-ის შემდეგ (კამპანიის მოგება 800 Elo-ზე ხელით ვერ მოწმდება; სათაური რუკის უკან იმალებოდა; ტექსტი ძალიან პატარა იყო). startFen-ის ყველა რეჟიმზე გავრცელება build-ში დატოვების რისკს ქმნიდა.
- How to change: `GameController` (debug ველი), `CampaignScenesSetup` (განლაგება) → ხელახლა Tools → Setup Campaign Scenes.

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
- ვარაუდი (Stage 2): კამპანიაში ადამიანი ყოველთვის თეთრია.
- ვარაუდი (Stage 2): რეგიონების ეპოქები placeholder-ია; Elo 800/1100/1400/1800/2200 და Elo → AI ზოლები მიახლოებითია, გაზომილი არ არის.
- ვარაუდი (Stage 2): Liberated რეგიონის თავიდან თამაში შეიძლება და პროგრესს არ ცვლის; განთავისუფლება მუდმივია.
- ვარაუდი (Stage 2): შედეგი ითვლება მხოლოდ აქტიური AI-ით; hot-seat-ის გამარჯვება რეგიონს არ ათავისუფლებს.
- ვარაუდი (Stage 2): New Campaign-ს დადასტურება სჭირდება; ბრძოლის შუაში გასვლა შედეგს არ წერს.
- ვარაუდი (Stage 2): ერთი save slot; დაზიანებული ფაილი `campaign.corrupt.json`-ად ინახება (მხოლოდ ბოლო).
- ვარაუდი (Stage 2): Quick Game-ის ნაგულისხმევი = White + Medium; Quick Game არაფერს ინახავს.

## Accepted Risks
