using System.Collections.Generic;
using Chess.Campaign;
using Chess.Core;
using Chess.Game;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.Editor
{
    /// <summary>
    /// Tools -> Setup Campaign Scenes: builds MainMenu and CampaignMap scenes, the BattleCanvas in MainScene
    /// and the Build Settings scene list. Generated canvases are deleted and recreated on every run.
    /// </summary>
    public static class CampaignScenesSetup
    {
        private const string Tag = "[CampaignScenesSetup] ";
        private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
        private const string CampaignMapPath = "Assets/Scenes/CampaignMap.unity";
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";

        private static readonly List<string> Created = new List<string>();
        private static readonly List<string> Updated = new List<string>();
        private static bool failed;
        private static TMP_FontAsset font;

        [MenuItem("Tools/Setup Campaign Scenes")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(Tag + "Cannot run during Play mode.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            font = GeorgianFontSetup.Ensure(out bool fontCharactersMissing);
            if (font == null)
            {
                Debug.LogError(Tag + "Georgian font is not ready, nothing was changed.");
                return;
            }

            Created.Clear();
            Updated.Clear();
            failed = fontCharactersMissing;

            var scene = OpenOrCreate(MainMenuPath, "MainMenu");
            BuildCameraAndEventSystem();
            BuildMainMenu();
            Save(scene);

            scene = OpenOrCreate(CampaignMapPath, "CampaignMap");
            BuildCameraAndEventSystem();
            BuildCampaignMap();
            Save(scene);

            scene = OpenOrCreate(MainScenePath, "MainScene");
            BuildBattleCanvas();
            Save(scene);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(CampaignMapPath, true),
                new EditorBuildSettingsScene(MainScenePath, true),
            };

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);

            Debug.Log(Tag + "Done" + (failed ? " WITH ERRORS (see above)" : "") + ".\n"
                + "Created: " + (Created.Count == 0 ? "-" : string.Join(", ", Created)) + ";\n"
                + "Updated: " + (Updated.Count == 0 ? "-" : string.Join(", ", Updated)) + ";\n"
                + "Build scenes: MainMenu, CampaignMap, MainScene");
        }

        private static string Get(TextId id)
        {
            return TextTable.Active.Get(id);
        }

        // ------------------------------------------------------------ scenes

        private static UnityEngine.SceneManagement.Scene OpenOrCreate(string path, string label)
        {
            if (!System.IO.File.Exists(path))
            {
                var created = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(created, path);
                Created.Add("scene " + label);
                return created;
            }
            Updated.Add("scene " + label);
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private static void Save(UnityEngine.SceneManagement.Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildCameraAndEventSystem()
        {
            GameObject camGo = GameObject.FindWithTag("MainCamera");
            if (camGo == null)
            {
                foreach (GameObject r in RootObjects())
                {
                    if (r.name == "Main Camera") { camGo = r; break; }
                }
            }
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                Created.Add("Main Camera");
            }
            Camera cam = camGo.GetComponent<Camera>();
            if (cam == null) cam = camGo.AddComponent<Camera>();
            if (camGo.GetComponent<AudioListener>() == null) camGo.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = EditorUi.Background;

            EditorUi.EnsureEventSystem(Created);
        }

        private static IEnumerable<GameObject> RootObjects()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        // ------------------------------------------------------------ MainMenu

        private static void BuildMainMenu()
        {
            EditorUi.DeleteRoots("MainMenuCanvas", Updated);
            GameObject canvas = EditorUi.NewCanvas("MainMenuCanvas", 0);
            Created.Add("MainMenuCanvas");
            var controller = canvas.AddComponent<MainMenuController>();

            EditorUi.MakeText("TitleText", canvas.transform, font, Get(TextId.MenuTitle), 64f,
                EditorUi.Top(new Vector2(0f, -140f), new Vector2(1400f, 100f)), TextAlignmentOptions.Center, Color.white, false, 40f);

            // MainPanel positions its buttons itself: a hidden Continue button leaves no gap.
            GameObject mainPanel = EditorUi.NewRect("MainPanel", canvas.transform,
                EditorUi.Center(new Vector2(0f, -40f), new Vector2(400f, 380f)));
            var layout = mainPanel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var buttonSize = new Vector2(400f, 80f);
            Button newCampaign = EditorUi.MakeButton("NewCampaignButton", mainPanel.transform, font, Get(TextId.MenuNewCampaign),
                EditorUi.Top(Vector2.zero, buttonSize), 32f, out _);
            Button cont = EditorUi.MakeButton("ContinueButton", mainPanel.transform, font, Get(TextId.MenuContinue),
                EditorUi.Top(Vector2.zero, buttonSize), 32f, out _);
            Button quick = EditorUi.MakeButton("QuickGameButton", mainPanel.transform, font, Get(TextId.MenuQuickGame),
                EditorUi.Top(Vector2.zero, buttonSize), 32f, out _);
            Button quit = EditorUi.MakeButton("QuitButton", mainPanel.transform, font, Get(TextId.MenuQuit),
                EditorUi.Top(Vector2.zero, buttonSize), 32f, out _);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)mainPanel.transform);

            // Outside MainPanel so it never takes part in the layout.
            TextMeshProUGUI warning = EditorUi.MakeText("WarningText", canvas.transform, font, "", 28f,
                EditorUi.Bottom(new Vector2(0f, 120f), new Vector2(1400f, 120f)), TextAlignmentOptions.Center, EditorUi.WarningColor, false, 20f);

            GameObject confirm = EditorUi.NewPanel("ConfirmPanel", canvas.transform, new Vector2(800f, 300f));
            EditorUi.MakeText("ConfirmText", confirm.transform, font, Get(TextId.ConfirmNewCampaign), 28f,
                EditorUi.Center(new Vector2(0f, 60f), new Vector2(740f, 120f)), TextAlignmentOptions.Center, Color.white, false, 20f);
            Button yes = EditorUi.MakeButton("ConfirmYesButton", confirm.transform, font, Get(TextId.ConfirmYes),
                EditorUi.Center(new Vector2(-185f, -90f), new Vector2(350f, 80f)), 32f, out _);
            Button no = EditorUi.MakeButton("ConfirmNoButton", confirm.transform, font, Get(TextId.Cancel),
                EditorUi.Center(new Vector2(185f, -90f), new Vector2(350f, 80f)), 32f, out _);
            confirm.SetActive(false);

            GameObject quickPanel = EditorUi.NewPanel("QuickGamePanel", canvas.transform, new Vector2(600f, 520f));
            EditorUi.MakeText("QuickGameTitleText", quickPanel.transform, font, Get(TextId.QuickGameTitle), 40f,
                EditorUi.Center(new Vector2(0f, 200f), new Vector2(560f, 60f)), TextAlignmentOptions.Center, Color.white, false, 28f);
            Button color = EditorUi.MakeButton("ColorButton", quickPanel.transform, font, GameText.QuickColorLabel(Side.White),
                EditorUi.Center(new Vector2(0f, 100f), buttonSize), 32f, out TMP_Text colorLabel);
            colorLabel.gameObject.name = "ColorLabel";
            Button difficulty = EditorUi.MakeButton("DifficultyButton", quickPanel.transform, font,
                GameText.QuickDifficultyLabel(Get(TextId.DifficultyMedium)),
                EditorUi.Center(new Vector2(0f, 0f), buttonSize), 32f, out TMP_Text difficultyLabel);
            difficultyLabel.gameObject.name = "DifficultyLabel";
            Button start = EditorUi.MakeButton("StartQuickGameButton", quickPanel.transform, font, Get(TextId.QuickStart),
                EditorUi.Center(new Vector2(0f, -100f), buttonSize), 32f, out _);
            Button back = EditorUi.MakeButton("BackFromQuickGameButton", quickPanel.transform, font, Get(TextId.Back),
                EditorUi.Center(new Vector2(0f, -200f), buttonSize), 32f, out _);
            quickPanel.SetActive(false);

            using (var so = new SerializedObject(controller))
            {
                SetRef(so, "mainPanel", mainPanel);
                SetRef(so, "confirmPanel", confirm);
                SetRef(so, "quickGamePanel", quickPanel);
                SetRef(so, "newCampaignButton", newCampaign);
                SetRef(so, "continueButton", cont);
                SetRef(so, "quickGameButton", quick);
                SetRef(so, "quitButton", quit);
                SetRef(so, "confirmYesButton", yes);
                SetRef(so, "confirmNoButton", no);
                SetRef(so, "colorButton", color);
                SetRef(so, "difficultyButton", difficulty);
                SetRef(so, "startQuickGameButton", start);
                SetRef(so, "backFromQuickGameButton", back);
                SetRef(so, "warningText", warning);
                SetRef(so, "colorLabel", colorLabel);
                SetRef(so, "difficultyLabel", difficultyLabel);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(controller);
        }

        // ------------------------------------------------------------ CampaignMap

        private static Vector2? RegionAnchor(string id)
        {
            switch (id)
            {
                case "kartli": return new Vector2(0.47f, 0.42f);
                case "kakheti": return new Vector2(0.80f, 0.38f);
                case "imereti": return new Vector2(0.17f, 0.45f);
                case "svaneti": return new Vector2(0.30f, 0.78f);
                case "khevsureti": return new Vector2(0.66f, 0.74f);
                default: return null;
            }
        }

        private static void BuildCampaignMap()
        {
            EditorUi.DeleteRoots("CampaignMapCanvas", Updated);
            GameObject canvas = EditorUi.NewCanvas("CampaignMapCanvas", 0);
            Created.Add("CampaignMapCanvas");
            var controller = canvas.AddComponent<CampaignMapController>();

            // x 210-1710, y 260-920 (reference 1920x1080, y from the top).
            GameObject map = EditorUi.NewRect("MapArea", canvas.transform,
                EditorUi.Center(new Vector2(0f, -50f), new Vector2(1500f, 660f)));
            Image mapImage = map.AddComponent<Image>();
            mapImage.color = EditorUi.MapColor;
            mapImage.raycastTarget = false;

            // y 25-115, created after MapArea so it draws on top.
            EditorUi.MakeText("TitleText", canvas.transform, font, Get(TextId.MapTitle), 64f,
                EditorUi.Top(new Vector2(0f, -25f), new Vector2(1400f, 90f)), TextAlignmentOptions.Center, Color.white, false, 40f);

            var buttons = new List<RegionButton>();
            foreach (Region region in CampaignRegions.All)
            {
                Vector2? anchor = RegionAnchor(region.Id);
                if (anchor == null)
                {
                    failed = true;
                    Debug.LogError(Tag + "No map anchor defined for region '" + region.Id + "'");
                    continue;
                }
                Button button = EditorUi.MakeButton("Region_" + region.Id, map.transform, font, region.Name,
                    EditorUi.Anchored(anchor.Value, new Vector2(400f, 190f)), 28f, out TMP_Text label);
                label.fontStyle = FontStyles.Bold;
                label.fontSizeMin = 18f;
                label.fontSizeMax = 28f;
                label.textWrappingMode = TextWrappingModes.Normal;
                label.overflowMode = TextOverflowModes.Truncate;
                label.rectTransform.offsetMin = new Vector2(8f, 8f);
                label.rectTransform.offsetMax = new Vector2(-8f, -8f);
                Image bg = button.GetComponent<Image>();
                bg.color = EditorUi.LockedColor;
                var rb = button.gameObject.AddComponent<RegionButton>();
                using (var so = new SerializedObject(rb))
                {
                    SetString(so, "regionId", region.Id);
                    SetRef(so, "button", button);
                    SetRef(so, "label", label);
                    SetRef(so, "background", bg);
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                EditorUtility.SetDirty(rb);
                buttons.Add(rb);
            }

            // x 460-1460, y 935-1025.
            TextMeshProUGUI status = EditorUi.MakeText("StatusText", canvas.transform, font, "", 28f,
                EditorUi.Bottom(new Vector2(0f, 55f), new Vector2(1000f, 90f)), TextAlignmentOptions.Center, Color.white, false, 20f);
            // x 40-440, y 960-1040.
            Button back = EditorUi.MakeButton("BackButton", canvas.transform, font, Get(TextId.BackToMenu),
                EditorUi.BottomLeft(new Vector2(40f, 40f), new Vector2(400f, 80f)), 32f, out _);

            using (var so = new SerializedObject(controller))
            {
                SerializedProperty arr = Find(so, "regionButtons");
                if (arr != null)
                {
                    arr.arraySize = buttons.Count;
                    for (int i = 0; i < buttons.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
                }
                SetRef(so, "statusText", status);
                SetRef(so, "backButton", back);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(controller);
        }

        // ------------------------------------------------------------ MainScene

        private static void BuildBattleCanvas()
        {
            GameController game = null;
            foreach (GameObject r in RootObjects())
            {
                if (r.name != "ChessGame") continue;
                game = r.GetComponent<GameController>();
                if (game != null) break;
            }
            if (game == null)
            {
                failed = true;
                Debug.LogError(Tag + "ChessGame not found in MainScene. Run Tools > Setup Chess Scene first.");
                return;
            }

            EditorUi.EnsureEventSystem(Created);

            EditorUi.DeleteRoots("BattleCanvas", Updated);
            GameObject canvas = EditorUi.NewCanvas("BattleCanvas", 10);
            Created.Add("BattleCanvas");
            var panel = canvas.AddComponent<BattlePanel>();

            // x 410-1510, y 24-94.
            TextMeshProUGUI info = EditorUi.MakeText("InfoText", canvas.transform, font, "", 28f,
                EditorUi.Top(new Vector2(0f, -24f), new Vector2(1100f, 70f)), TextAlignmentOptions.Top, Color.white, false, 20f);
            // x 1576-1896, y 24-94.
            Button leave = EditorUi.MakeButton("LeaveButton", canvas.transform, font, Get(TextId.BackToMap),
                EditorUi.TopRight(new Vector2(-24f, -24f), new Vector2(320f, 70f)), 32f, out TMP_Text leaveLabel);
            leaveLabel.gameObject.name = "LeaveLabel";

            // x 580-1340, y 340-740.
            GameObject result = EditorUi.NewPanel("ResultPanel", canvas.transform, new Vector2(760f, 400f));
            TextMeshProUGUI title = EditorUi.MakeText("TitleText", result.transform, font, "", 40f,
                EditorUi.Center(new Vector2(0f, 140f), new Vector2(700f, 70f)), TextAlignmentOptions.Center, Color.white, false, 28f);
            TextMeshProUGUI message = EditorUi.MakeText("MessageText", result.transform, font, "", 28f,
                EditorUi.Center(new Vector2(0f, 30f), new Vector2(700f, 140f)), TextAlignmentOptions.Center, Color.white, false, 20f);
            Button retry = EditorUi.MakeButton("RetryButton", result.transform, font, Get(TextId.Retry),
                EditorUi.Center(new Vector2(-170f, -130f), new Vector2(320f, 70f)), 32f, out TMP_Text retryLabel);
            retryLabel.gameObject.name = "RetryLabel";
            Button back = EditorUi.MakeButton("BackButton", result.transform, font, Get(TextId.BackToMap),
                EditorUi.Center(new Vector2(170f, -130f), new Vector2(320f, 70f)), 32f, out TMP_Text backLabel);
            backLabel.gameObject.name = "BackLabel";
            result.SetActive(false);

            using (var so = new SerializedObject(panel))
            {
                SetRef(so, "infoText", info);
                SetRef(so, "leaveButton", leave);
                SetRef(so, "leaveLabel", leaveLabel);
                SetRef(so, "resultRoot", result);
                SetRef(so, "titleText", title);
                SetRef(so, "messageText", message);
                SetRef(so, "retryButton", retry);
                SetRef(so, "retryLabel", retryLabel);
                SetRef(so, "backButton", back);
                SetRef(so, "backLabel", backLabel);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(panel);

            using (var so = new SerializedObject(game))
            {
                SetRef(so, "battlePanel", panel);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(game);
            Updated.Add("GameController.battlePanel");
        }

        // ------------------------------------------------------------ SerializedObject helpers

        private static SerializedProperty Find(SerializedObject so, string name)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null)
            {
                failed = true;
                Debug.LogError(Tag + "Property '" + name + "' not found on " + so.targetObject.GetType().Name);
            }
            return p;
        }

        private static void SetRef(SerializedObject so, string name, UnityEngine.Object value)
        {
            SerializedProperty p = Find(so, name);
            if (p != null) p.objectReferenceValue = value;
        }

        private static void SetString(SerializedObject so, string name, string value)
        {
            SerializedProperty p = Find(so, name);
            if (p != null) p.stringValue = value;
        }
    }
}
