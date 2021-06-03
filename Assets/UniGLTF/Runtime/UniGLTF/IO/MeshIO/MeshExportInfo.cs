using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UniGLTF
{
    [Serializable]
    public class MeshExportInfo
    {
        #region この２つの組が gltf mesh の Unique なキーとなる
        public Mesh Mesh;
        public Material[] Materials;
        #endregion

        /// <summary>
        /// ひとつの Mesh を複数の Renderer が共有することがありうる
        /// </summary>
        public List<Renderer> Renderers;

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

        MeshExportInfo(Renderer renderer)
        {
            Materials = renderer.sharedMaterials;
            Renderers = new List<Renderer> { renderer };
        }

        /// <summary>
        /// ヒエラルキーからエクスポートする Mesh の情報を収集する
        /// </summary>
        /// <param name="exportRoot"></param>
        /// <param name="list"></param>
        /// <param name="settings"></param>
        /// <param name="blendShapeFilter"> blendShape の export を filtering する </param>
        public static void GetInfo(GameObject exportRoot, List<MeshExportInfo> list, MeshExportSettings settings, IBlendShapeExportFilter blendShapeFilter)
        {
            list.Clear();
            if (exportRoot == null)
            {
                return;
            }

            foreach (var renderer in exportRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (TryGetMeshInfo(exportRoot, renderer, settings, blendShapeFilter, out MeshExportInfo info))
                {
                    list.Add(info);
                }
            }
        }

        static bool TryGetMeshInfo(GameObject root, Renderer renderer, MeshExportSettings settings, IBlendShapeExportFilter blendShapeFilter, out MeshExportInfo info)
        {
            if (root == null)
            {
                info = default;
                return false;
            }
            if (renderer == null)
            {
                info = default;
                return false;
            }

            if (renderer is SkinnedMeshRenderer smr)
            {
                info = new MeshExportInfo(renderer);
                info.Skinned = true;
                info.Mesh = smr.sharedMesh;
                info.IsRendererActive = smr.EnableForExport();
            }
            else if (renderer is MeshRenderer mr)
            {
                info = new MeshExportInfo(renderer);
                var filter = mr.GetComponent<MeshFilter>();
                if (filter != null)
                {
                    info.Mesh = filter.sharedMesh;
                }
                info.IsRendererActive = mr.EnableForExport();
            }
            else
            {
                throw new NotImplementedException();
            }

            if (info.Mesh == null)
            {
                info.Summary = "no mesh";
            }

            info.VertexColor = VertexColorUtility.DetectVertexColor(info.Mesh, info.Materials);

            var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, root.transform);
            CalcMeshSize(ref info, relativePath, settings, blendShapeFilter);

            return true;
        }

        static void CalcMeshSize(ref MeshExportInfo info,
            string relativePath,
            MeshExportSettings settings,
            IBlendShapeExportFilter blendShapeFilter
            )
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
            info.ExportBlendShapeVertexSize = settings.ExportOnlyBlendShapePosition ? 4 * 3 : 4 * (3 + 3);
            for (var i = 0; i < info.Mesh.blendShapeCount; ++i)
            {
                if (!blendShapeFilter.UseBlendShape(i, relativePath))
                {
                    continue;
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
                case VertexColorState.ExistsAndIsUsed:
                case VertexColorState.ExistsAndMixed: // エクスポートする
                    sb.Insert(0, "[use vcolor]");
                    break;
                case VertexColorState.ExistsButNotUsed:
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
    }
}
