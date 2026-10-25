using System;
using System.Collections.Generic;
using Chess.Campaign;
using Chess.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

        private static readonly Color Background = Hex("1C1C22", 1f);
        private static readonly Color PanelColor = Hex("2A2A33", 0.95f);
        private static readonly Color MapColor = Hex("2F3A2A", 1f);
        private static readonly Color ButtonColor = Hex("3A4A6B", 1f);
        private static readonly Color WarningColor = Hex("FFB347", 1f);
        private static readonly Color DisabledColor = Hex("5A5A5A", 0.5f);
        private static readonly Color LockedColor = Hex("5A5A5A", 1f);

        private static readonly List<string> Created = new List<string>();
        private static readonly List<string> Updated = new List<string>();
        private static bool failed;
        private static Font font;

        [MenuItem("Tools/Setup Campaign Scenes")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(Tag + "Cannot run during Play mode.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Created.Clear();
            Updated.Clear();
            failed = false;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                Debug.LogError(Tag + "Built-in font LegacyRuntime.ttf not found.");
                return;
            }

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
            cam.backgroundColor = Background;

            EnsureEventSystem();
        }

        private static void EnsureEventSystem()
        {
            EventSystem[] all = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            EventSystem es = null;
            foreach (EventSystem e in all)
            {
                if (es == null) es = e;
                else UnityEngine.Object.DestroyImmediate(e.gameObject);
            }
            if (es == null)
            {
                es = new GameObject("EventSystem").AddComponent<EventSystem>();
                Created.Add("EventSystem");
            }
            foreach (StandaloneInputModule old in es.GetComponents<StandaloneInputModule>())
            {
                UnityEngine.Object.DestroyImmediate(old);
            }
            var module = es.GetComponent<InputSystemUIInputModule>();
            if (module == null) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private static IEnumerable<GameObject> RootObjects()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        private static void DeleteRoots(string name)
        {
            foreach (GameObject r in RootObjects())
            {
                if (r.name != name) continue;
                UnityEngine.Object.DestroyImmediate(r);
                Updated.Add(name + " (recreated)");
            }
        }

        // ------------------------------------------------------------ MainMenu

        private static void BuildMainMenu()
        {
            DeleteRoots("MainMenuCanvas");
            GameObject canvas = NewCanvas("MainMenuCanvas", 0);
            Created.Add("MainMenuCanvas");
            var controller = canvas.AddComponent<MainMenuController>();

            MakeText("TitleText", canvas.transform, "Georgian Battle Chess", 64,
                Top(new Vector2(0f, -140f), new Vector2(1400f, 100f)), TextAnchor.MiddleCenter, Color.white);

            GameObject mainPanel = NewRect("MainPanel", canvas.transform, Center(new Vector2(0f, -40f), new Vector2(400f, 380f)));
            Button newCampaign = MakeButton("NewCampaignButton", mainPanel.transform, "New Campaign", Center(new Vector2(0f, 150f), new Vector2(400f, 80f)), 32, out _);
            Button cont = MakeButton("ContinueButton", mainPanel.transform, "Continue", Center(new Vector2(0f, 50f), new Vector2(400f, 80f)), 32, out _);
            Button quick = MakeButton("QuickGameButton", mainPanel.transform, "Quick Game", Center(new Vector2(0f, -50f), new Vector2(400f, 80f)), 32, out _);
            Button quit = MakeButton("QuitButton", mainPanel.transform, "Quit", Center(new Vector2(0f, -150f), new Vector2(400f, 80f)), 32, out _);

            Text warning = MakeText("WarningText", canvas.transform, "", 28,
                Bottom(new Vector2(0f, 120f), new Vector2(1400f, 120f)), TextAnchor.MiddleCenter, WarningColor);

            GameObject confirm = NewPanel("ConfirmPanel", canvas.transform, new Vector2(800f, 300f));
            MakeText("ConfirmText", confirm.transform, "Start a new campaign? Current progress will be lost.", 28,
                Center(new Vector2(0f, 60f), new Vector2(740f, 120f)), TextAnchor.MiddleCenter, Color.white);
            Button yes = MakeButton("ConfirmYesButton", confirm.transform, "Start new", Center(new Vector2(-185f, -90f), new Vector2(350f, 80f)), 32, out _);
            Button no = MakeButton("ConfirmNoButton", confirm.transform, "Cancel", Center(new Vector2(185f, -90f), new Vector2(350f, 80f)), 32, out _);
            confirm.SetActive(false);

            GameObject quickPanel = NewPanel("QuickGamePanel", canvas.transform, new Vector2(600f, 520f));
            MakeText("QuickGameTitleText", quickPanel.transform, "Quick Game", 40,
                Center(new Vector2(0f, 200f), new Vector2(560f, 60f)), TextAnchor.MiddleCenter, Color.white);
            Button color = MakeButton("ColorButton", quickPanel.transform, "Color: White", Center(new Vector2(0f, 100f), new Vector2(400f, 80f)), 32, out Text colorLabel);
            colorLabel.gameObject.name = "ColorLabel";
            Button difficulty = MakeButton("DifficultyButton", quickPanel.transform, "Difficulty: Medium", Center(new Vector2(0f, 0f), new Vector2(400f, 80f)), 32, out Text difficultyLabel);
            difficultyLabel.gameObject.name = "DifficultyLabel";
            Button start = MakeButton("StartQuickGameButton", quickPanel.transform, "Start", Center(new Vector2(0f, -100f), new Vector2(400f, 80f)), 32, out _);
            Button back = MakeButton("BackFromQuickGameButton", quickPanel.transform, "Back", Center(new Vector2(0f, -200f), new Vector2(400f, 80f)), 32, out _);
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
                case "kartli": return new Vector2(0.50f, 0.42f);
                case "kakheti": return new Vector2(0.78f, 0.38f);
                case "imereti": return new Vector2(0.24f, 0.45f);
                case "svaneti": return new Vector2(0.30f, 0.78f);
                case "khevsureti": return new Vector2(0.64f, 0.74f);
                default: return null;
            }
        }

        private static void BuildCampaignMap()
        {
            DeleteRoots("CampaignMapCanvas");
            GameObject canvas = NewCanvas("CampaignMapCanvas", 0);
            Created.Add("CampaignMapCanvas");
            var controller = canvas.AddComponent<CampaignMapController>();

            MakeText("TitleText", canvas.transform, "Campaign Map", 64,
                Top(new Vector2(0f, -80f), new Vector2(1400f, 100f)), TextAnchor.MiddleCenter, Color.white);

            GameObject map = NewRect("MapArea", canvas.transform, Center(new Vector2(0f, -20f), new Vector2(1500f, 720f)));
            Image mapImage = map.AddComponent<Image>();
            mapImage.color = MapColor;
            mapImage.raycastTarget = false;

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
                Button button = MakeButton("Region_" + region.Id, map.transform, region.Name, Anchored(anchor.Value, new Vector2(380f, 170f)), 24, out Text label);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 14;
                label.resizeTextMaxSize = 24;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                label.rectTransform.offsetMin = new Vector2(8f, 8f);
                label.rectTransform.offsetMax = new Vector2(-8f, -8f);
                Image bg = button.GetComponent<Image>();
                bg.color = LockedColor;
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

            Text status = MakeText("StatusText", canvas.transform, "", 28,
                Bottom(new Vector2(0f, 100f), new Vector2(1000f, 90f)), TextAnchor.MiddleCenter, Color.white);
            Button back = MakeButton("BackButton", canvas.transform, "Back to menu",
                new Placement(Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(40f, 40f), new Vector2(400f, 80f)), 32, out _);

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

            EnsureEventSystem();

            DeleteRoots("BattleCanvas");
            GameObject canvas = NewCanvas("BattleCanvas", 10);
            Created.Add("BattleCanvas");
            var panel = canvas.AddComponent<BattlePanel>();

            Text info = MakeText("InfoText", canvas.transform, "", 28,
                Top(new Vector2(0f, -24f), new Vector2(1200f, 60f)), TextAnchor.UpperCenter, Color.white);
            Button leave = MakeButton("LeaveButton", canvas.transform, "Back", new Placement(Vector2.one, Vector2.one, Vector2.one, new Vector2(-24f, -24f), new Vector2(260f, 70f)), 32, out Text leaveLabel);
            leaveLabel.gameObject.name = "LeaveLabel";

            GameObject result = NewPanel("ResultPanel", canvas.transform, new Vector2(700f, 360f));
            Text title = MakeText("TitleText", result.transform, "", 40,
                Center(new Vector2(0f, 130f), new Vector2(660f, 60f)), TextAnchor.MiddleCenter, Color.white);
            Text message = MakeText("MessageText", result.transform, "", 28,
                Center(new Vector2(0f, 30f), new Vector2(660f, 120f)), TextAnchor.MiddleCenter, Color.white);
            Button retry = MakeButton("RetryButton", result.transform, "Retry", Center(new Vector2(-150f, -120f), new Vector2(260f, 70f)), 32, out Text retryLabel);
            retryLabel.gameObject.name = "RetryLabel";
            Button back = MakeButton("BackButton", result.transform, "Back", Center(new Vector2(150f, -120f), new Vector2(260f, 70f)), 32, out Text backLabel);
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

        // ------------------------------------------------------------ UI builders

        private readonly struct Placement
        {
            public readonly Vector2 AnchorMin;
            public readonly Vector2 AnchorMax;
            public readonly Vector2 Pivot;
            public readonly Vector2 Position;
            public readonly Vector2 Size;

            public Placement(Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
            {
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                Pivot = pivot;
                Position = position;
                Size = size;
            }
        }

        private static Placement Center(Vector2 pos, Vector2 size)
        {
            var half = new Vector2(0.5f, 0.5f);
            return new Placement(half, half, half, pos, size);
        }

        private static Placement Top(Vector2 pos, Vector2 size)
        {
            return new Placement(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), pos, size);
        }

        private static Placement Bottom(Vector2 pos, Vector2 size)
        {
            return new Placement(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), pos, size);
        }

        private static Placement Anchored(Vector2 anchor, Vector2 size)
        {
            return new Placement(anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        }

        private static GameObject NewCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static GameObject NewRect(string name, Transform parent, Placement p)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Apply((RectTransform)go.transform, p);
            return go;
        }

        private static void Apply(RectTransform rt, Placement p)
        {
            rt.anchorMin = p.AnchorMin;
            rt.anchorMax = p.AnchorMax;
            rt.pivot = p.Pivot;
            rt.anchoredPosition = p.Position;
            rt.sizeDelta = p.Size;
        }

        private static GameObject NewPanel(string name, Transform parent, Vector2 size)
        {
            GameObject go = NewRect(name, parent, Center(Vector2.zero, size));
            go.AddComponent<Image>().color = PanelColor;
            return go;
        }

        private static Text MakeText(string name, Transform parent, string text, int fontSize, Placement p, TextAnchor alignment, Color color)
        {
            GameObject go = NewRect(name, parent, p);
            return ConfigureText(go.AddComponent<Text>(), text, fontSize, alignment, color);
        }

        private static Text ConfigureText(Text t, string text, int fontSize, TextAnchor alignment, Color color)
        {
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        private static Button MakeButton(string name, Transform parent, string labelText, Placement p, int fontSize, out Text label)
        {
            GameObject go = NewRect(name, parent, p);
            var image = go.AddComponent<Image>();
            image.color = ButtonColor;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = DisabledColor;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.1f;
            button.colors = cb;

            GameObject labelGo = NewRect("Label", go.transform, new Placement(Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero));
            label = ConfigureText(labelGo.AddComponent<Text>(), labelText, fontSize, TextAnchor.MiddleCenter, Color.white);
            return button;
        }

        private static Color Hex(string hex, float alpha)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            c.a = alpha;
            return c;
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
