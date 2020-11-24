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

        /// <summary>
        /// Mesh に頂点カラーが含まれているか。
        /// 含まれている場合にマテリアルは Unlit.VColorMultiply になっているか？
        /// </summary>
        public enum VertexColorState
        {
            // VColorが存在しない
            None,
            // VColorが存在して使用している(UnlitはすべてVColorMultiply)
            ExistsAndIsUsed,
            // VColorが存在するが使用していない(UnlitはすべてVColorNone。もしくはUnlitが存在しない)
            ExistsButNotUsed,
            // VColorが存在して、Unlit.Multiply と Unlit.NotMultiply が混在している。 Unlit.NotMultiply を MToon か Standardに変更した方がよい
            ExistsAndMixed,
        }
        public VertexColorState VertexColor;

        static bool MaterialUseVertexColor(Material m)
        {
            if (m == null)
            {
                return false;
            }
            if (m.shader.name != UniGLTF.UniUnlit.Utils.ShaderName)
            {
                return false;
            }
            if (UniGLTF.UniUnlit.Utils.GetVColBlendMode(m) != UniGLTF.UniUnlit.UniUnlitVertexColorBlendOp.Multiply)
            {
                return false;
            }
            return true;
        }

        public static VertexColorState DetectVertexColor(Mesh mesh, Material[] materials)
        {
            if (mesh != null && mesh.colors != null && mesh.colors.Length == mesh.vertexCount)
            {
                // mesh が 頂点カラーを保持している
                VertexColorState? state = default;
                if (materials != null)
                {
                    foreach (var m in materials)
                    {
                        var currentState = MaterialUseVertexColor(m)
                            ? MeshExportInfo.VertexColorState.ExistsAndIsUsed
                            : MeshExportInfo.VertexColorState.ExistsButNotUsed
                            ;
                        if (state.HasValue)
                        {
                            if (state.Value != currentState)
                            {
                                state = MeshExportInfo.VertexColorState.ExistsAndMixed;
                                break;
                            }
                        }
                        else
                        {
                            state = currentState;
                        }
                    }
                }
                return state.GetValueOrDefault(VertexColorState.None);
            }
            else
            {
                return VertexColorState.None;
            }
        }
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
