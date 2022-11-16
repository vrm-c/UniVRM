using UnityEngine;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniGLTF
{
    public static class GltfMaterialImportUtils
    {
        public static string ImportMaterialName(int index, glTFMaterial src)
        {
            if (src != null && !string.IsNullOrEmpty(src.name))
            {
                return src.name;
            }
            return $"material_{index:00}";
        }

        public static Color? ImportLinearBaseColorFactor(GltfData data, glTFMaterial src)
        {
            if (src.pbrMetallicRoughness == null) return null;
            var baseColorFactor = src.pbrMetallicRoughness.baseColorFactor;
            if (baseColorFactor == null || baseColorFactor.Length != 4) return null;

            if (data.MigrationFlags.IsBaseColorFactorGamma)
            {
                return baseColorFactor.ToColor4(ColorSpace.sRGB, ColorSpace.Linear);
            }
            else
            {
                return baseColorFactor.ToColor4(ColorSpace.Linear, ColorSpace.Linear);
            }
        }

        public static Color? ImportLinearEmissiveFactor(GltfData data, glTFMaterial src)
        {
            if (src.emissiveFactor == null || src.emissiveFactor.Length != 3) return null;

            // NOTE: glTF 仕様違反だが emissiveFactor に 1.0 より大きな値が入っていた場合もそのまま受け入れる.
            var emissiveFactor = new Vector3(src.emissiveFactor[0], src.emissiveFactor[1], src.emissiveFactor[2]);
            if (glTF_KHR_materials_emissive_strength.TryGet(src.extensions, out var emissiveStrength))
            {
                emissiveFactor *= emissiveStrength.emissiveStrength;
            }
            else if (Extensions.VRMC_materials_hdr_emissiveMultiplier.GltfDeserializer.TryGet(src.extensions, out var ex))
            {
                if (ex.EmissiveMultiplier != null)
                {
                    emissiveFactor *= ex.EmissiveMultiplier.Value;
                }
            }

            if (data.MigrationFlags.IsEmissiveFactorGamma)
            {
                return emissiveFactor.ToColor3(VRMShaders.ColorSpace.sRGB, VRMShaders.ColorSpace.Linear);
            }
            else
            {
                return emissiveFactor.ToColor3(ColorSpace.Linear, ColorSpace.Linear);
            }
        }
    }
}