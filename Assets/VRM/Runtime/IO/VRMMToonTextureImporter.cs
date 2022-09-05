using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public static class VRMMToonTextureImporter
    {
        public static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfData data, glTF_VRM_extensions vrm, int materialIdx)
        {
            var vrmMaterial = vrm.materialProperties[materialIdx];
            foreach (var kv in vrmMaterial.textureProperties)
            {
                if (TryGetTextureFromMaterialProperty(data, vrmMaterial, kv.Key, out var key, out var desc))
                {
                    yield return (key, desc);
                }
            }
        }
        public static bool TryGetTextureFromMaterialProperty(GltfData data, glTF_VRM_Material vrmMaterial, string textureKey, out SubAssetKey key, out TextureDescriptor desc)
        {
            // 任意の shader の import を許容する
            if (/*vrmMaterial.shader == MToon.Utils.ShaderName &&*/ vrmMaterial.textureProperties.TryGetValue(textureKey, out var textureIdx))
            {
                var (offset, scale) = (new Vector2(0, 0), new Vector2(1, 1));
                if (TryGetTextureOffsetAndScale(vrmMaterial, textureKey, out var os))
                {
                    offset = os.offset;
                    scale = os.scale;
                }

                switch (textureKey)
                {
                    case MToon.Utils.PropBumpMap:
                        return GltfTextureImporter.TryCreateNormal(data, textureIdx, offset, scale, out key, out desc);
                    default:
                        return GltfTextureImporter.TryCreateSrgb(data, textureIdx, offset, scale, out key, out desc);
                }
            }

            key = default;
            desc = default;
            return false;
        }

        public static bool TryGetTextureOffsetAndScale(glTF_VRM_Material vrmMaterial, string unityTextureKey, out (Vector2 offset, Vector2 scale) os)
        {
            if (vrmMaterial.vectorProperties.TryGetValue(unityTextureKey, out var vector))
            {
                os = (new Vector2(vector[0], vector[1]), new Vector2(vector[2], vector[3]));
                return true;
            }

            os = (new Vector2(0, 0), new Vector2(1, 1));
            return false;
        }
    }
}
