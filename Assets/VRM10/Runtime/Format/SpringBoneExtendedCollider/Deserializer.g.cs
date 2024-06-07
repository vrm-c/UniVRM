// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_springBone_extended_collider {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_springBone_extended_collider.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_springBone_extended_collider extension)
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


public static VRMC_springBone_extended_collider Deserialize(JsonNode parsed)
{
    var value = new VRMC_springBone_extended_collider();

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

        if(key=="shape"){
            value.Shape = Deserialize_Shape(kv.Value);
            continue;
        }

    }
    return value;
}

public static ExtendedColliderShape Deserialize_Shape(JsonNode parsed)
{
    var value = new ExtendedColliderShape();

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
            value.Sphere = __shape_Deserialize_Sphere(kv.Value);
            continue;
        }

        if(key=="capsule"){
            value.Capsule = __shape_Deserialize_Capsule(kv.Value);
            continue;
        }

        if(key=="plane"){
            value.Plane = __shape_Deserialize_Plane(kv.Value);
            continue;
        }

    }
    return value;
}

public static ExtendedColliderShapeSphere __shape_Deserialize_Sphere(JsonNode parsed)
{
    var value = new ExtendedColliderShapeSphere();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.Offset = __shape__sphere_Deserialize_Offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.Radius = kv.Value.GetSingle();
            continue;
        }

        if(key=="inside"){
            value.Inside = kv.Value.GetBoolean();
            continue;
        }

    }
    return value;
}

public static float[] __shape__sphere_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static ExtendedColliderShapeCapsule __shape_Deserialize_Capsule(JsonNode parsed)
{
    var value = new ExtendedColliderShapeCapsule();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.Offset = __shape__capsule_Deserialize_Offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.Radius = kv.Value.GetSingle();
            continue;
        }

        if(key=="tail"){
            value.Tail = __shape__capsule_Deserialize_Tail(kv.Value);
            continue;
        }

        if(key=="inside"){
            value.Inside = kv.Value.GetBoolean();
            continue;
        }

    }
    return value;
}

public static float[] __shape__capsule_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __shape__capsule_Deserialize_Tail(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static ExtendedColliderShapePlane __shape_Deserialize_Plane(JsonNode parsed)
{
    var value = new ExtendedColliderShapePlane();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.Offset = __shape__plane_Deserialize_Offset(kv.Value);
            continue;
        }

        if(key=="normal"){
            value.Normal = __shape__plane_Deserialize_Normal(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __shape__plane_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __shape__plane_Deserialize_Normal(JsonNode parsed)
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
