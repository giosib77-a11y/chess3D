using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Chess.Editor
{
    /// <summary>
    /// Shared uGUI/TMP builders used by the scene setup commands.
    /// </summary>
    internal static class EditorUi
    {
        internal static readonly Color Background = Hex("1C1C22", 1f);
        internal static readonly Color PanelColor = Hex("2A2A33", 0.95f);
        internal static readonly Color MapColor = Hex("2F3A2A", 1f);
        internal static readonly Color ButtonColor = Hex("3A4A6B", 1f);
        internal static readonly Color WarningColor = Hex("FFB347", 1f);
        internal static readonly Color DisabledColor = Hex("5A5A5A", 0.5f);
        internal static readonly Color LockedColor = Hex("5A5A5A", 1f);
        internal static readonly Color ErrorColor = Hex("FFD933", 1f);
        internal static readonly Color HelpColor = Hex("D9D9D9", 1f);

        internal readonly struct Placement
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

        internal static Placement Center(Vector2 pos, Vector2 size)
        {
            var half = new Vector2(0.5f, 0.5f);
            return new Placement(half, half, half, pos, size);
        }

        internal static Placement Top(Vector2 pos, Vector2 size)
        {
            return new Placement(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), pos, size);
        }

        internal static Placement Bottom(Vector2 pos, Vector2 size)
        {
            return new Placement(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), pos, size);
        }

        internal static Placement TopLeft(Vector2 pos, Vector2 size)
        {
            return new Placement(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        }

        internal static Placement TopRight(Vector2 pos, Vector2 size)
        {
            return new Placement(Vector2.one, Vector2.one, Vector2.one, pos, size);
        }

        internal static Placement BottomLeft(Vector2 pos, Vector2 size)
        {
            return new Placement(Vector2.zero, Vector2.zero, Vector2.zero, pos, size);
        }

        internal static Placement Anchored(Vector2 anchor, Vector2 size)
        {
            return new Placement(anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        }

        internal static Color Hex(string hex, float alpha)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            c.a = alpha;
            return c;
        }

        internal static GameObject NewCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        internal static GameObject NewRect(string name, Transform parent, Placement p)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = p.AnchorMin;
            rt.anchorMax = p.AnchorMax;
            rt.pivot = p.Pivot;
            rt.anchoredPosition = p.Position;
            rt.sizeDelta = p.Size;
            return go;
        }

        internal static GameObject NewPanel(string name, Transform parent, Vector2 size)
        {
            GameObject go = NewRect(name, parent, Center(Vector2.zero, size));
            go.AddComponent<Image>().color = PanelColor;
            return go;
        }

        internal static TextMeshProUGUI MakeText(string name, Transform parent, TMP_FontAsset font, string text, float fontSize,
            Placement p, TextAlignmentOptions alignment, Color color, bool bold = false, float autoSizeMin = 0f)
        {
            GameObject go = NewRect(name, parent, p);
            return ConfigureText(go.AddComponent<TextMeshProUGUI>(), font, text, fontSize, alignment, color, bold, autoSizeMin);
        }

        private static TextMeshProUGUI ConfigureText(TextMeshProUGUI t, TMP_FontAsset font, string text, float fontSize,
            TextAlignmentOptions alignment, Color color, bool bold, float autoSizeMin)
        {
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = color;
            t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            t.parseCtrlCharacters = false;
            t.richText = false;
            if (autoSizeMin > 0f)
            {
                t.enableAutoSizing = true;
                t.fontSizeMin = autoSizeMin;
                t.fontSizeMax = fontSize;
            }
            return t;
        }

        internal static Button MakeButton(string name, Transform parent, TMP_FontAsset font, string label, Placement p,
            float fontSize, out TMP_Text labelText)
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

            var stretch = new Placement(Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            labelText = MakeText("Label", go.transform, font, label, fontSize, stretch, TextAlignmentOptions.Center, Color.white,
                false, 20f);
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            labelText.rectTransform.offsetMin = new Vector2(24f, 4f);
            labelText.rectTransform.offsetMax = new Vector2(-24f, -4f);
            return button;
        }

        internal static void EnsureEventSystem(List<string> created)
        {
            EventSystem[] all = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            EventSystem es = null;
            foreach (EventSystem e in all)
            {
                if (es == null) es = e;
                else Object.DestroyImmediate(e.gameObject);
            }
            if (es == null)
            {
                es = new GameObject("EventSystem").AddComponent<EventSystem>();
                created.Add("EventSystem");
            }
            foreach (StandaloneInputModule old in es.GetComponents<StandaloneInputModule>())
            {
                Object.DestroyImmediate(old);
            }
            var module = es.GetComponent<InputSystemUIInputModule>();
            if (module == null) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        internal static void DeleteRoots(string name, List<string> updated)
        {
            foreach (GameObject r in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (r.name != name) continue;
                Object.DestroyImmediate(r);
                updated.Add(name + " (recreated)");
            }
        }
    }
}
