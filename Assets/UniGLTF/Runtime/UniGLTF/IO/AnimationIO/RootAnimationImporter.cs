using UnityEngine;

namespace UniGLTF
{
    public sealed class RootAnimationImporter : IAnimationImporter
    {
        public AnimationClip Import(glTF gltf, int i, Axes invertAxis)
        {
            return AnimationImporterUtil.ConvertAnimationClip(gltf, gltf.animations[i], invertAxis.Create());
        }
    }
}
