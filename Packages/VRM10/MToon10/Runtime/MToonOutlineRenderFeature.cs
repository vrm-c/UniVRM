#if MTOON_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VRM10.MToon10
{
    public sealed class MToonOutlineRenderFeature : ScriptableRendererFeature
    {
#if UNITY_6000_0_OR_NEWER
        private RenderObjectsPass _opaquePass;
        private RenderObjectsPass _transparentPass;
#else
        private MToonOutlineRenderPass _opaquePass;
        private MToonOutlineRenderPass _transparentPass;
#endif

        public override void Create()
        {
#if UNITY_6000_0_OR_NEWER
            var profilerTagName = nameof(MToonOutlineRenderFeature);
            var shaderTags = new[] {"MToonOutline"};
            var layerMask = -1;
            var cameraSettings = new RenderObjects.CustomCameraSettings();
            _opaquePass = new RenderObjectsPass(profilerTagName, RenderPassEvent.AfterRenderingOpaques, shaderTags, RenderQueueType.Opaque, layerMask, cameraSettings);
            _transparentPass = new RenderObjectsPass(profilerTagName, RenderPassEvent.BeforeRenderingTransparents, shaderTags, RenderQueueType.Transparent, layerMask, cameraSettings);
#else
            _opaquePass = new MToonOutlineRenderPass(RenderPassEvent.AfterRenderingOpaques, RenderQueueRange.opaque);
            _transparentPass = new MToonOutlineRenderPass(RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent);
#endif
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_opaquePass);
            renderer.EnqueuePass(_transparentPass);
        }
    }
}
#endif