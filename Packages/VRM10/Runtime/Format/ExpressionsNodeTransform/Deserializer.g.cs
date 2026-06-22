// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_vrm_expressions_node_transform {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_vrm_expressions_node_transform.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_vrm_expressions_node_transform extension)
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


public static VRMC_vrm_expressions_node_transform Deserialize(JsonNode parsed)
{
    var value = new VRMC_vrm_expressions_node_transform();

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

        if(key=="nodeTransformBinds"){
            value.NodeTransformBinds = Deserialize_NodeTransformBinds(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<NodeTransformBind> Deserialize_NodeTransformBinds(JsonNode parsed)
{
    var value = new List<NodeTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_NodeTransformBinds_ITEM(x));
    }
	return value;
} 

public static NodeTransformBind Deserialize_NodeTransformBinds_ITEM(JsonNode parsed)
{
    var value = new NodeTransformBind();

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

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

        if(key=="rotation"){
            value.Rotation = __nodeTransformBinds_ITEM_Deserialize_Rotation(kv.Value);
            continue;
        }

        if(key=="scale"){
            value.Scale = __nodeTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="translation"){
            value.Translation = __nodeTransformBinds_ITEM_Deserialize_Translation(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __nodeTransformBinds_ITEM_Deserialize_Rotation(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __nodeTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __nodeTransformBinds_ITEM_Deserialize_Translation(JsonNode parsed)
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
