using UnityEngine;

namespace UniGLTF
{
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

    public static class VertexColorUtility
    {
        static bool MaterialUseVertexColor(Material m)
        {
            if (m == null)
            {
                return false;
            }
            if (m.shader.name != UniGLTF.UniUnlit.UniUnlitUtil.ShaderName)
            {
                return false;
            }
            if (UniGLTF.UniUnlit.UniUnlitUtil.GetVColBlendMode(m) != UniGLTF.UniUnlit.UniUnlitVertexColorBlendOp.Multiply)
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
                            ? VertexColorState.ExistsAndIsUsed
                            : VertexColorState.ExistsButNotUsed
                            ;
                        if (state.HasValue)
                        {
                            if (state.Value != currentState)
                            {
                                state = VertexColorState.ExistsAndMixed;
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
    }
}
