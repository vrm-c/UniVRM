#if MTOON_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VRM10.MToon10
{
    public sealed class MToonOutlineRenderFeature : ScriptableRendererFeature
    {
        private MToonOutlineRenderPass _opaquePass;
        private MToonOutlineRenderPass _transparentPass;

        public override void Create()
        {
            _opaquePass = new MToonOutlineRenderPass(RenderPassEvent.AfterRenderingOpaques, RenderQueueRange.opaque);
            _transparentPass = new MToonOutlineRenderPass(RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_opaquePass);
            renderer.EnqueuePass(_transparentPass);
        }
    }
}
#endif