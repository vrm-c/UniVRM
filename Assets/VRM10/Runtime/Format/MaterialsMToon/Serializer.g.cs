// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_materials_mtoon {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_materials_mtoon extension)
        {
            if (dst is glTFExtensionImport)
            {
                throw new NotImplementedException();
            }

            if (!(dst is glTFExtensionExport extensions))
            {
                extensions = new glTFExtensionExport();
                dst = extensions;
            }

            var f = new JsonFormatter();
            Serialize(f, extension);
            extensions.Add(VRMC_materials_mtoon.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_materials_mtoon value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.Version)){
        f.Key("version");                
        f.Value(value.Version);
    }

    if(value.TransparentWithZWrite.HasValue){
        f.Key("transparentWithZWrite");                
        f.Value(value.TransparentWithZWrite.GetValueOrDefault());
    }

    if(value.RenderQueueOffsetNumber.HasValue){
        f.Key("renderQueueOffsetNumber");                
        f.Value(value.RenderQueueOffsetNumber.GetValueOrDefault());
    }

    if(value.ShadeFactor!=null&&value.ShadeFactor.Count()>=0){
        f.Key("shadeFactor");                
        Serialize_ShadeFactor(f, value.ShadeFactor);
    }

    if(value.ShadeMultiplyTexture.HasValue){
        f.Key("shadeMultiplyTexture");                
        f.Value(value.ShadeMultiplyTexture.GetValueOrDefault());
    }

    if(value.ShadingShiftFactor.HasValue){
        f.Key("shadingShiftFactor");                
        f.Value(value.ShadingShiftFactor.GetValueOrDefault());
    }

    if(value.ShadingToonyFactor.HasValue){
        f.Key("shadingToonyFactor");                
        f.Value(value.ShadingToonyFactor.GetValueOrDefault());
    }

    if(value.LightColorAttenuationFactor.HasValue){
        f.Key("lightColorAttenuationFactor");                
        f.Value(value.LightColorAttenuationFactor.GetValueOrDefault());
    }

    if(value.GiIntensityFactor.HasValue){
        f.Key("giIntensityFactor");                
        f.Value(value.GiIntensityFactor.GetValueOrDefault());
    }

    if(value.AdditiveTexture.HasValue){
        f.Key("additiveTexture");                
        f.Value(value.AdditiveTexture.GetValueOrDefault());
    }

    if(value.RimFactor!=null&&value.RimFactor.Count()>=0){
        f.Key("rimFactor");                
        Serialize_RimFactor(f, value.RimFactor);
    }

    if(value.RimMultiplyTexture.HasValue){
        f.Key("rimMultiplyTexture");                
        f.Value(value.RimMultiplyTexture.GetValueOrDefault());
    }

    if(value.RimLightingMixFactor.HasValue){
        f.Key("rimLightingMixFactor");                
        f.Value(value.RimLightingMixFactor.GetValueOrDefault());
    }

    if(value.RimFresnelPowerFactor.HasValue){
        f.Key("rimFresnelPowerFactor");                
        f.Value(value.RimFresnelPowerFactor.GetValueOrDefault());
    }

    if(value.RimLiftFactor.HasValue){
        f.Key("rimLiftFactor");                
        f.Value(value.RimLiftFactor.GetValueOrDefault());
    }

    if(true){
        f.Key("outlineWidthMode");                
        f.Value(value.OutlineWidthMode.ToString());
    }

    if(value.OutlineWidthFactor.HasValue){
        f.Key("outlineWidthFactor");                
        f.Value(value.OutlineWidthFactor.GetValueOrDefault());
    }

    if(value.OutlineWidthMultiplyTexture.HasValue){
        f.Key("outlineWidthMultiplyTexture");                
        f.Value(value.OutlineWidthMultiplyTexture.GetValueOrDefault());
    }

    if(value.OutlineScaledMaxDistanceFactor.HasValue){
        f.Key("outlineScaledMaxDistanceFactor");                
        f.Value(value.OutlineScaledMaxDistanceFactor.GetValueOrDefault());
    }

    if(true){
        f.Key("outlineColorMode");                
        f.Value(value.OutlineColorMode.ToString());
    }

    if(value.OutlineFactor!=null&&value.OutlineFactor.Count()>=0){
        f.Key("outlineFactor");                
        Serialize_OutlineFactor(f, value.OutlineFactor);
    }

    if(value.OutlineLightingMixFactor.HasValue){
        f.Key("outlineLightingMixFactor");                
        f.Value(value.OutlineLightingMixFactor.GetValueOrDefault());
    }

    if(value.UvAnimationMaskTexture.HasValue){
        f.Key("uvAnimationMaskTexture");                
        f.Value(value.UvAnimationMaskTexture.GetValueOrDefault());
    }

    if(value.UvAnimationScrollXSpeedFactor.HasValue){
        f.Key("uvAnimationScrollXSpeedFactor");                
        f.Value(value.UvAnimationScrollXSpeedFactor.GetValueOrDefault());
    }

    if(value.UvAnimationScrollYSpeedFactor.HasValue){
        f.Key("uvAnimationScrollYSpeedFactor");                
        f.Value(value.UvAnimationScrollYSpeedFactor.GetValueOrDefault());
    }

    if(value.UvAnimationRotationSpeedFactor.HasValue){
        f.Key("uvAnimationRotationSpeedFactor");                
        f.Value(value.UvAnimationRotationSpeedFactor.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_ShadeFactor(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_RimFactor(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_OutlineFactor(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

    } // class
} // namespace
