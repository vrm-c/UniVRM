using System;

namespace UniGLTF
{
    [Serializable]
    public struct MeshExportSettings
    {
        //
        // https://github.com/vrm-c/UniVRM/issues/800
        //
        // VertexBuffer を共有バッファ方式にする
        // UniVRM-0.71.0 までの挙動
        //
        public bool UseSharingVertexBuffer;

        // MorphTarget に Sparse Accessor を使う
        public bool UseSparseAccessorForMorphTarget;

        // MorphTarget を Position だけにする(normal とか捨てる)
        public bool ExportOnlyBlendShapePosition;

        // tangent を出力する
        public bool ExportTangents;

        public static MeshExportSettings Default => new MeshExportSettings
        {
            UseSparseAccessorForMorphTarget = false,
            ExportOnlyBlendShapePosition = false,
            // WIP
            UseSharingVertexBuffer = true,
#if GLTF_EXPORT_TANGENTS
            ExportTangents = true,
#endif            
        };
    }
}
