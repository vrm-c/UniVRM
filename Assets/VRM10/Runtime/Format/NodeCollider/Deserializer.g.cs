// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_node_collider {

public static class GltfDeserializer
{

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_node_collider extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == VRMC_node_collider.ExtensionNameUtf8)
            {
                extension = Deserialize(kv.Value);
                return true;
            }
        }
    }

    extension = default;
    return false;
}


public static VRMC_node_collider Deserialize(JsonNode parsed)
{
    var value = new VRMC_node_collider();

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

        if(key=="shapes"){
            value.Shapes = Deserialize_Shapes(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<ColliderShape> Deserialize_Shapes(JsonNode parsed)
{
    var value = new List<ColliderShape>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_Shapes_ITEM(x));
    }
	return value;
} 

public static ColliderShape Deserialize_Shapes_ITEM(JsonNode parsed)
{
    var value = new ColliderShape();

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

        if(key=="sphere"){
            value.Sphere = Deserialize_Sphere(kv.Value);
            continue;
        }

        if(key=="capsule"){
            value.Capsule = Deserialize_Capsule(kv.Value);
            continue;
        }

    }
    return value;
}

public static ColliderShapeSphere Deserialize_Sphere(JsonNode parsed)
{
    var value = new ColliderShapeSphere();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.Offset = Deserialize_Offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.Radius = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static ColliderShapeCapsule Deserialize_Capsule(JsonNode parsed)
{
    var value = new ColliderShapeCapsule();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.Offset = Deserialize_Offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.Radius = kv.Value.GetSingle();
            continue;
        }

        if(key=="tail"){
            value.Tail = Deserialize_Tail(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_Tail(JsonNode parsed)
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
