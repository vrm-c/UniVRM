using System;
using System.Linq;
using UniGLTF;
using UnityEngine;
using VRM10.MToon10;

namespace UniVRM10
{
    /// <summary>
    /// A class that generates MaterialDescriptor for "VRM10/Universal Render Pipeline/MToon10" shader based on vrm-1.0 Material specification.
    ///
    /// https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_materials_mtoon-1.0/README.md
    /// </summary>
    public class UrpVrm10MToonMaterialImporter
    {
        /// <summary>
        /// Can be replaced with custom shaders that are compatible with "VRM10/Universal Render Pipeline/MToon10" properties and keywords.
        /// </summary>
        public Shader Shader { get; set; }

        public UrpVrm10MToonMaterialImporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find("VRM10/Universal Render Pipeline/MToon10");
        }

        /// <summary>
        /// VMRC_materials_mtoon の場合にマテリアル生成情報を作成する
        /// </summary>
        public bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            var m = data.GLTF.materials[i];
            if (!UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions, out var mtoon))
            {
                // Fallback to glTF, when MToon extension does not exist.
                matDesc = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            matDesc = new MaterialDescriptor(
                m.name,
                Shader.Find(MToon10Meta.UnityUrpShaderName),
                null,
                Vrm10MToonTextureImporter.EnumerateAllTextures(data, m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.Item2.Item2),
                BuiltInVrm10MToonMaterialImporter.TryGetAllFloats(m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
                BuiltInVrm10MToonMaterialImporter.TryGetAllColors(data, m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
                BuiltInVrm10MToonMaterialImporter.TryGetAllFloatArrays(m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
                new Action<Material>[]
                {
                    material =>
                    {
                        // Set hidden properties, keywords from float properties.
                        new MToonValidator(material).Validate();
                    }
                });

            return true;
        }
    }
}
