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

        public void SetRoot(GameObject ExportRoot, MeshExportSettings settings)
        {
            Settings = settings;

            MeshExportInfo.GetInfo(ExportRoot, Meshes, settings, UseBlendShape);
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
