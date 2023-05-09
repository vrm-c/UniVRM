#if MTOON_URP
using UnityEngine.Rendering.Universal;

namespace VRMShaders.VRM10.MToon10.Runtime
{
    public sealed class MToonOutlineRenderFeature : ScriptableRendererFeature
    {
        private MToonOutlineRenderPass _pass;

        public override void Create()
        {
            _pass = new MToonOutlineRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }
    }
}
#endif