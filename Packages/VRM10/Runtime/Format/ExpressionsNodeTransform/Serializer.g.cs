// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_vrm_expressions_node_transform {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_vrm_expressions_node_transform extension)
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
            extensions.Add(VRMC_vrm_expressions_node_transform.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_vrm_expressions_node_transform value)
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

    if(value.NodeTransformBinds!=null&&value.NodeTransformBinds.Count()>=1){
        f.Key("nodeTransformBinds");                
        Serialize_NodeTransformBinds(f, value.NodeTransformBinds);
    }

    f.EndMap();
}

public static void Serialize_NodeTransformBinds(JsonFormatter f, List<NodeTransformBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_NodeTransformBinds_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_NodeTransformBinds_ITEM(JsonFormatter f, NodeTransformBind value)
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

    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    if(value.Rotation!=null&&value.Rotation.Count()>=4){
        f.Key("rotation");                
        __nodeTransformBinds_ITEM_Serialize_Rotation(f, value.Rotation);
    }

    if(value.Scale!=null&&value.Scale.Count()>=3){
        f.Key("scale");                
        __nodeTransformBinds_ITEM_Serialize_Scale(f, value.Scale);
    }

    if(value.Translation!=null&&value.Translation.Count()>=3){
        f.Key("translation");                
        __nodeTransformBinds_ITEM_Serialize_Translation(f, value.Translation);
    }

    f.EndMap();
}

public static void __nodeTransformBinds_ITEM_Serialize_Rotation(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __nodeTransformBinds_ITEM_Serialize_Scale(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __nodeTransformBinds_ITEM_Serialize_Translation(JsonFormatter f, float[] value)
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
