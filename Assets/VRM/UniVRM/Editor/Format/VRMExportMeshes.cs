using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeshUtility;
using UniGLTF;
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

        public List<MeshExportInfo> Meshes = new List<MeshExportInfo>();

        public int ExpectedExportByteSize => Meshes.Where(x => x.IsRendererActive).Sum(x => x.ExportByteSize);

        List<Validation> m_validations = new List<Validation>();
        public IEnumerable<Validation> Validations => m_validations;

        public static void CalcMeshSize(ref MeshExportInfo info,
                                        string relativePath, VRMExportSettings settings, IReadOnlyList<BlendShapeClip> clips)
        {
            var sb = new StringBuilder();
            if (!info.IsRendererActive)
            {
                sb.Append("[NotActive]");
            }

            info.VertexCount = info.Mesh.vertexCount;
            info.ExportVertexSize = 0;
            info.TotalBlendShapeCount = 0;
            info.ExportBlendShapeCount = 0;

            // float4 x 3
            // vertices
            sb.Append($"(Pos");
            if (info.HasNormal)
            {
                sb.Append("+Nom");
                info.ExportVertexSize += 4 * 3;
            }
            if (info.HasUV)
            {
                sb.Append("+UV");
                info.ExportVertexSize += 4 * 2;
            }
            if (info.HasVertexColor)
            {
                sb.Append("+Col");
                info.ExportVertexSize += 4 * 4;
            }
            if (info.HasSkinning)
            {
                // short, float x 4 weights
                sb.Append("+Skin");
                info.ExportVertexSize += (2 + 4) * 4;
            }
            // indices
            info.IndexCount = info.Mesh.triangles.Length;

            // postion + normal ?. always tangent is ignored
            info.TotalBlendShapeCount = info.Mesh.blendShapeCount;
            info.ExportBlendShapeVertexSize = settings.OnlyBlendshapePosition ? 4 * 3 : 4 * (3 + 3);
            for (var i = 0; i < info.Mesh.blendShapeCount; ++i)
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

                ++info.ExportBlendShapeCount;
            }

            if (info.ExportBlendShapeCount > 0)
            {
                sb.Append($"+Morph x {info.ExportBlendShapeCount}");
            }
            sb.Append($") x {info.Mesh.vertexCount}");
            switch (info.VertexColor)
            {
                case MeshExportInfo.VertexColorState.ExistsAndIsUsed:
                case MeshExportInfo.VertexColorState.ExistsAndMixed: // エクスポートする
                    sb.Insert(0, "[use vcolor]");
                    break;
                case MeshExportInfo.VertexColorState.ExistsButNotUsed:
                    sb.Insert(0, "[remove vcolor]");
                    break;
            }
            if (info.ExportBlendShapeCount > 0 && !info.HasSkinning)
            {
                sb.Insert(0, "[morph without skin]");
            }

            // total bytes
            sb.Insert(0, $"{info.ExportByteSize:#,0} Bytes = ");
            info.Summary = sb.ToString();
        }

        bool TryGetMeshInfo(GameObject root, Renderer renderer, IReadOnlyList<BlendShapeClip> clips, VRMExportSettings settings, out MeshExportInfo info)
        {
            info = default;
            if (root == null)
            {
                info.Summary = "";
                return false;
            }
            if (renderer == null)
            {
                info.Summary = "no Renderer";
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
                info.Summary = "no Mesh";
                return false;
            }

            info.VertexColor = MeshExportInfo.DetectVertexColor(info.Mesh, info.Renderer.sharedMaterials);

            var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, root.transform);
            CalcMeshSize(ref info, relativePath, settings, clips);

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
                if (TryGetMeshInfo(ExportRoot, renderer, clips, settings, out MeshExportInfo info))
                {
                    Meshes.Add(info);
                }
            }
        }
    }
}
