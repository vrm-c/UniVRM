// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_materials_mtoon {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_materials_mtoon.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_materials_mtoon extension)
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


public static VRMC_materials_mtoon Deserialize(JsonNode parsed)
{
    var value = new VRMC_materials_mtoon();

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

        if(key=="specVersion"){
            value.SpecVersion = kv.Value.GetString();
            continue;
        }

        if(key=="transparentWithZWrite"){
            value.TransparentWithZWrite = kv.Value.GetBoolean();
            continue;
        }

        if(key=="renderQueueOffsetNumber"){
            value.RenderQueueOffsetNumber = kv.Value.GetInt32();
            continue;
        }

        if(key=="shadeColorFactor"){
            value.ShadeColorFactor = Deserialize_ShadeColorFactor(kv.Value);
            continue;
        }

        if(key=="shadeMultiplyTexture"){
            value.ShadeMultiplyTexture = Deserialize_ShadeMultiplyTexture(kv.Value);
            continue;
        }

        if(key=="shadingShiftFactor"){
            value.ShadingShiftFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="shadingShiftTexture"){
            value.ShadingShiftTexture = Deserialize_ShadingShiftTexture(kv.Value);
            continue;
        }

        if(key=="shadingToonyFactor"){
            value.ShadingToonyFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="giEqualizationFactor"){
            value.GiEqualizationFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="matcapFactor"){
            value.MatcapFactor = Deserialize_MatcapFactor(kv.Value);
            continue;
        }

        if(key=="matcapTexture"){
            value.MatcapTexture = Deserialize_MatcapTexture(kv.Value);
            continue;
        }

        if(key=="parametricRimColorFactor"){
            value.ParametricRimColorFactor = Deserialize_ParametricRimColorFactor(kv.Value);
            continue;
        }

        if(key=="rimMultiplyTexture"){
            value.RimMultiplyTexture = Deserialize_RimMultiplyTexture(kv.Value);
            continue;
        }

        if(key=="rimLightingMixFactor"){
            value.RimLightingMixFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="parametricRimFresnelPowerFactor"){
            value.ParametricRimFresnelPowerFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="parametricRimLiftFactor"){
            value.ParametricRimLiftFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="outlineWidthMode"){
            value.OutlineWidthMode = (OutlineWidthMode)Enum.Parse(typeof(OutlineWidthMode), kv.Value.GetString(), true);
            continue;
        }

        if(key=="outlineWidthFactor"){
            value.OutlineWidthFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="outlineWidthMultiplyTexture"){
            value.OutlineWidthMultiplyTexture = Deserialize_OutlineWidthMultiplyTexture(kv.Value);
            continue;
        }

        if(key=="outlineColorFactor"){
            value.OutlineColorFactor = Deserialize_OutlineColorFactor(kv.Value);
            continue;
        }

        if(key=="outlineLightingMixFactor"){
            value.OutlineLightingMixFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="uvAnimationMaskTexture"){
            value.UvAnimationMaskTexture = Deserialize_UvAnimationMaskTexture(kv.Value);
            continue;
        }

        if(key=="uvAnimationScrollXSpeedFactor"){
            value.UvAnimationScrollXSpeedFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="uvAnimationScrollYSpeedFactor"){
            value.UvAnimationScrollYSpeedFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="uvAnimationRotationSpeedFactor"){
            value.UvAnimationRotationSpeedFactor = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_ShadeColorFactor(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static TextureInfo Deserialize_ShadeMultiplyTexture(JsonNode parsed)
{
    var value = new TextureInfo();

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

        if(key=="index"){
            value.Index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.TexCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static ShadingShiftTextureInfo Deserialize_ShadingShiftTexture(JsonNode parsed)
{
    var value = new ShadingShiftTextureInfo();

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

        if(key=="index"){
            value.Index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.TexCoord = kv.Value.GetInt32();
            continue;
        }

        if(key=="scale"){
            value.Scale = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_MatcapFactor(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static TextureInfo Deserialize_MatcapTexture(JsonNode parsed)
{
    var value = new TextureInfo();

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

        if(key=="index"){
            value.Index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.TexCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_ParametricRimColorFactor(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static TextureInfo Deserialize_RimMultiplyTexture(JsonNode parsed)
{
    var value = new TextureInfo();

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

        if(key=="index"){
            value.Index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.TexCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static TextureInfo Deserialize_OutlineWidthMultiplyTexture(JsonNode parsed)
{
    var value = new TextureInfo();

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

        if(key=="index"){
            value.Index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.TexCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_OutlineColorFactor(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static TextureInfo Deserialize_UvAnimationMaskTexture(JsonNode parsed)
{
    var value = new TextureInfo();

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

        if(key=="index"){
            value.Index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.TexCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

} // GltfDeserializer
} // UniGLTF 
