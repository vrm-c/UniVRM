// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_materials_mtoon {

public static class GltfDeserializer
{

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_materials_mtoon extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == VRMC_materials_mtoon.ExtensionNameUtf8)
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

        if(key=="version"){
            value.Version = kv.Value.GetString();
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

        if(key=="shadeFactor"){
            value.ShadeFactor = Deserialize_ShadeFactor(kv.Value);
            continue;
        }

        if(key=="shadeMultiplyTexture"){
            value.ShadeMultiplyTexture = kv.Value.GetInt32();
            continue;
        }

        if(key=="shadingShiftFactor"){
            value.ShadingShiftFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="shadingToonyFactor"){
            value.ShadingToonyFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="lightColorAttenuationFactor"){
            value.LightColorAttenuationFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="giIntensityFactor"){
            value.GiIntensityFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="additiveTexture"){
            value.AdditiveTexture = kv.Value.GetInt32();
            continue;
        }

        if(key=="rimFactor"){
            value.RimFactor = Deserialize_RimFactor(kv.Value);
            continue;
        }

        if(key=="rimMultiplyTexture"){
            value.RimMultiplyTexture = kv.Value.GetInt32();
            continue;
        }

        if(key=="rimLightingMixFactor"){
            value.RimLightingMixFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="rimFresnelPowerFactor"){
            value.RimFresnelPowerFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="rimLiftFactor"){
            value.RimLiftFactor = kv.Value.GetSingle();
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
            value.OutlineWidthMultiplyTexture = kv.Value.GetInt32();
            continue;
        }

        if(key=="outlineScaledMaxDistanceFactor"){
            value.OutlineScaledMaxDistanceFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="outlineColorMode"){
            value.OutlineColorMode = (OutlineColorMode)Enum.Parse(typeof(OutlineColorMode), kv.Value.GetString(), true);
            continue;
        }

        if(key=="outlineFactor"){
            value.OutlineFactor = Deserialize_OutlineFactor(kv.Value);
            continue;
        }

        if(key=="outlineLightingMixFactor"){
            value.OutlineLightingMixFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="uvAnimationMaskTexture"){
            value.UvAnimationMaskTexture = kv.Value.GetInt32();
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

public static float[] Deserialize_ShadeFactor(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] Deserialize_RimFactor(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] Deserialize_OutlineFactor(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

} // GltfDeserializer
} // UniGLTF 
