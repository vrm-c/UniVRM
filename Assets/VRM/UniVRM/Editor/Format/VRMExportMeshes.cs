using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// Export時にMeshを一覧する。
    /// 
    /// Mesh関連の Validation する。
    /// Meshのエクスポートサイズを試算する。
    /// </summary>
    [Serializable]
    public class VRMExportMeshes : ScriptableObject
    {
        static Mesh GetMesh(Renderer r)
        {
            if (r is SkinnedMeshRenderer smr)
            {
                return smr.sharedMesh;
            }
            if (r is MeshRenderer)
            {
                MeshFilter f = r.GetComponent<MeshFilter>();
                if (f != null)
                {
                    return f.sharedMesh;
                }
            }
            return null;
        }

        static bool ClipsContainsName(IReadOnlyList<BlendShapeClip> clips, bool onlyPreset, BlendShapeBinding binding)
        {
            foreach (var c in clips)
            {
                if (onlyPreset)
                {
                    if (c.Preset == BlendShapePreset.Unknown)
                    {
                        continue;
                    }
                }

                foreach (var b in c.Values)
                {
                    if (b.RelativePath == binding.RelativePath && b.Index == binding.Index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [Serializable]
        public struct MeshInfo
        {
            public Renderer Renderer;
            public Mesh Mesh;
            public bool IsRendererActive;
            public bool Skinned;

            public enum VertexColorState
            {
                // 存在しない
                None,
                // 存在して使用している
                ExistsAndIsUsed,
                // 存在するが使用していない
                ExistsButNotUsed,
            }
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

            public void CalcMeshSize(string relativePath, VRMExportSettings settings, IReadOnlyList<BlendShapeClip> clips)
            {
                VertexCount = Mesh.vertexCount;
                ExportVertexSize = 0;
                TotalBlendShapeCount = 0;
                ExportBlendShapeCount = 0;

                // float4 x 3
                // vertices
                if (Mesh.normals != null)
                {
                    ExportVertexSize += 4 * 3;
                }
                if (Mesh.uv != null)
                {
                    ExportVertexSize += 4 * 2;
                }
                if (Mesh.colors != null)
                {
                    ExportVertexSize += 4 * 4;
                }
                if (Mesh.boneWeights != null)
                {
                    // short, float x 4 weights
                    ExportVertexSize += (2 + 4) * 4;
                }
                // indices
                IndexCount = Mesh.triangles.Length;

                // postion + normal ?. always tangent is ignored
                TotalBlendShapeCount = Mesh.blendShapeCount;
                ExportBlendShapeVertexSize = settings.OnlyBlendshapePosition ? 4 * 3 : 4 * (3 + 3);
                for (var i = 0; i < Mesh.blendShapeCount; ++i)
                {
                    // var name = Mesh.GetBlendShapeName(i);
                    if (settings.ReduceBlendshape)
                    {
                        if (!ClipsContainsName(clips, settings.ReduceBlendshapeClip, new BlendShapeBinding
                        {
                            Index = i,
                            RelativePath = relativePath,
                        }))
                        {
                            // skip
                            continue;
                        }
                    }

                    ++ExportBlendShapeCount;
                }
            }
        }

        public List<MeshInfo> Meshes = new List<MeshInfo>();

        public int ExpectedExportByteSize => Meshes.Where(x => x.IsRendererActive).Sum(x => x.ExportByteSize);

        List<Validation> m_validations = new List<Validation>();
        public IEnumerable<Validation> Validations => m_validations;

        static bool MaterialUseVertexColor(Material m)
        {
            return UniGLTF.UniUnlit.Utils.GetVColBlendMode(m) == UniGLTF.UniUnlit.UniUnlitVertexColorBlendOp.Multiply;
        }

        bool TryGetMeshInfo(GameObject root, Renderer renderer, IReadOnlyList<BlendShapeClip> clips, VRMExportSettings settings, out MeshInfo info)
        {
            info = default;
            if (root == null)
            {
                return false;
            }
            if (renderer == null)
            {
                return false;
            }
            info.Renderer = renderer;

            if (renderer is SkinnedMeshRenderer smr)
            {
                info.Skinned = true;
                info.Mesh = smr.sharedMesh;
                info.IsRendererActive = smr.EnableForExport();
            }
            else if (renderer is MeshRenderer mr)
            {
                var filter = mr.GetComponent<MeshFilter>();
                if (filter != null)
                {
                    info.Mesh = filter.sharedMesh;
                }
                info.IsRendererActive = mr.EnableForExport();
            }
            else
            {
                return false;
            }

            var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, root.transform);
            info.CalcMeshSize(relativePath, settings, clips);

            if (info.Mesh.colors != null && info.Mesh.colors.Length == info.Mesh.vertexCount)
            {
                if (renderer.sharedMaterials.Any(x => MaterialUseVertexColor(x)))
                {
                    info.VertexColor = MeshInfo.VertexColorState.ExistsAndIsUsed;
                }
                else
                {
                    info.VertexColor = MeshInfo.VertexColorState.ExistsButNotUsed;
                }
            }

            return true;
        }

        public void SetRoot(GameObject ExportRoot, VRMExportSettings settings)
        {
            m_validations.Clear();
            Meshes.Clear();
            if (ExportRoot == null)
            {
                return;
            }

            var clips = new List<BlendShapeClip>();
            var proxy = ExportRoot.GetComponent<VRMBlendShapeProxy>();
            if (proxy != null)
            {
                // Export サイズ の 計算
                if (proxy.BlendShapeAvatar != null)
                {
                    clips.AddRange(proxy.BlendShapeAvatar.Clips);
                }
            }

            foreach (var renderer in ExportRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (TryGetMeshInfo(ExportRoot, renderer, clips, settings, out MeshInfo info))
                {
                    Meshes.Add(info);
                }
            }
        }
    }
}
