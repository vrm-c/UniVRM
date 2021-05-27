using System.Collections.Generic;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public static class MaterialImporterParamExtensions
    {
        public static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateSubAssetKeyValue(this MaterialDescriptor matDesc)
        {
            foreach (var kv in matDesc.TextureSlots)
            {
                yield return (kv.Value.SubAssetKey, kv.Value);
            }
        }
    }
}
