// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_materials_hdr_emissiveMultiplier {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_materials_hdr_emissiveMultiplier.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_materials_hdr_emissiveMultiplier extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == ExtensionNameUtf8)
            {
                extension = Deserialize(kv.Value);
                return true;
            }
        }
    }

    extension = default;
    return false;
}


public static VRMC_materials_hdr_emissiveMultiplier Deserialize(JsonNode parsed)
{
    var value = new VRMC_materials_hdr_emissiveMultiplier();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="extensions"){
            value.Extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.Extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="emissiveMultiplier"){
            value.EmissiveMultiplier = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

} // GltfDeserializer
} // UniGLTF 
