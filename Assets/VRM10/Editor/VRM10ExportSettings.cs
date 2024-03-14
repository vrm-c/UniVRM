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
        [Tooltip("not implemented yet. Remove blendshape that is not used from BlendShapeClip")]
        [ReadOnly]
        public bool ReduceBlendshape = false;

        /// <summary>
        /// skip if BlendShapeClip.Preset == Unknown
        /// </summary>
        [Tooltip("not implemented yet. Remove blendShapeClip that preset is Unknown")]
        [ReadOnly]
        public bool ReduceBlendshapeClip = false;

        [Tooltip("Use sparse accessor for morph target")]
        public bool MorphTargetUseSparse = true;

        /// <summary>
        /// FreezeBlendShape
        /// </summary>
        [Tooltip("freeze rotation and scale and blendshape")]
        public bool FreezeMesh = false;

        public GltfExportSettings MeshExportSettings => new GltfExportSettings
        {
            UseSparseAccessorForMorphTarget = MorphTargetUseSparse,
            ExportOnlyBlendShapePosition = true,
            DivideVertexBuffer = true,
        };

        public GameObject Root { get; set; }
    }
}
