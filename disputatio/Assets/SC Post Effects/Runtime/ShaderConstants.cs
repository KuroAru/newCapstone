// SC Post Effects — shader / property name constants (restores missing generated definitions).
// Values are aligned with *.shader headers and com.unity.render-pipelines.core Blit.hlsl.

using UnityEngine;

namespace SCPE
{
    internal static class ShaderNames
    {
        public const string PREFIX = "Hidden/SC Post Effects/";

        public const string AO2D = PREFIX + "Ambient Occlusion 2D";
        public const string BlackBars = PREFIX + "Black Bars";
        public const string Blur = PREFIX + "Blur";
        public const string Caustics = PREFIX + "Caustics";
        public const string CloudShadows = PREFIX + "Cloud Shadows";
        public const string ColorSplit = PREFIX + "Color Split";
        public const string Colorize = PREFIX + "Colorize";
        public const string Danger = PREFIX + "Danger";
        public const string DepthNormals = PREFIX + "DepthNormals";
        public const string Dithering = PREFIX + "Dithering";
        public const string DoubleVision = PREFIX + "Double Vision";
        public const string EdgeDetection = PREFIX + "Edge Detection";
        public const string Fog = PREFIX + "Fog";
        public const string Gradient = PREFIX + "Gradient";
        public const string HueShift3D = PREFIX + "3D Hue Shift";
        public const string Kaleidoscope = PREFIX + "Kaleidoscope";
        public const string Kuwahara = PREFIX + "Kuwahara";
        public const string LensFlares = PREFIX + "Lensflares";
        public const string LightStreaks = PREFIX + "Light Streaks";
        public const string LUT = PREFIX + "LUT";
        public const string Mosaic = PREFIX + "Mosaic";
        public const string Overlay = PREFIX + "Overlay";
        public const string Pixelize = PREFIX + "Pixelize";
        public const string Posterize = PREFIX + "Posterize";
        public const string RadialBlur = PREFIX + "Radial Blur";
        public const string Refraction = PREFIX + "Refraction";
        public const string Ripples = PREFIX + "Ripples";
        public const string Scanlines = PREFIX + "Scanlines";
        public const string Sharpen = PREFIX + "Sharpen";
        public const string Sketch = PREFIX + "Sketch";
        public const string SpeedLines = PREFIX + "SpeedLines";
        public const string Sunshafts = PREFIX + "Sun Shafts";
        public const string TiltShift = PREFIX + "Tilt Shift";
        public const string Transition = PREFIX + "Transition";
        public const string TubeDistortion = PREFIX + "Tube Distortion";
    }

    internal static class ShaderParameters
    {
        public static readonly int _DeferredRendering = Shader.PropertyToID("_DeferredRendering");
        public static readonly int _BlitScaleBiasRt = Shader.PropertyToID("_BlitScaleBiasRt");
        public static readonly int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        public static readonly int unity_WorldToLight = Shader.PropertyToID("unity_WorldToLight");
        public static readonly int Params = Shader.PropertyToID("_Params");
        public static readonly int BlurRadius = Shader.PropertyToID("_BlurRadius");
        public static readonly int BlurOffsets = Shader.PropertyToID("_BlurOffsets");
        public static readonly int FadeParams = Shader.PropertyToID("_FadeParams");
    }

    internal static class TextureNames
    {
        public const string Main = "_MainTex";
        public const string Source = "_BlitTexture";
        public const string DepthNormals = "_CameraDepthNormalsTexture";
        public const string FogSkyboxTex = "_SkyboxTex";
    }

    internal static class ShaderKeywords
    {
        public const string ReconstructedDepthNormals = "_RECONSTRUCT_NORMAL";
    }
}
