using System;
using UnityEngine;

namespace UniGLTF
{
    [Serializable]
    public struct MeshExportInfo
    {
        public Renderer Renderer;
        public Mesh Mesh;
        public bool IsRendererActive;
        public bool Skinned;

        public bool HasNormal => Mesh != null && Mesh.normals != null && Mesh.normals.Length == Mesh.vertexCount;
        public bool HasUV => Mesh != null && Mesh.uv != null && Mesh.uv.Length == Mesh.vertexCount;

        public bool HasVertexColor => Mesh.colors != null && Mesh.colors.Length == Mesh.vertexCount
            && VertexColor == VertexColorState.ExistsAndIsUsed
            || VertexColor == VertexColorState.ExistsAndMixed // Export する
            ;

        public bool HasSkinning => Mesh.boneWeights != null && Mesh.boneWeights.Length == Mesh.vertexCount;

        public VertexColorState VertexColor;

        public int VertexCount;

        /// <summary>
        /// Position, UV, Normal
        /// [Color]
        /// [SkinningWeight]
        /// </summary>
        public int ExportVertexSize;

        public int IndexCount;

        // int 決め打ち
        public int IndicesSize => IndexCount * 4;

        public int ExportBlendShapeVertexSize;

        public int TotalBlendShapeCount;

        public int ExportBlendShapeCount;

        public int ExportByteSize => ExportVertexSize * VertexCount + IndicesSize + ExportBlendShapeCount * ExportBlendShapeVertexSize * VertexCount;

        public string Summary;
    }
}
