// SC Post Effects — inspector UI helpers (missing from partial import).

using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SCPE
{
    public static class SCPE_GUI
    {
        public static void CheckGradientImportSettings(Object asset)
        {
            if (asset == null) return;
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) return;
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;

            bool ok = importer.textureType == TextureImporterType.Default
                      && !importer.isReadable
                      && importer.wrapMode == TextureWrapMode.Clamp;
            if (ok) return;

            EditorGUILayout.HelpBox(
                "For best results use Texture Type: Default, Read/Write Off, Wrap Mode: Clamp.",
                MessageType.Warning);
        }

        public static readonly GUIStyle PathField = EditorStyles.textField;

        private const string DocsBase = "https://staggart.xyz/unity/sc-post-effects/scpe-docs/?section=";

        public static void DisplayDocumentationButton(string sectionSlug)
        {
            if (GUILayout.Button("Documentation", GUILayout.Width(110)))
                Application.OpenURL(DocsBase + sectionSlug);
        }

        public static void DisplaySetupWarning<T>(ref bool isSetup) where T : ScriptableRendererFeature
        {
            DisplaySetupWarning<T>(ref isSetup, true);
        }

        /// <param name="showWarning">If false, only refreshes <paramref name="isSetup"/> (no help box).</param>
        public static void DisplaySetupWarning<T>(ref bool isSetup, bool showWarning) where T : ScriptableRendererFeature
        {
            isSetup = AutoSetup.ValidEffectSetup<T>();
            if (!showWarning || isSetup) return;

            EditorGUILayout.HelpBox(
                $"Add an active {typeof(T).Name} Renderer Feature to your URP Renderer (Forward/Universal Renderer asset).",
                MessageType.Warning);
        }

        public static void DisplayIntensityWarning(SerializedDataParameter parameter)
        {
            if (parameter?.value == null) return;
            var p = parameter.value;
            float v = 1f;
            if (p.propertyType == SerializedPropertyType.Float) v = p.floatValue;
            else if (p.propertyType == SerializedPropertyType.Integer) v = p.intValue;
            else return;
            if (Mathf.Abs(v) > 0.0001f) return;
            EditorGUILayout.HelpBox("Value is zero; the effect may not be visible.", MessageType.Info);
        }

        public static void DisplayTextureOverrideWarning(bool textureOverridden)
        {
            if (!textureOverridden) return;
            EditorGUILayout.HelpBox("A custom texture override is enabled; ensure import settings match the effect requirements.", MessageType.Info);
        }

        public static void DrawSunInfo()
        {
            EditorGUILayout.HelpBox("Sun-based projection uses the scene's main Directional Light.", MessageType.Info);
        }

        public static void ShowDepthTextureWarning()
        {
            ShowDepthTextureWarning(true);
        }

        public static void ShowDepthTextureWarning(bool show)
        {
            if (!show) return;
            EditorGUILayout.HelpBox("This effect may need the depth texture. Enable Depth Texture on your URP Renderer if results look wrong.", MessageType.Info);
        }
    }
}
