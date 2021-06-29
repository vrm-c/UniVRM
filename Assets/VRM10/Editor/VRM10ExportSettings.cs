using System;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public class VRM10ExportSettings : ScriptableObject
    {
        /// <summary>
        /// エクスポート時にBlendShapeClipから参照されないBlendShapeを削除する
        /// </summary>
        [Tooltip("Remove blendshape that is not used from BlendShapeClip")]
        public bool ReduceBlendshape = false;

        /// <summary>
        /// skip if BlendShapeClip.Preset == Unknown
        /// </summary>
        [Tooltip("Remove blendShapeClip that preset is Unknown")]
        public bool ReduceBlendshapeClip = false;

        public GltfExportSettings MeshExportSettings => new GltfExportSettings
        {
            UseSparseAccessorForMorphTarget = true,
            ExportOnlyBlendShapePosition = true,
            DivideVertexBuffer = true,
        };

        public GameObject Root { get; set; }
    }
}
