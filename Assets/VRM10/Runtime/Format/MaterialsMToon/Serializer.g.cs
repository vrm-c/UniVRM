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


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.SpecVersion)){
        f.Key("specVersion");                
        f.Value(value.SpecVersion);
    }

    if(value.TransparentWithZWrite.HasValue){
        f.Key("transparentWithZWrite");                
        f.Value(value.TransparentWithZWrite.GetValueOrDefault());
    }

    if(value.RenderQueueOffsetNumber.HasValue){
        f.Key("renderQueueOffsetNumber");                
        f.Value(value.RenderQueueOffsetNumber.GetValueOrDefault());
    }

    if(value.ShadeColorFactor!=null&&value.ShadeColorFactor.Count()>=3){
        f.Key("shadeColorFactor");                
        Serialize_ShadeColorFactor(f, value.ShadeColorFactor);
    }

    if(value.ShadeMultiplyTexture!=null){
        f.Key("shadeMultiplyTexture");                
        Serialize_ShadeMultiplyTexture(f, value.ShadeMultiplyTexture);
    }

    if(value.ShadingShiftFactor.HasValue){
        f.Key("shadingShiftFactor");                
        f.Value(value.ShadingShiftFactor.GetValueOrDefault());
    }

    if(value.ShadingShiftTexture!=null){
        f.Key("shadingShiftTexture");                
        Serialize_ShadingShiftTexture(f, value.ShadingShiftTexture);
    }

    if(value.ShadingToonyFactor.HasValue){
        f.Key("shadingToonyFactor");                
        f.Value(value.ShadingToonyFactor.GetValueOrDefault());
    }

    if(value.GiEqualizationFactor.HasValue){
        f.Key("giEqualizationFactor");                
        f.Value(value.GiEqualizationFactor.GetValueOrDefault());
    }

    if(value.MatcapFactor!=null&&value.MatcapFactor.Count()>=3){
        f.Key("matcapFactor");                
        Serialize_MatcapFactor(f, value.MatcapFactor);
    }

    if(value.MatcapTexture!=null){
        f.Key("matcapTexture");                
        Serialize_MatcapTexture(f, value.MatcapTexture);
    }

    if(value.ParametricRimColorFactor!=null&&value.ParametricRimColorFactor.Count()>=3){
        f.Key("parametricRimColorFactor");                
        Serialize_ParametricRimColorFactor(f, value.ParametricRimColorFactor);
    }

    if(value.RimMultiplyTexture!=null){
        f.Key("rimMultiplyTexture");                
        Serialize_RimMultiplyTexture(f, value.RimMultiplyTexture);
    }

    if(value.RimLightingMixFactor.HasValue){
        f.Key("rimLightingMixFactor");                
        f.Value(value.RimLightingMixFactor.GetValueOrDefault());
    }

    if(value.ParametricRimFresnelPowerFactor.HasValue){
        f.Key("parametricRimFresnelPowerFactor");                
        f.Value(value.ParametricRimFresnelPowerFactor.GetValueOrDefault());
    }

    if(value.ParametricRimLiftFactor.HasValue){
        f.Key("parametricRimLiftFactor");                
        f.Value(value.ParametricRimLiftFactor.GetValueOrDefault());
    }

    if(true){
        f.Key("outlineWidthMode");                
        f.Value(value.OutlineWidthMode.ToString());
    }

    if(value.OutlineWidthFactor.HasValue){
        f.Key("outlineWidthFactor");                
        f.Value(value.OutlineWidthFactor.GetValueOrDefault());
    }

    if(value.OutlineWidthMultiplyTexture!=null){
        f.Key("outlineWidthMultiplyTexture");                
        Serialize_OutlineWidthMultiplyTexture(f, value.OutlineWidthMultiplyTexture);
    }

    if(value.OutlineColorFactor!=null&&value.OutlineColorFactor.Count()>=3){
        f.Key("outlineColorFactor");                
        Serialize_OutlineColorFactor(f, value.OutlineColorFactor);
    }

    if(value.OutlineLightingMixFactor.HasValue){
        f.Key("outlineLightingMixFactor");                
        f.Value(value.OutlineLightingMixFactor.GetValueOrDefault());
    }

    if(value.UvAnimationMaskTexture!=null){
        f.Key("uvAnimationMaskTexture");                
        Serialize_UvAnimationMaskTexture(f, value.UvAnimationMaskTexture);
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

public static void Serialize_ShadeColorFactor(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_ShadeMultiplyTexture(JsonFormatter f, TextureInfo value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Index.HasValue){
        f.Key("index");                
        f.Value(value.Index.GetValueOrDefault());
    }

    if(value.TexCoord.HasValue){
        f.Key("texCoord");                
        f.Value(value.TexCoord.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_ShadingShiftTexture(JsonFormatter f, ShadingShiftTextureInfo value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Index.HasValue){
        f.Key("index");                
        f.Value(value.Index.GetValueOrDefault());
    }

    if(value.TexCoord.HasValue){
        f.Key("texCoord");                
        f.Value(value.TexCoord.GetValueOrDefault());
    }

    if(value.Scale.HasValue){
        f.Key("scale");                
        f.Value(value.Scale.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_MatcapFactor(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_MatcapTexture(JsonFormatter f, TextureInfo value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Index.HasValue){
        f.Key("index");                
        f.Value(value.Index.GetValueOrDefault());
    }

    if(value.TexCoord.HasValue){
        f.Key("texCoord");                
        f.Value(value.TexCoord.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_ParametricRimColorFactor(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_RimMultiplyTexture(JsonFormatter f, TextureInfo value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Index.HasValue){
        f.Key("index");                
        f.Value(value.Index.GetValueOrDefault());
    }

    if(value.TexCoord.HasValue){
        f.Key("texCoord");                
        f.Value(value.TexCoord.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_OutlineWidthMultiplyTexture(JsonFormatter f, TextureInfo value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Index.HasValue){
        f.Key("index");                
        f.Value(value.Index.GetValueOrDefault());
    }

    if(value.TexCoord.HasValue){
        f.Key("texCoord");                
        f.Value(value.TexCoord.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_OutlineColorFactor(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_UvAnimationMaskTexture(JsonFormatter f, TextureInfo value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        (value.Extensions as glTFExtension).Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        (value.Extras as glTFExtension).Serialize(f);
    }

    if(value.Index.HasValue){
        f.Key("index");                
        f.Value(value.Index.GetValueOrDefault());
    }

    if(value.TexCoord.HasValue){
        f.Key("texCoord");                
        f.Value(value.TexCoord.GetValueOrDefault());
    }

    f.EndMap();
}

    } // class
} // namespace
