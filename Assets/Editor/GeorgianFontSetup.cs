using System;
using System.Collections.Generic;
using Chess.Campaign;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Chess.Editor
{
    /// <summary>
    /// Tools -> Setup Georgian Font: creates the static NotoSansGeorgian SDF font asset (once) and checks
    /// that every UI text character is covered by it or its fallback.
    /// </summary>
    public static class GeorgianFontSetup
    {
        public const string FontPath = "Assets/Fonts/NotoSansGeorgian-Regular.ttf";
        public const string AssetPath = "Assets/Fonts/NotoSansGeorgian SDF.asset";
        public const string FallbackPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private const string Tag = "[GeorgianFont] ";

        [MenuItem("Tools/Setup Georgian Font")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(Tag + "Cannot run during Play mode.");
                return;
            }
            Ensure(out _);
        }

        public static TMP_FontAsset Ensure(out bool charactersMissing)
        {
            charactersMissing = false;

            // 1. TMP Essentials
            if (Resources.Load<TMP_Settings>("TMP Settings") == null || Shader.Find("TextMeshPro/Mobile/Distance Field") == null)
            {
                Debug.LogError(Tag + "TMP Essential Resources are not imported. Use Window > TextMeshPro > Import TMP Essential Resources, then run the command again.");
                return null;
            }

            // 2. Existing asset is reused untouched when complete.
            TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            if (asset != null)
            {
                bool complete = asset.atlasTextures != null && asset.atlasTextures.Length > 0
                    && asset.atlasTextures[0] != null && asset.material != null;
                if (complete)
                {
                    Debug.Log(Tag + "Using existing " + AssetPath + ".");
                    charactersMissing = !CheckCoverage(asset);
                    return asset;
                }
                Debug.LogWarning(Tag + "Existing font asset is incomplete and was recreated: " + AssetPath);
                AssetDatabase.DeleteAsset(AssetPath);
            }

            // 3. Source TTF
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogError(Tag + "Font file not found: Assets/Fonts/NotoSansGeorgian-Regular.ttf. Download Noto Sans Georgian (PROGRESS.md, USER task 44).");
                return null;
            }

            // 4. Create
            asset = TMP_FontAsset.CreateFontAsset(font, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
            if (asset == null)
            {
                Debug.LogError(Tag + "Could not create the font asset from " + FontPath + ".");
                return null;
            }
            asset.name = "NotoSansGeorgian SDF";
            AssetDatabase.CreateAsset(asset, AssetPath);

            // Sub-assets: without them the asset would lose its atlas and material. TryAddCharacters
            // reinitializes the same texture object in place (1x1 -> 1024x1024), so adding them first is safe.
            Texture2D atlas = asset.atlasTextures[0];
            atlas.name = "NotoSansGeorgian SDF Atlas";
            AssetDatabase.AddObjectToAsset(atlas, asset);
            asset.material.name = "NotoSansGeorgian SDF Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);

            string atlasCharacters = TextTable.AtlasCharacters;
            asset.TryAddCharacters(atlasCharacters, out string notAdded);
            notAdded = notAdded ?? "";
            if (notAdded.Length > 0)
            {
                var codes = new List<string>();
                foreach (char c in notAdded) codes.Add(Code(c));
                Debug.LogWarning(Tag + "Not added to the atlas: " + string.Join(", ", codes));
            }

            asset.atlasPopulationMode = AtlasPopulationMode.Static;

            TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackPath);
            if (fallback != null) asset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
            else Debug.LogWarning(Tag + "Fallback LiberationSans SDF not found.");

            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(atlas);
            EditorUtility.SetDirty(asset.material);
            AssetDatabase.SaveAssets();
            int added = atlasCharacters.Length - notAdded.Length;
            Debug.Log(Tag + "Created " + AssetPath + ": " + added + " of " + atlasCharacters.Length + " atlas characters.");

            charactersMissing = !CheckCoverage(asset);
            return asset;
        }

        // Returns true when every UI text character is available in the font or its fallback.
        private static bool CheckCoverage(TMP_FontAsset asset)
        {
            var missingAll = new SortedSet<uint>();
            var primaryMissing = new SortedSet<char>();
            foreach (TextId id in Enum.GetValues(typeof(TextId)))
            {
                string text = TextTable.Active.Get(id);
                if (string.IsNullOrEmpty(text)) continue;
                text = text.Replace("\n", "");
                if (text.Length == 0) continue;

                if (!asset.HasCharacters(text, out uint[] missing, true) && missing != null)
                {
                    foreach (uint u in missing) missingAll.Add(u);
                }
                if (!asset.HasCharacters(text, out List<char> fromFallback) && fromFallback != null)
                {
                    foreach (char c in fromFallback) primaryMissing.Add(c);
                }
            }

            bool ok = missingAll.Count == 0;
            if (!ok)
            {
                var codes = new List<string>();
                foreach (uint u in missingAll) codes.Add(Code(u));
                Debug.LogError(Tag + "UI characters missing from the font and its fallback: " + string.Join(", ", codes));
            }

            var usedFallback = new List<string>();
            foreach (char c in primaryMissing)
            {
                if (!missingAll.Contains(c)) usedFallback.Add(c.ToString());
            }
            if (usedFallback.Count > 0)
            {
                Debug.Log(Tag + "UI characters taken from fallback LiberationSans SDF: " + string.Join(", ", usedFallback));
            }

            if (ok) Debug.Log(Tag + "All UI text characters are covered.");
            return ok;
        }

        private static string Code(uint codePoint)
        {
            return "U+" + codePoint.ToString("X4");
        }
    }
}
