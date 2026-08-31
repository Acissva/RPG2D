using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace RPG2D.Editor
{
    public static class TextMeshProChineseFontTool
    {
        private const string SourceFontPath = "Assets/TextMesh Pro/Fonts/SourceHanSansSC/OTF/SimplifiedChinese/SourceHanSansSC-Regular.otf";
        private const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/SourceHanSansSC-Regular SDF Dynamic.asset";
        private const int SamplingPointSize = 90;
        private const int AtlasPadding = 9;
        private const int AtlasSize = 4096;

        [MenuItem("RPG2D/Tools/TextMeshPro/Setup SourceHanSansSC Chinese Font")]
        public static void SetupSourceHanSansScChineseFont()
        {
            TMP_FontAsset fontAsset = GetOrCreateFontAsset();
            if (fontAsset == null)
            {
                return;
            }

            ConfigureFontAsset(fontAsset);
            ConfigureTmpSettings(fontAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = fontAsset;
            EditorGUIUtility.PingObject(fontAsset);
            Debug.Log($"TMP Chinese font is ready: {FontAssetPath}", fontAsset);
        }

        [MenuItem("RPG2D/Tools/TextMeshPro/Apply SourceHanSansSC To Selected TMP Texts")]
        public static void ApplySourceHanSansScToSelectedTmpTexts()
        {
            TMP_FontAsset fontAsset = GetOrCreateFontAsset();
            if (fontAsset == null)
            {
                return;
            }

            int changedCount = 0;
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text text in texts)
                {
                    Undo.RecordObject(text, "Apply SourceHanSansSC TMP Font");
                    text.font = fontAsset;
                    EditorUtility.SetDirty(text);
                    changedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Applied SourceHanSansSC TMP font to {changedCount} selected TMP text component(s).", fontAsset);
        }

        private static TMP_FontAsset GetOrCreateFontAsset()
        {
            TMP_FontAsset existingFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existingFontAsset != null)
            {
                return existingFontAsset;
            }

            Font sourceFont = LoadSourceFont();
            if (sourceFont == null)
            {
                return null;
            }

            string outputDirectory = Path.GetDirectoryName(FontAssetPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                SamplingPointSize,
                AtlasPadding,
                GlyphRenderMode.SDFAA,
                AtlasSize,
                AtlasSize,
                AtlasPopulationMode.Dynamic,
                true);

            if (fontAsset == null)
            {
                Debug.LogError($"Failed to create TMP font asset from {SourceFontPath}.");
                return null;
            }

            fontAsset.name = "SourceHanSansSC-Regular SDF Dynamic";
            fontAsset.creationSettings = CreateCreationSettings();

            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
            AddSubAssets(fontAsset);
            EditorUtility.SetDirty(fontAsset);

            return fontAsset;
        }

        private static Font LoadSourceFont()
        {
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (sourceFont != null)
            {
                return sourceFont;
            }

            AssetDatabase.ImportAsset(SourceFontPath, ImportAssetOptions.ForceUpdate);
            sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (sourceFont == null)
            {
                Debug.LogError($"Source font not found or not imported as a Font asset: {SourceFontPath}");
            }

            return sourceFont;
        }

        private static void ConfigureFontAsset(TMP_FontAsset fontAsset)
        {
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.isMultiAtlasTexturesEnabled = true;

            if (fontAsset.fallbackFontAssetTable == null)
            {
                fontAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            }

            if (fontAsset.atlasWidth != AtlasSize || fontAsset.atlasHeight != AtlasSize)
            {
                Debug.LogWarning(
                    $"Existing font asset uses {fontAsset.atlasWidth}x{fontAsset.atlasHeight}. " +
                    $"For best Chinese support, delete {FontAssetPath} and run this menu again to recreate it as {AtlasSize}x{AtlasSize}.",
                    fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
        }

        private static void ConfigureTmpSettings(TMP_FontAsset fontAsset)
        {
            TMP_Settings settings = TMP_Settings.GetSettings();
            if (settings == null)
            {
                Debug.LogWarning("TMP Settings asset was not found. Import TextMesh Pro Essential Resources first.");
                return;
            }

            SerializedObject serializedSettings = new SerializedObject(settings);

            SerializedProperty defaultFontAsset = serializedSettings.FindProperty("m_defaultFontAsset");
            if (defaultFontAsset != null)
            {
                defaultFontAsset.objectReferenceValue = fontAsset;
            }

            SerializedProperty fallbackFontAssets = serializedSettings.FindProperty("m_fallbackFontAssets");
            if (fallbackFontAssets != null && !ContainsObjectReference(fallbackFontAssets, fontAsset))
            {
                int index = fallbackFontAssets.arraySize;
                fallbackFontAssets.InsertArrayElementAtIndex(index);
                fallbackFontAssets.GetArrayElementAtIndex(index).objectReferenceValue = fontAsset;
            }

            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        private static bool ContainsObjectReference(SerializedProperty arrayProperty, Object target)
        {
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue == target)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddSubAssets(TMP_FontAsset fontAsset)
        {
            if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
            {
                fontAsset.atlasTextures[0].name = "SourceHanSansSC-Regular Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.name = "SourceHanSansSC-Regular Atlas Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }
        }

        private static FontAssetCreationSettings CreateCreationSettings()
        {
            return new FontAssetCreationSettings
            {
                sourceFontFileName = Path.GetFileName(SourceFontPath),
                sourceFontFileGUID = AssetDatabase.AssetPathToGUID(SourceFontPath),
                pointSizeSamplingMode = 0,
                pointSize = SamplingPointSize,
                padding = AtlasPadding,
                packingMode = 0,
                atlasWidth = AtlasSize,
                atlasHeight = AtlasSize,
                characterSetSelectionMode = 7,
                characterSequence = string.Empty,
                referencedFontAssetGUID = string.Empty,
                referencedTextAssetGUID = string.Empty,
                fontStyle = 0,
                fontStyleModifier = 0,
                renderMode = (int)GlyphRenderMode.SDFAA,
                includeFontFeatures = true
            };
        }
    }
}
