using System;
using System.Linq;
using UniGLTF;
using UnityEngine;
using VRMShaders;
using VRMShaders.VRM10.MToon10.Runtime;

namespace UniVRM10
{
    /// <summary>
    /// Convert MToon parameters from glTF specification to Unity implementation.(for URP)
    /// </summary>
    public static class UrpVrm10MToonMaterialImporter
    {
        /// <summary>
        /// VMRC_materials_mtoon の場合にマテリアル生成情報を作成する
        /// </summary>
        public static bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
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
                Shader.Find(MToon10Meta.URPUnityShaderName),
                null,
                Vrm10MToonTextureImporter.EnumerateAllTextures(data, m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.Item2.Item2),
                BuiltInVrm10MToonMaterialImporter.TryGetAllFloats(m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
                BuiltInVrm10MToonMaterialImporter.TryGetAllColors(m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
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
