// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_springBone {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_springBone.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_springBone extension)
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


public static VRMC_springBone Deserialize(JsonNode parsed)
{
    var value = new VRMC_springBone();

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

        if(key=="colliders"){
            value.Colliders = Deserialize_Colliders(kv.Value);
            continue;
        }

        if(key=="colliderGroups"){
            value.ColliderGroups = Deserialize_ColliderGroups(kv.Value);
            continue;
        }

        if(key=="springs"){
            value.Springs = Deserialize_Springs(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<Collider> Deserialize_Colliders(JsonNode parsed)
{
    var value = new List<Collider>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_Colliders_ITEM(x));
    }
	return value;
} 

public static Collider Deserialize_Colliders_ITEM(JsonNode parsed)
{
    var value = new Collider();

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

        if(key=="shape"){
            value.Shape = __colliders_ITEM_Deserialize_Shape(kv.Value);
            continue;
        }

    }
    return value;
}

public static ColliderShape __colliders_ITEM_Deserialize_Shape(JsonNode parsed)
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
            value.Sphere = __colliders_ITEM__shape_Deserialize_Sphere(kv.Value);
            continue;
        }

        if(key=="capsule"){
            value.Capsule = __colliders_ITEM__shape_Deserialize_Capsule(kv.Value);
            continue;
        }

    }
    return value;
}

public static ColliderShapeSphere __colliders_ITEM__shape_Deserialize_Sphere(JsonNode parsed)
{
    var value = new ColliderShapeSphere();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.Offset = __colliders_ITEM__shape__sphere_Deserialize_Offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.Radius = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] __colliders_ITEM__shape__sphere_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static ColliderShapeCapsule __colliders_ITEM__shape_Deserialize_Capsule(JsonNode parsed)
{
    var value = new ColliderShapeCapsule();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.Offset = __colliders_ITEM__shape__capsule_Deserialize_Offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.Radius = kv.Value.GetSingle();
            continue;
        }

        if(key=="tail"){
            value.Tail = __colliders_ITEM__shape__capsule_Deserialize_Tail(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __colliders_ITEM__shape__capsule_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __colliders_ITEM__shape__capsule_Deserialize_Tail(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<ColliderGroup> Deserialize_ColliderGroups(JsonNode parsed)
{
    var value = new List<ColliderGroup>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_ColliderGroups_ITEM(x));
    }
	return value;
} 

public static ColliderGroup Deserialize_ColliderGroups_ITEM(JsonNode parsed)
{
    var value = new ColliderGroup();

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

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="colliders"){
            value.Colliders = __colliderGroups_ITEM_Deserialize_Colliders(kv.Value);
            continue;
        }

    }
    return value;
}

public static int[] __colliderGroups_ITEM_Deserialize_Colliders(JsonNode parsed)
{
    var value = new int[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static List<Spring> Deserialize_Springs(JsonNode parsed)
{
    var value = new List<Spring>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_Springs_ITEM(x));
    }
	return value;
} 

public static Spring Deserialize_Springs_ITEM(JsonNode parsed)
{
    var value = new Spring();

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

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="joints"){
            value.Joints = __springs_ITEM_Deserialize_Joints(kv.Value);
            continue;
        }

        if(key=="colliderGroups"){
            value.ColliderGroups = __springs_ITEM_Deserialize_ColliderGroups(kv.Value);
            continue;
        }

        if(key=="center"){
            value.Center = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static List<SpringBoneJoint> __springs_ITEM_Deserialize_Joints(JsonNode parsed)
{
    var value = new List<SpringBoneJoint>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__springs_ITEM_Deserialize_Joints_ITEM(x));
    }
	return value;
} 

public static SpringBoneJoint __springs_ITEM_Deserialize_Joints_ITEM(JsonNode parsed)
{
    var value = new SpringBoneJoint();

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

        if(key=="hitRadius"){
            value.HitRadius = kv.Value.GetSingle();
            continue;
        }

        if(key=="stiffness"){
            value.Stiffness = kv.Value.GetSingle();
            continue;
        }

        if(key=="gravityPower"){
            value.GravityPower = kv.Value.GetSingle();
            continue;
        }

        if(key=="gravityDir"){
            value.GravityDir = __springs_ITEM__joints_ITEM_Deserialize_GravityDir(kv.Value);
            continue;
        }

        if(key=="dragForce"){
            value.DragForce = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] __springs_ITEM__joints_ITEM_Deserialize_GravityDir(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static int[] __springs_ITEM_Deserialize_ColliderGroups(JsonNode parsed)
{
    var value = new int[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

} // GltfDeserializer
} // UniGLTF 
