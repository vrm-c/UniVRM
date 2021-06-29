

using System;
using UnityEngine;

namespace UniGLTF
{
    [Serializable]
    public class GltfExportSettings
    {
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
