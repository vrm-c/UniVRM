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


public static VRMC_springBone Deserialize(ListTreeNode<JsonValue> parsed)
{
    var value = new VRMC_springBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="settings"){
            value.Settings = Deserialize_Settings(kv.Value);
            continue;
        }

        if(key=="springs"){
            value.Springs = Deserialize_Springs(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<SpringSetting> Deserialize_Settings(ListTreeNode<JsonValue> parsed)
{
    var value = new List<SpringSetting>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_Settings_ITEM(x));
    }
	return value;
} 

public static SpringSetting Deserialize_Settings_ITEM(ListTreeNode<JsonValue> parsed)
{
    var value = new SpringSetting();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

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

public static float[] Deserialize_GravityDir(ListTreeNode<JsonValue> parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<Spring> Deserialize_Springs(ListTreeNode<JsonValue> parsed)
{
    var value = new List<Spring>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_Springs_ITEM(x));
    }
	return value;
} 

public static Spring Deserialize_Springs_ITEM(ListTreeNode<JsonValue> parsed)
{
    var value = new Spring();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="setting"){
            value.Setting = kv.Value.GetInt32();
            continue;
        }

        if(key=="springRoot"){
            value.SpringRoot = kv.Value.GetInt32();
            continue;
        }

        if(key=="hitRadius"){
            value.HitRadius = kv.Value.GetSingle();
            continue;
        }

        if(key=="colliders"){
            value.Colliders = Deserialize_Colliders(kv.Value);
            continue;
        }

    }
    return value;
}

public static int[] Deserialize_Colliders(ListTreeNode<JsonValue> parsed)
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
