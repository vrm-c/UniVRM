using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF
{
    public interface IAnimationImporter
    {
        List<AnimationClip> Import(glTF gltf, GameObject root, List<Transform> nodes, List<AnimationClip> clips, Axises invertAxis);
    }
}
