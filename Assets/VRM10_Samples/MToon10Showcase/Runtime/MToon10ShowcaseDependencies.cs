using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/MToon10ShowcaseDependencies")]
#endif
    public sealed class MToon10ShowcaseDependencies : ScriptableObject
    {
        public Texture2D checkerTexture;
        public Texture2D litCheckerTexture;
        public Texture2D shadeCheckerTexture;

        public Shader mtoon10Shader;
    }
}