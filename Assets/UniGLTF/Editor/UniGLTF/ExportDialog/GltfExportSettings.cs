

using UnityEngine;

namespace UniGLTF
{
    public class GltfExportSettings : ScriptableObject
    {
        public GameObject Root { get; set; }

        public Axises InverseAxis;

        [Header("MorphTarget(BlendShape)")]
        public bool Sparse;

        public bool DropNormal;

        public bool DivideVertexBuffer;
    }
}
