using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniGLTF.M17N;
using UnityEngine;

namespace UniGLTF
{
    [Serializable]
    public class MeshExportValidator : ScriptableObject
    {
        public static Mesh GetMesh(Renderer r)
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

        public List<MeshExportInfo> Meshes = new List<MeshExportInfo>();

        public int ExpectedExportByteSize => Meshes.Where(x => x.IsRendererActive).Sum(x => x.ExportByteSize);

        public MeshExportSettings Settings;

        public virtual bool UseBlendShape(int index, string relativePath) => true;

        public virtual void CalcMeshSize(ref MeshExportInfo info,
                                        string relativePath)
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
            info.ExportBlendShapeVertexSize = Settings.ExportOnlyBlendShapePosition ? 4 * 3 : 4 * (3 + 3);
            for (var i = 0; i < info.Mesh.blendShapeCount; ++i)
            {
                if (!UseBlendShape(i, relativePath))
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

        bool TryGetMeshInfo(GameObject root, Renderer renderer, out MeshExportInfo info)
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
            CalcMeshSize(ref info, relativePath);

            return true;
        }

        public void SetRoot(GameObject ExportRoot, MeshExportSettings settings)
        {
            Settings = settings;
            Meshes.Clear();
            if (ExportRoot == null)
            {
                return;
            }

            foreach (var renderer in ExportRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (TryGetMeshInfo(ExportRoot, renderer, out MeshExportInfo info))
                {
                    Meshes.Add(info);
                }
            }
        }

        public Func<string, string> GltfMaterialFromUnityShaderName = DefaultGltfMaterialType;

        public static string DefaultGltfMaterialType(string shaderName)
        {
            if (shaderName == "Standard")
            {
                return "pbr";
            }
            if (MaterialExporter.IsUnlit(shaderName))
            {
                return "unlit";
            }
            return null;
        }

        public enum Messages
        {
            MATERIALS_LESS_THAN_SUBMESH_COUNT,
            MATERIALS_GREATER_THAN_SUBMESH_COUNT,
            MATERIALS_CONTAINS_NULL,
            UNKNOWN_SHADER,
        }

        public IEnumerable<Validation> Validate(GameObject ExportRoot)
        {
            foreach (var info in Meshes)
            {
                // invalid materials.len
                if (info.Renderer.sharedMaterials.Length < info.Mesh.subMeshCount)
                {
                    // submesh より material の方が少ない
                    yield return Validation.Error(Messages.MATERIALS_LESS_THAN_SUBMESH_COUNT.Msg());
                }
                else
                {
                    if (info.Renderer.sharedMaterials.Length > info.Mesh.subMeshCount)
                    {
                        // submesh より material の方が多い
                        yield return Validation.Warning(Messages.MATERIALS_GREATER_THAN_SUBMESH_COUNT.Msg());
                    }

                    if (info.Renderer.sharedMaterials.Take(info.Mesh.subMeshCount).Any(x => x == null))
                    {
                        // material に null が含まれる(unity で magenta になっているはず)
                        yield return Validation.Error($"{info.Renderer}: {Messages.MATERIALS_CONTAINS_NULL.Msg()}");
                    }
                }
            }

            foreach (var m in Meshes.SelectMany(x => x.Renderer.sharedMaterials).Distinct())
            {
                if (m == null)
                {
                    continue;
                }
                var gltfMaterial = GltfMaterialFromUnityShaderName(m.shader.name);
                if (string.IsNullOrEmpty(gltfMaterial))
                {
                    yield return Validation.Warning($"{m}: unknown shader: {m.shader.name} => export as gltf default");
                }
            }

            yield break;
        }
    }
}
