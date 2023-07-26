using UniGLTF;
using UnityEngine;

namespace VRM10.Settings
{
    public abstract class MaterialDescriptorGeneratorFactory : ScriptableObject
    {
        public abstract IMaterialDescriptorGenerator Create();
    }
}