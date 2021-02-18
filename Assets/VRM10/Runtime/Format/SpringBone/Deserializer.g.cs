// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_springBone {

public static class GltfDeserializer
{

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_springBone extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == VRMC_springBone.ExtensionNameUtf8)
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

        if(key=="springs"){
            value.Springs = Deserialize_Springs(kv.Value);
            continue;
        }

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
            value.Joints = Deserialize_Joints(kv.Value);
            continue;
        }

        if(key=="colliders"){
            value.Colliders = Deserialize_Colliders(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<SpringBoneJoint> Deserialize_Joints(JsonNode parsed)
{
    var value = new List<SpringBoneJoint>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_Joints_ITEM(x));
    }
	return value;
} 

public static SpringBoneJoint Deserialize_Joints_ITEM(JsonNode parsed)
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
            value.GravityDir = Deserialize_GravityDir(kv.Value);
            continue;
        }

        if(key=="dragForce"){
            value.DragForce = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_GravityDir(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static int[] Deserialize_Colliders(JsonNode parsed)
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
