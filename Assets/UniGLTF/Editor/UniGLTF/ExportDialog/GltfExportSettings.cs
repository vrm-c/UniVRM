

using UnityEngine;

namespace UniGLTF
{
    public class GltfExportSettings : ScriptableObject
    {
        public GameObject Root { get; set; }

        public Axes InverseAxis;

        [Header("MorphTarget(BlendShape)")]
        public bool Sparse;

        public bool DropNormal;

        public bool DivideVertexBuffer;

        public MeshExportSettings MeshExportSettings => new MeshExportSettings
        {
            UseSparseAccessorForMorphTarget = Sparse,
            ExportOnlyBlendShapePosition = DropNormal,
            DivideVertexBuffer = DivideVertexBuffer,
        };
    }
}
