

using System;
using UnityEngine;

namespace UniGLTF
{
    [Serializable]
    public class GltfExportSettings
    {
        public Axes InverseAxis = Axes.Z;

        /// <summary>
        /// https://github.com/vrm-c/UniVRM/issues/800
        ///
        /// VertexBuffer を共有バッファ方式にする
        /// UniVRM-0.71.0 までの挙動
        /// </summary>
        public bool DivideVertexBuffer;

        /// <summary>
        /// MorphTarget に Sparse Accessor を使う
        /// </summary>
        public bool UseSparseAccessorForMorphTarget;

        /// <summary>
        /// MorphTarget を Position だけにする(normal とか捨てる)
        /// </summary>
        public bool ExportOnlyBlendShapePosition;

        /// <summary>
        /// tangent を出力する
        /// </summary>
        public bool ExportTangents
#if GLTF_EXPORT_TANGENTS
             = true,
#endif            
        ;

        /// <summary>
        /// Keep VertexColor
        /// </summary>
        public bool KeepVertexColor;

        /// <summary>
        /// https://github.com/vrm-c/UniVRM/issues/1582
        /// 
        /// Allowed hide flags for MeshFilters to be exported
        /// </summary>
        public HideFlags MeshFilterAllowedHideFlags = HideFlags.None;
    }
}
