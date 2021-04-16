using System.Collections.Generic;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public static class MaterialImporterParamExtensions
    {
        public static IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateSubAssetKeyValue(this MaterialImportParam param)
        {
            foreach (var kv in param.TextureSlots)
            {
                var key = new SubAssetKey(typeof(Texture2D), kv.Value.UnityObjectName);
                yield return (key, kv.Value);
            }
        }
    }
}
