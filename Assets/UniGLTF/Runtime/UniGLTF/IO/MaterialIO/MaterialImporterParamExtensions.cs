using System.Collections.Generic;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public static class MaterialImporterParamExtensions
    {
        public static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateSubAssetKeyValue(this MaterialImportParam param)
        {
            foreach (var kv in param.TextureSlots)
            {
                yield return (kv.Value.SubAssetKey, kv.Value);
            }
        }
    }
}
