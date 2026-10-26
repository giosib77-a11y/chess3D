using System;
using System.Collections.Generic;
using Chess.AI;
using Chess.Campaign;
using Chess.Core;
using Chess.Game;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Chess.Editor
{
    /// <summary>Tools -> Setup Chess Scene: idempotently builds materials, camera and the ChessGame root in MainScene.</summary>
    public static class ChessSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/MainScene.unity";
        private const string MaterialsFolder = "Assets/Materials";
        private const string Tag = "[ChessSceneSetup] ";

        private static readonly List<string> Created = new List<string>();
        private static readonly List<string> Updated = new List<string>();
        private static bool failed;

        [MenuItem("Tools/Setup Chess Scene")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(Tag + "Cannot run during Play mode.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!System.IO.File.Exists(ScenePath))
            {
                Debug.LogError(Tag + "Scene not found: " + ScenePath);
                return;
            }
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
            {
                Debug.LogError(Tag + "Shader 'Universal Render Pipeline/Lit' not found.");
                return;
            }

            TMP_FontAsset font = GeorgianFontSetup.Ensure(out bool fontCharactersMissing);
            if (font == null)
            {
                Debug.LogError(Tag + "Georgian font is not ready, nothing was changed.");
                return;
            }

            Created.Clear();
            Updated.Clear();
            failed = fontCharactersMissing;

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EnsureFolder();
            Material ground = EnsureMaterial(lit, "M_Ground", "8A9A5B", 255, false);
            Material tile = EnsureMaterial(lit, "M_Tile", "FFFFFF", 40, true);
            Material legal = EnsureMaterial(lit, "M_TileLegal", "3CDC3C", 160, true);
            Material selected = EnsureMaterial(lit, "M_TileSelected", "FFD700", 180, true);
            Material white = EnsureMaterial(lit, "M_WhitePiece", "EDE6D6", 255, false);
            Material black = EnsureMaterial(lit, "M_BlackPiece", "2B2B2B", 255, false);

            GameObject camGo = EnsureCamera(out int removed);
            Camera cam = camGo.GetComponent<Camera>();
            CameraRig rig = EnsureComponent<CameraRig>(camGo, "CameraRig");

            GameObject root = EnsureRoot();
            BoardView board = EnsureComponent<BoardView>(root, "BoardView");
            PiecesView pieces = EnsureComponent<PiecesView>(root, "PiecesView");
            SquarePicker picker = EnsureComponent<SquarePicker>(root, "SquarePicker");
            PromotionPrompt prompt = EnsureComponent<PromotionPrompt>(root, "PromotionPrompt");
            SelectionController selection = EnsureComponent<SelectionController>(root, "SelectionController");
            StatusHud hud = EnsureComponent<StatusHud>(root, "StatusHud");
            GameController game = EnsureComponent<GameController>(root, "GameController");

            using (var so = new SerializedObject(rig))
            {
                SetFloatProp(so, "commanderEyeHeight", 7.0f);
                SetFloatProp(so, "commanderBackOffset", 4.0f);
                SetFloatProp(so, "commanderPitch", 40f);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUi.EnsureEventSystem(Created);
            BuildHudCanvas(font, hud, prompt);

            using (var so = new SerializedObject(board))
            {
                SetRef(so, "groundMaterial", ground);
                SetRef(so, "tileMaterial", tile);
                SetRef(so, "legalTargetMaterial", legal);
                SetRef(so, "selectedMaterial", selected);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            using (var so = new SerializedObject(pieces))
            {
                SetRef(so, "whiteMaterial", white);
                SetRef(so, "blackMaterial", black);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            using (var so = new SerializedObject(picker))
            {
                SetRef(so, "raycastCamera", cam);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            using (var so = new SerializedObject(selection))
            {
                SetRef(so, "boardView", board);
                SetRef(so, "picker", picker);
                SetRef(so, "promotionPrompt", prompt);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            using (var so = new SerializedObject(game))
            {
                SetRef(so, "boardView", board);
                SetRef(so, "piecesView", pieces);
                SetRef(so, "selection", selection);
                SetRef(so, "cameraRig", rig);
                SetRef(so, "hud", hud);
                SetBool(so, "playAgainstAI", true);
                SetEnum(so, "difficulty", DifficultyPreset.Easy);
                SetEnum(so, "humanSide", Side.White);
                SetString(so, "stockfishRelativePath", "Stockfish/stockfish.exe");
                SetString(so, "stockfishPathOverride", "");
                SetString(so, "startFen", "");
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(board);
            EditorUtility.SetDirty(pieces);
            EditorUtility.SetDirty(picker);
            EditorUtility.SetDirty(selection);
            EditorUtility.SetDirty(game);
            EditorUtility.SetDirty(rig);
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(prompt);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            string msg = Tag + "Done" + (failed ? " WITH ERRORS (see above)" : "") + ".\n"
                + "Created: " + (Created.Count == 0 ? "-" : string.Join(", ", Created)) + ";\n"
                + "Updated: " + (Updated.Count == 0 ? "-" : string.Join(", ", Updated)) + ";\n"
                + "Removed missing scripts: " + removed;
            Debug.Log(msg);
        }

        private static void BuildHudCanvas(TMP_FontAsset font, StatusHud hud, PromotionPrompt prompt)
        {
            EditorUi.DeleteRoots("HudCanvas", Updated);
            GameObject canvas = EditorUi.NewCanvas("HudCanvas", 20);
            Created.Add("HudCanvas");

            // HUD: no raycast-blocking graphic, so board clicks pass through.
            GameObject hudPanel = EditorUi.NewRect("HudPanel", canvas.transform,
                EditorUi.TopLeft(new Vector2(24f, -110f), new Vector2(960f, 220f)));
            var layout = hudPanel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var line = new Vector2(960f, 40f);
            TextMeshProUGUI status = EditorUi.MakeText("StatusText", hudPanel.transform, font, "", 30f,
                EditorUi.TopLeft(Vector2.zero, line), TextAlignmentOptions.TopLeft, Color.white, true);
            TextMeshProUGUI error = EditorUi.MakeText("ErrorText", hudPanel.transform, font, "", 24f,
                EditorUi.TopLeft(Vector2.zero, line), TextAlignmentOptions.TopLeft, EditorUi.ErrorColor);
            error.gameObject.SetActive(false);
            EditorUi.MakeText("HelpText", hudPanel.transform, font, GameText.HelpLine, 28f,
                EditorUi.TopLeft(Vector2.zero, line), TextAlignmentOptions.TopLeft, EditorUi.HelpColor);

            GameObject promo = EditorUi.NewPanel("PromotionPanel", canvas.transform, new Vector2(480f, 560f));
            ((RectTransform)promo.transform).anchoredPosition = new Vector2(0f, -90f);
            TextMeshProUGUI title = EditorUi.MakeText("TitleText", promo.transform, font, GameText.PromotionTitle(Side.White), 32f,
                EditorUi.Center(new Vector2(0f, 220f), new Vector2(440f, 60f)), TextAlignmentOptions.Center, Color.white, true, 24f);
            Button queen = PromoButton(promo, font, "QueenButton", GameText.PieceName(PieceType.Queen), 130f);
            Button rook = PromoButton(promo, font, "RookButton", GameText.PieceName(PieceType.Rook), 50f);
            Button bishop = PromoButton(promo, font, "BishopButton", GameText.PieceName(PieceType.Bishop), -30f);
            Button knight = PromoButton(promo, font, "KnightButton", GameText.PieceName(PieceType.Knight), -110f);
            Button cancel = PromoButton(promo, font, "CancelButton", TextTable.Active.Get(TextId.Cancel), -200f);
            promo.SetActive(false);

            using (var so = new SerializedObject(hud))
            {
                SetRef(so, "statusText", status);
                SetRef(so, "errorText", error);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            using (var so = new SerializedObject(prompt))
            {
                SetRef(so, "panel", promo);
                SetRef(so, "titleText", title);
                SetRef(so, "queenButton", queen);
                SetRef(so, "rookButton", rook);
                SetRef(so, "bishopButton", bishop);
                SetRef(so, "knightButton", knight);
                SetRef(so, "cancelButton", cancel);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Button PromoButton(GameObject parent, TMP_FontAsset font, string name, string label, float y)
        {
            return EditorUi.MakeButton(name, parent.transform, font, label,
                EditorUi.Center(new Vector2(0f, y), new Vector2(400f, 70f)), 30f, out TMP_Text _);
        }

        private static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(MaterialsFolder)) return;
            AssetDatabase.CreateFolder("Assets", "Materials");
            Created.Add("folder " + MaterialsFolder);
        }

        private static Material EnsureMaterial(Shader shader, string name, string hex, int alpha255, bool transparent)
        {
            string path = MaterialsFolder + "/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(m, path);
                Created.Add(name);
            }
            else
            {
                Updated.Add(name);
            }

            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            c.a = alpha255 / 255f;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            m.color = c;

            if (transparent)
            {
                SetFloat(m, "_Surface", 1f);
                SetFloat(m, "_Blend", 0f);
                SetFloat(m, "_SrcBlend", (float)BlendMode.SrcAlpha);
                SetFloat(m, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                SetFloat(m, "_SrcBlendAlpha", (float)BlendMode.One);
                SetFloat(m, "_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                SetFloat(m, "_ZWrite", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.SetOverrideTag("RenderType", "Transparent");
                m.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                SetFloat(m, "_Surface", 0f);
                SetFloat(m, "_SrcBlend", (float)BlendMode.One);
                SetFloat(m, "_DstBlend", (float)BlendMode.Zero);
                SetFloat(m, "_ZWrite", 1f);
                m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.SetOverrideTag("RenderType", "Opaque");
                m.renderQueue = -1;
            }
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void SetFloat(Material m, string prop, float v)
        {
            if (m.HasProperty(prop)) m.SetFloat(prop, v);
        }

        private static GameObject EnsureCamera(out int removed)
        {
            GameObject go = GameObject.FindWithTag("MainCamera");
            if (go == null)
            {
                foreach (GameObject r in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (r.name == "Main Camera") { go = r; break; }
                }
            }
            if (go == null)
            {
                go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
                Created.Add("Main Camera");
            }
            else
            {
                Updated.Add("Main Camera");
            }
            if (go.GetComponent<Camera>() == null) go.AddComponent<Camera>();
            removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            return go;
        }

        private static GameObject EnsureRoot()
        {
            foreach (GameObject r in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (r.name == "ChessGame")
                {
                    Updated.Add("ChessGame");
                    return r;
                }
            }
            var go = new GameObject("ChessGame");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.transform.localScale = Vector3.one;
            Created.Add("ChessGame");
            return go;
        }

        private static T EnsureComponent<T>(GameObject go, string label) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c != null)
            {
                Updated.Add(label);
                return c;
            }
            Created.Add(label);
            return go.AddComponent<T>();
        }

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

        private static void SetBool(SerializedObject so, string name, bool value)
        {
            SerializedProperty p = Find(so, name);
            if (p != null) p.boolValue = value;
        }

        private static void SetFloatProp(SerializedObject so, string name, float value)
        {
            SerializedProperty p = Find(so, name);
            if (p != null) p.floatValue = value;
        }

        private static void SetString(SerializedObject so, string name, string value)
        {
            SerializedProperty p = Find(so, name);
            if (p != null) p.stringValue = value;
        }

        // Enum index = position in declaration order (works for byte-backed enums too).
        private static void SetEnum<TEnum>(SerializedObject so, string name, TEnum value) where TEnum : struct, Enum
        {
            SerializedProperty p = Find(so, name);
            if (p == null) return;
            int index = Array.IndexOf(Enum.GetValues(typeof(TEnum)), value);
            if (index < 0 || index >= p.enumNames.Length)
            {
                failed = true;
                Debug.LogError(Tag + "Enum value " + value + " not valid for '" + name + "'");
                return;
            }
            p.enumValueIndex = index;
        }
    }
}
