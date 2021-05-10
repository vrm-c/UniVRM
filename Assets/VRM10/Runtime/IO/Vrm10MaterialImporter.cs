using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public static class Vrm10MaterialImporter
    {
        /// <summary>
        /// VMRC_materials_mtoon の場合にマテリアル生成情報を作成する
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="i"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool TryCreateParam(GltfParser parser, int i, out MaterialImportParam param)
        {
            var m = parser.GLTF.materials[i];
            if (!UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions,
                out UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon mtoon))
            {
                // Fallback to glTF, when MToon extension does not exist.
                param = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            param = new MaterialImportParam(m.name, MToon.Utils.ShaderName);

            foreach (var (key, (subAssetKey, value)) in Vrm10MToonMaterialTextureImporter.TryGetAllTextures(parser, m, mtoon))
            {
                param.TextureSlots.Add(key, value);
            }

            foreach (var (key, value) in Vrm10MToonMaterialParameterImporter.TryGetAllColors(m, mtoon))
            {
                param.Colors.Add(key, value);
            }

            foreach (var (key, value) in Vrm10MToonMaterialParameterImporter.TryGetAllFloats(m, mtoon))
            {
                param.FloatValues.Add(key, value);
            }

            foreach (var (key, value) in Vrm10MToonMaterialParameterImporter.TryGetAllFloatArrays(m, mtoon))
            {
                param.Vectors.Add(key, value);
            }

            param.RenderQueue = Vrm10MToonMaterialParameterImporter.TryGetRenderQueue(m, mtoon);
            
            param.Actions.Add(material =>
            {
                // Set hidden properties, keywords from float properties.
                MToon.Utils.ValidateProperties(material, isBlendModeChangedByUser: false);
            });

            return true;
        }

        public static MaterialImportParam GetMaterialParam(GltfParser parser, int i)
        {
            // mtoon
            if (!TryCreateParam(parser, i, out MaterialImportParam param))
            {
                // unlit
                if (!GltfUnlitMaterial.TryCreateParam(parser, i, out param))
                {
                    // pbr
                    GltfPBRMaterial.TryCreateParam(parser, i, out param);
                }
            }
            return param;
        }
    }
}
