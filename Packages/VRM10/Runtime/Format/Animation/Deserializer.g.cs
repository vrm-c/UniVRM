// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_vrm_animation {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_vrm_animation.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_vrm_animation extension)
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


public static VRMC_vrm_animation Deserialize(JsonNode parsed)
{
    var value = new VRMC_vrm_animation();

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

        if(key=="humanoid"){
            value.Humanoid = Deserialize_Humanoid(kv.Value);
            continue;
        }

        if(key=="expressions"){
            value.Expressions = Deserialize_Expressions(kv.Value);
            continue;
        }

        if(key=="lookAt"){
            value.LookAt = Deserialize_LookAt(kv.Value);
            continue;
        }

    }
    return value;
}

public static Humanoid Deserialize_Humanoid(JsonNode parsed)
{
    var value = new Humanoid();

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

        if(key=="humanBones"){
            value.HumanBones = __humanoid_Deserialize_HumanBones(kv.Value);
            continue;
        }

    }
    return value;
}

public static HumanBones __humanoid_Deserialize_HumanBones(JsonNode parsed)
{
    var value = new HumanBones();

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

        if(key=="hips"){
            value.Hips = __humanoid__humanBones_Deserialize_Hips(kv.Value);
            continue;
        }

        if(key=="spine"){
            value.Spine = __humanoid__humanBones_Deserialize_Spine(kv.Value);
            continue;
        }

        if(key=="chest"){
            value.Chest = __humanoid__humanBones_Deserialize_Chest(kv.Value);
            continue;
        }

        if(key=="upperChest"){
            value.UpperChest = __humanoid__humanBones_Deserialize_UpperChest(kv.Value);
            continue;
        }

        if(key=="neck"){
            value.Neck = __humanoid__humanBones_Deserialize_Neck(kv.Value);
            continue;
        }

        if(key=="head"){
            value.Head = __humanoid__humanBones_Deserialize_Head(kv.Value);
            continue;
        }

        if(key=="jaw"){
            value.Jaw = __humanoid__humanBones_Deserialize_Jaw(kv.Value);
            continue;
        }

        if(key=="leftUpperLeg"){
            value.LeftUpperLeg = __humanoid__humanBones_Deserialize_LeftUpperLeg(kv.Value);
            continue;
        }

        if(key=="leftLowerLeg"){
            value.LeftLowerLeg = __humanoid__humanBones_Deserialize_LeftLowerLeg(kv.Value);
            continue;
        }

        if(key=="leftFoot"){
            value.LeftFoot = __humanoid__humanBones_Deserialize_LeftFoot(kv.Value);
            continue;
        }

        if(key=="leftToes"){
            value.LeftToes = __humanoid__humanBones_Deserialize_LeftToes(kv.Value);
            continue;
        }

        if(key=="rightUpperLeg"){
            value.RightUpperLeg = __humanoid__humanBones_Deserialize_RightUpperLeg(kv.Value);
            continue;
        }

        if(key=="rightLowerLeg"){
            value.RightLowerLeg = __humanoid__humanBones_Deserialize_RightLowerLeg(kv.Value);
            continue;
        }

        if(key=="rightFoot"){
            value.RightFoot = __humanoid__humanBones_Deserialize_RightFoot(kv.Value);
            continue;
        }

        if(key=="rightToes"){
            value.RightToes = __humanoid__humanBones_Deserialize_RightToes(kv.Value);
            continue;
        }

        if(key=="leftShoulder"){
            value.LeftShoulder = __humanoid__humanBones_Deserialize_LeftShoulder(kv.Value);
            continue;
        }

        if(key=="leftUpperArm"){
            value.LeftUpperArm = __humanoid__humanBones_Deserialize_LeftUpperArm(kv.Value);
            continue;
        }

        if(key=="leftLowerArm"){
            value.LeftLowerArm = __humanoid__humanBones_Deserialize_LeftLowerArm(kv.Value);
            continue;
        }

        if(key=="leftHand"){
            value.LeftHand = __humanoid__humanBones_Deserialize_LeftHand(kv.Value);
            continue;
        }

        if(key=="rightShoulder"){
            value.RightShoulder = __humanoid__humanBones_Deserialize_RightShoulder(kv.Value);
            continue;
        }

        if(key=="rightUpperArm"){
            value.RightUpperArm = __humanoid__humanBones_Deserialize_RightUpperArm(kv.Value);
            continue;
        }

        if(key=="rightLowerArm"){
            value.RightLowerArm = __humanoid__humanBones_Deserialize_RightLowerArm(kv.Value);
            continue;
        }

        if(key=="rightHand"){
            value.RightHand = __humanoid__humanBones_Deserialize_RightHand(kv.Value);
            continue;
        }

        if(key=="leftThumbMetacarpal"){
            value.LeftThumbMetacarpal = __humanoid__humanBones_Deserialize_LeftThumbMetacarpal(kv.Value);
            continue;
        }

        if(key=="leftThumbProximal"){
            value.LeftThumbProximal = __humanoid__humanBones_Deserialize_LeftThumbProximal(kv.Value);
            continue;
        }

        if(key=="leftThumbDistal"){
            value.LeftThumbDistal = __humanoid__humanBones_Deserialize_LeftThumbDistal(kv.Value);
            continue;
        }

        if(key=="leftIndexProximal"){
            value.LeftIndexProximal = __humanoid__humanBones_Deserialize_LeftIndexProximal(kv.Value);
            continue;
        }

        if(key=="leftIndexIntermediate"){
            value.LeftIndexIntermediate = __humanoid__humanBones_Deserialize_LeftIndexIntermediate(kv.Value);
            continue;
        }

        if(key=="leftIndexDistal"){
            value.LeftIndexDistal = __humanoid__humanBones_Deserialize_LeftIndexDistal(kv.Value);
            continue;
        }

        if(key=="leftMiddleProximal"){
            value.LeftMiddleProximal = __humanoid__humanBones_Deserialize_LeftMiddleProximal(kv.Value);
            continue;
        }

        if(key=="leftMiddleIntermediate"){
            value.LeftMiddleIntermediate = __humanoid__humanBones_Deserialize_LeftMiddleIntermediate(kv.Value);
            continue;
        }

        if(key=="leftMiddleDistal"){
            value.LeftMiddleDistal = __humanoid__humanBones_Deserialize_LeftMiddleDistal(kv.Value);
            continue;
        }

        if(key=="leftRingProximal"){
            value.LeftRingProximal = __humanoid__humanBones_Deserialize_LeftRingProximal(kv.Value);
            continue;
        }

        if(key=="leftRingIntermediate"){
            value.LeftRingIntermediate = __humanoid__humanBones_Deserialize_LeftRingIntermediate(kv.Value);
            continue;
        }

        if(key=="leftRingDistal"){
            value.LeftRingDistal = __humanoid__humanBones_Deserialize_LeftRingDistal(kv.Value);
            continue;
        }

        if(key=="leftLittleProximal"){
            value.LeftLittleProximal = __humanoid__humanBones_Deserialize_LeftLittleProximal(kv.Value);
            continue;
        }

        if(key=="leftLittleIntermediate"){
            value.LeftLittleIntermediate = __humanoid__humanBones_Deserialize_LeftLittleIntermediate(kv.Value);
            continue;
        }

        if(key=="leftLittleDistal"){
            value.LeftLittleDistal = __humanoid__humanBones_Deserialize_LeftLittleDistal(kv.Value);
            continue;
        }

        if(key=="rightThumbMetacarpal"){
            value.RightThumbMetacarpal = __humanoid__humanBones_Deserialize_RightThumbMetacarpal(kv.Value);
            continue;
        }

        if(key=="rightThumbProximal"){
            value.RightThumbProximal = __humanoid__humanBones_Deserialize_RightThumbProximal(kv.Value);
            continue;
        }

        if(key=="rightThumbDistal"){
            value.RightThumbDistal = __humanoid__humanBones_Deserialize_RightThumbDistal(kv.Value);
            continue;
        }

        if(key=="rightIndexProximal"){
            value.RightIndexProximal = __humanoid__humanBones_Deserialize_RightIndexProximal(kv.Value);
            continue;
        }

        if(key=="rightIndexIntermediate"){
            value.RightIndexIntermediate = __humanoid__humanBones_Deserialize_RightIndexIntermediate(kv.Value);
            continue;
        }

        if(key=="rightIndexDistal"){
            value.RightIndexDistal = __humanoid__humanBones_Deserialize_RightIndexDistal(kv.Value);
            continue;
        }

        if(key=="rightMiddleProximal"){
            value.RightMiddleProximal = __humanoid__humanBones_Deserialize_RightMiddleProximal(kv.Value);
            continue;
        }

        if(key=="rightMiddleIntermediate"){
            value.RightMiddleIntermediate = __humanoid__humanBones_Deserialize_RightMiddleIntermediate(kv.Value);
            continue;
        }

        if(key=="rightMiddleDistal"){
            value.RightMiddleDistal = __humanoid__humanBones_Deserialize_RightMiddleDistal(kv.Value);
            continue;
        }

        if(key=="rightRingProximal"){
            value.RightRingProximal = __humanoid__humanBones_Deserialize_RightRingProximal(kv.Value);
            continue;
        }

        if(key=="rightRingIntermediate"){
            value.RightRingIntermediate = __humanoid__humanBones_Deserialize_RightRingIntermediate(kv.Value);
            continue;
        }

        if(key=="rightRingDistal"){
            value.RightRingDistal = __humanoid__humanBones_Deserialize_RightRingDistal(kv.Value);
            continue;
        }

        if(key=="rightLittleProximal"){
            value.RightLittleProximal = __humanoid__humanBones_Deserialize_RightLittleProximal(kv.Value);
            continue;
        }

        if(key=="rightLittleIntermediate"){
            value.RightLittleIntermediate = __humanoid__humanBones_Deserialize_RightLittleIntermediate(kv.Value);
            continue;
        }

        if(key=="rightLittleDistal"){
            value.RightLittleDistal = __humanoid__humanBones_Deserialize_RightLittleDistal(kv.Value);
            continue;
        }

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_Hips(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_Spine(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_Chest(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_UpperChest(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_Neck(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_Head(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_Jaw(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftUpperLeg(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftLowerLeg(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftFoot(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftToes(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightUpperLeg(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightLowerLeg(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightFoot(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightToes(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftShoulder(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftUpperArm(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftLowerArm(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftHand(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightShoulder(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightUpperArm(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightLowerArm(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightHand(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftThumbMetacarpal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftThumbProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftThumbDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftIndexProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftIndexIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftIndexDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftMiddleProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftMiddleIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftMiddleDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftRingProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftRingIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftRingDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftLittleProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftLittleIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_LeftLittleDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightThumbMetacarpal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightThumbProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightThumbDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightIndexProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightIndexIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightIndexDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightMiddleProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightMiddleIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightMiddleDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightRingProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightRingIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightRingDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightLittleProximal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightLittleIntermediate(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static HumanBone __humanoid__humanBones_Deserialize_RightLittleDistal(JsonNode parsed)
{
    var value = new HumanBone();

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

    }
    return value;
}

public static Expressions Deserialize_Expressions(JsonNode parsed)
{
    var value = new Expressions();

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

        if(key=="preset"){
            value.Preset = __expressions_Deserialize_Preset(kv.Value);
            continue;
        }

        if(key=="custom"){
            value.Custom = __expressions_Deserialize_Custom(kv.Value);
            continue;
        }

    }
    return value;
}

public static Preset __expressions_Deserialize_Preset(JsonNode parsed)
{
    var value = new Preset();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="happy"){
            value.Happy = __expressions__preset_Deserialize_Happy(kv.Value);
            continue;
        }

        if(key=="angry"){
            value.Angry = __expressions__preset_Deserialize_Angry(kv.Value);
            continue;
        }

        if(key=="sad"){
            value.Sad = __expressions__preset_Deserialize_Sad(kv.Value);
            continue;
        }

        if(key=="relaxed"){
            value.Relaxed = __expressions__preset_Deserialize_Relaxed(kv.Value);
            continue;
        }

        if(key=="surprised"){
            value.Surprised = __expressions__preset_Deserialize_Surprised(kv.Value);
            continue;
        }

        if(key=="aa"){
            value.Aa = __expressions__preset_Deserialize_Aa(kv.Value);
            continue;
        }

        if(key=="ih"){
            value.Ih = __expressions__preset_Deserialize_Ih(kv.Value);
            continue;
        }

        if(key=="ou"){
            value.Ou = __expressions__preset_Deserialize_Ou(kv.Value);
            continue;
        }

        if(key=="ee"){
            value.Ee = __expressions__preset_Deserialize_Ee(kv.Value);
            continue;
        }

        if(key=="oh"){
            value.Oh = __expressions__preset_Deserialize_Oh(kv.Value);
            continue;
        }

        if(key=="blink"){
            value.Blink = __expressions__preset_Deserialize_Blink(kv.Value);
            continue;
        }

        if(key=="blinkLeft"){
            value.BlinkLeft = __expressions__preset_Deserialize_BlinkLeft(kv.Value);
            continue;
        }

        if(key=="blinkRight"){
            value.BlinkRight = __expressions__preset_Deserialize_BlinkRight(kv.Value);
            continue;
        }

        if(key=="neutral"){
            value.Neutral = __expressions__preset_Deserialize_Neutral(kv.Value);
            continue;
        }

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Happy(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Angry(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Sad(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Relaxed(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Surprised(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Aa(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Ih(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Ou(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Ee(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Oh(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Blink(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_BlinkLeft(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_BlinkRight(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Expression __expressions__preset_Deserialize_Neutral(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static Dictionary<string, Expression> __expressions_Deserialize_Custom(JsonNode parsed)
{
    var value = new Dictionary<string, Expression>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), __expressions_Deserialize_Custom_ITEM(kv.Value));
    }
	return value;
} 

public static Expression __expressions_Deserialize_Custom_ITEM(JsonNode parsed)
{
    var value = new Expression();

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

    }
    return value;
}

public static LookAt Deserialize_LookAt(JsonNode parsed)
{
    var value = new LookAt();

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

        if(key=="offsetFromHeadBone"){
            value.OffsetFromHeadBone = __lookAt_Deserialize_OffsetFromHeadBone(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __lookAt_Deserialize_OffsetFromHeadBone(JsonNode parsed)
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
