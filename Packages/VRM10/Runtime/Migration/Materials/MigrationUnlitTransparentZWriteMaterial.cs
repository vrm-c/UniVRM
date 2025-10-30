using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Migration Target: VRM file with "VRM/UnlitTransparentZWrite" material exported with UniVRM v0.43 or lower.
    ///
    /// 過去の UniVRM の "VRM/UnlitTransparentZWrite" シェーダをマイグレーションする.
    /// 他の Unlit シェーダと違い、VRMC_materials_mtoon を用いてマイグレーションする.
    /// </summary>
    internal static class MigrationUnlitTransparentZWriteMaterial
    {
        private const int MaxRenderQueueOffset = 9; // NOTE: vrm-1.0 spec

        private const string Unity0XShaderName = "VRM/UnlitTransparentZWrite";
        private const int Unity0XDefaultRenderQueue = 2501;

        public static bool Migrate(glTF gltf, IReadOnlyList<JsonNode> vrm0XMaterials)
        {
            var anyMigrated = false;
            var mapper = GetRenderQueueMapper(vrm0XMaterials);

            for (var materialIdx = 0; materialIdx < gltf.materials.Count; ++materialIdx)
            {
                try
                {
                    var newMaterial = Migrate(vrm0XMaterials[materialIdx], gltf.materials[materialIdx].name, mapper);
                    if (newMaterial != null)
                    {
                        // NOTE: UnlitTransparentZWrite の場合は、名前を引き継いで、glTFMaterial を上書きする.
                        gltf.materials[materialIdx] = newMaterial;
                        anyMigrated = true;
                    }
                }
                catch (Exception ex)
                {
                    UniGLTFLogger.Exception(ex);
                }
            }

            return anyMigrated;
        }

        private static Dictionary<int, int> GetRenderQueueMapper(IReadOnlyList<JsonNode> vrm0XMaterials)
        {
            try
            {
                var renderQueueSet = new SortedSet<int>();
                foreach (var vrm0XMaterial in vrm0XMaterials)
                {
                    var renderQueue = MigrationMaterialUtil.GetRenderQueue(vrm0XMaterial);
                    if (renderQueue.HasValue && renderQueue.Value != -1)
                    {
                        renderQueueSet.Add(renderQueue.Value);
                    }
                    else
                    {
                        renderQueueSet.Add(Unity0XDefaultRenderQueue);
                    }
                }

                var mapper = new Dictionary<int, int>();
                var currentQueueOffset = 0;
                foreach (var queue in renderQueueSet)
                {
                    mapper.Add(queue, currentQueueOffset);
                    currentQueueOffset = Mathf.Min(currentQueueOffset + 1, MaxRenderQueueOffset);
                }

                return mapper;
            }
            catch (Exception ex)
            {
                UniGLTFLogger.Exception(ex);
                return new Dictionary<int, int>();
            }
        }

        private static glTFMaterial Migrate(JsonNode vrm0XMaterial, string materialName, Dictionary<int, int> renderQueueMapper)
        {
            try
            {
                if (MigrationMaterialUtil.GetShaderName(vrm0XMaterial) != Unity0XShaderName)
                {
                    return null;
                }

                var baseColorFactor = MigrationMaterialUtil.GetBaseColorFactor(vrm0XMaterial);
                var baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
                var emissiveTexture = new glTFMaterialEmissiveTextureInfo
                {
                    index = baseColorTexture.index,
                    extensions = baseColorTexture.extensions,
                };
                var renderQueue = MigrationMaterialUtil.GetRenderQueue(vrm0XMaterial) ?? Unity0XDefaultRenderQueue;
                var renderQueueOffset = renderQueueMapper.ContainsKey(renderQueue) ? renderQueueMapper[renderQueue] : 0;

                var mtoonMaterial = new glTFMaterial
                {
                    name = materialName,
                    extensions = new glTFExtensionExport().Add(
                        glTF_KHR_materials_unlit.ExtensionName,
                        new ArraySegment<byte>(glTF_KHR_materials_unlit.Raw)
                    ),
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new[] { 0f, 0f, 0f, baseColorFactor[3] }, // black + _Color.a
                        baseColorTexture = baseColorTexture, // _MainTex
                        metallicFactor = 0f,
                        roughnessFactor = 1f,
                    },
                    alphaMode = "BLEND",
                    alphaCutoff = 0.5f,
                    doubleSided = false,
                    emissiveFactor = new[] { baseColorFactor[0], baseColorFactor[1], baseColorFactor[2] }, // _Color.rgb
                    emissiveTexture = emissiveTexture,
                };

                var mtoon10 = new VRMC_materials_mtoon
                {
                    SpecVersion = Vrm10Exporter.MTOON_SPEC_VERSION,
                    TransparentWithZWrite = true, // transparent with zWrite
                    RenderQueueOffsetNumber = renderQueueOffset,
                    ShadeColorFactor = new[] { 0f, 0f, 0f }, // black
                    OutlineWidthMode = OutlineWidthMode.none // disable outline
                };
                UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref mtoonMaterial.extensions,
                    mtoon10);

                return mtoonMaterial;
            }
            catch (Exception)
            {
                UniGLTFLogger.Warning($"Migration failed in VRM/UnlitTransparentZWrite material: {materialName}");
                return null;
            }
        }
    }
}