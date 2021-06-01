using System.Collections.Generic;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public interface IAnimationImporter
    {
        AnimationClip Import(glTF gltf, int i, Axes invertAxis);
    }
}
