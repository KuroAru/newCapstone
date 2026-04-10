// SC Post Effects — editor utility (missing from partial import).

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SCPE
{
    public static class AutoSetup
    {
        public static bool ValidEffectSetup<T>() where T : ScriptableRendererFeature
        {
            var urp = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) return false;

            foreach (var rendererData in urp.rendererDataList)
            {
                if (rendererData == null) continue;
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature is T typed && typed.isActive) return true;
                }
            }

            return false;
        }
    }
}
