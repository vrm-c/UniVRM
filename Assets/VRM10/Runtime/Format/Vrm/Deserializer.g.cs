// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_vrm {

public static class GltfDeserializer
{

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_vrm extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == VRMC_vrm.ExtensionNameUtf8)
            {
                extension = Deserialize(kv.Value);
                return true;
            }
        }
    }

    extension = default;
    return false;
}


public static VRMC_vrm Deserialize(ListTreeNode<JsonValue> parsed)
{
    var value = new VRMC_vrm();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="specVersion"){
            value.SpecVersion = kv.Value.GetString();
            continue;
        }

        if(key=="meta"){
            value.Meta = Deserialize_Meta(kv.Value);
            continue;
        }

        if(key=="humanoid"){
            value.Humanoid = Deserialize_Humanoid(kv.Value);
            continue;
        }

        if(key=="firstPerson"){
            value.FirstPerson = Deserialize_FirstPerson(kv.Value);
            continue;
        }

        if(key=="lookAt"){
            value.LookAt = Deserialize_LookAt(kv.Value);
            continue;
        }

        if(key=="expressions"){
            value.Expressions = Deserialize_Expressions(kv.Value);
            continue;
        }

    }
    return value;
}

public static Meta Deserialize_Meta(ListTreeNode<JsonValue> parsed)
{
    var value = new Meta();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="version"){
            value.Version = kv.Value.GetString();
            continue;
        }

        if(key=="authors"){
            value.Authors = Deserialize_Authors(kv.Value);
            continue;
        }

        if(key=="copyrightInformation"){
            value.CopyrightInformation = kv.Value.GetString();
            continue;
        }

        if(key=="contactInformation"){
            value.ContactInformation = kv.Value.GetString();
            continue;
        }

        if(key=="references"){
            value.References = Deserialize_References(kv.Value);
            continue;
        }

        if(key=="thirdPartyLicenses"){
            value.ThirdPartyLicenses = kv.Value.GetString();
            continue;
        }

        if(key=="thumbnailImage"){
            value.ThumbnailImage = kv.Value.GetInt32();
            continue;
        }

        if(key=="avatarPermission"){
            value.AvatarPermission = (AvatarPermissionType)Enum.Parse(typeof(AvatarPermissionType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="allowExcessivelyViolentUsage"){
            value.AllowExcessivelyViolentUsage = kv.Value.GetBoolean();
            continue;
        }

        if(key=="allowExcessivelySexualUsage"){
            value.AllowExcessivelySexualUsage = kv.Value.GetBoolean();
            continue;
        }

        if(key=="commercialUsage"){
            value.CommercialUsage = (CommercialUsageType)Enum.Parse(typeof(CommercialUsageType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="allowPoliticalOrReligiousUsage"){
            value.AllowPoliticalOrReligiousUsage = kv.Value.GetBoolean();
            continue;
        }

        if(key=="creditNotation"){
            value.CreditNotation = (CreditNotationType)Enum.Parse(typeof(CreditNotationType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="allowRedistribution"){
            value.AllowRedistribution = kv.Value.GetBoolean();
            continue;
        }

        if(key=="modification"){
            value.Modification = (ModificationType)Enum.Parse(typeof(ModificationType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="otherLicenseUrl"){
            value.OtherLicenseUrl = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<string> Deserialize_Authors(ListTreeNode<JsonValue> parsed)
{
    var value = new List<string>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
} 

public static List<string> Deserialize_References(ListTreeNode<JsonValue> parsed)
{
    var value = new List<string>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
} 

public static Humanoid Deserialize_Humanoid(ListTreeNode<JsonValue> parsed)
{
    var value = new Humanoid();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="humanBones"){
            value.HumanBones = Deserialize_HumanBones(kv.Value);
            continue;
        }

    }
    return value;
}

public static HumanBones Deserialize_HumanBones(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBones();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="hips"){
            value.Hips = Deserialize_Hips(kv.Value);
            continue;
        }

        if(key=="spine"){
            value.Spine = Deserialize_Spine(kv.Value);
            continue;
        }

        if(key=="chest"){
            value.Chest = Deserialize_Chest(kv.Value);
            continue;
        }

        if(key=="upperChest"){
            value.UpperChest = Deserialize_UpperChest(kv.Value);
            continue;
        }

        if(key=="neck"){
            value.Neck = Deserialize_Neck(kv.Value);
            continue;
        }

        if(key=="head"){
            value.Head = Deserialize_Head(kv.Value);
            continue;
        }

        if(key=="leftEye"){
            value.LeftEye = Deserialize_LeftEye(kv.Value);
            continue;
        }

        if(key=="rightEye"){
            value.RightEye = Deserialize_RightEye(kv.Value);
            continue;
        }

        if(key=="jaw"){
            value.Jaw = Deserialize_Jaw(kv.Value);
            continue;
        }

        if(key=="leftUpperLeg"){
            value.LeftUpperLeg = Deserialize_LeftUpperLeg(kv.Value);
            continue;
        }

        if(key=="leftLowerLeg"){
            value.LeftLowerLeg = Deserialize_LeftLowerLeg(kv.Value);
            continue;
        }

        if(key=="leftFoot"){
            value.LeftFoot = Deserialize_LeftFoot(kv.Value);
            continue;
        }

        if(key=="leftToes"){
            value.LeftToes = Deserialize_LeftToes(kv.Value);
            continue;
        }

        if(key=="rightUpperLeg"){
            value.RightUpperLeg = Deserialize_RightUpperLeg(kv.Value);
            continue;
        }

        if(key=="rightLowerLeg"){
            value.RightLowerLeg = Deserialize_RightLowerLeg(kv.Value);
            continue;
        }

        if(key=="rightFoot"){
            value.RightFoot = Deserialize_RightFoot(kv.Value);
            continue;
        }

        if(key=="rightToes"){
            value.RightToes = Deserialize_RightToes(kv.Value);
            continue;
        }

        if(key=="leftShoulder"){
            value.LeftShoulder = Deserialize_LeftShoulder(kv.Value);
            continue;
        }

        if(key=="leftUpperArm"){
            value.LeftUpperArm = Deserialize_LeftUpperArm(kv.Value);
            continue;
        }

        if(key=="leftLowerArm"){
            value.LeftLowerArm = Deserialize_LeftLowerArm(kv.Value);
            continue;
        }

        if(key=="leftHand"){
            value.LeftHand = Deserialize_LeftHand(kv.Value);
            continue;
        }

        if(key=="rightShoulder"){
            value.RightShoulder = Deserialize_RightShoulder(kv.Value);
            continue;
        }

        if(key=="rightUpperArm"){
            value.RightUpperArm = Deserialize_RightUpperArm(kv.Value);
            continue;
        }

        if(key=="rightLowerArm"){
            value.RightLowerArm = Deserialize_RightLowerArm(kv.Value);
            continue;
        }

        if(key=="rightHand"){
            value.RightHand = Deserialize_RightHand(kv.Value);
            continue;
        }

        if(key=="leftThumbProximal"){
            value.LeftThumbProximal = Deserialize_LeftThumbProximal(kv.Value);
            continue;
        }

        if(key=="leftThumbIntermediate"){
            value.LeftThumbIntermediate = Deserialize_LeftThumbIntermediate(kv.Value);
            continue;
        }

        if(key=="leftThumbDistal"){
            value.LeftThumbDistal = Deserialize_LeftThumbDistal(kv.Value);
            continue;
        }

        if(key=="leftIndexProximal"){
            value.LeftIndexProximal = Deserialize_LeftIndexProximal(kv.Value);
            continue;
        }

        if(key=="leftIndexIntermediate"){
            value.LeftIndexIntermediate = Deserialize_LeftIndexIntermediate(kv.Value);
            continue;
        }

        if(key=="leftIndexDistal"){
            value.LeftIndexDistal = Deserialize_LeftIndexDistal(kv.Value);
            continue;
        }

        if(key=="leftMiddleProximal"){
            value.LeftMiddleProximal = Deserialize_LeftMiddleProximal(kv.Value);
            continue;
        }

        if(key=="leftMiddleIntermediate"){
            value.LeftMiddleIntermediate = Deserialize_LeftMiddleIntermediate(kv.Value);
            continue;
        }

        if(key=="leftMiddleDistal"){
            value.LeftMiddleDistal = Deserialize_LeftMiddleDistal(kv.Value);
            continue;
        }

        if(key=="leftRingProximal"){
            value.LeftRingProximal = Deserialize_LeftRingProximal(kv.Value);
            continue;
        }

        if(key=="leftRingIntermediate"){
            value.LeftRingIntermediate = Deserialize_LeftRingIntermediate(kv.Value);
            continue;
        }

        if(key=="leftRingDistal"){
            value.LeftRingDistal = Deserialize_LeftRingDistal(kv.Value);
            continue;
        }

        if(key=="leftLittleProximal"){
            value.LeftLittleProximal = Deserialize_LeftLittleProximal(kv.Value);
            continue;
        }

        if(key=="leftLittleIntermediate"){
            value.LeftLittleIntermediate = Deserialize_LeftLittleIntermediate(kv.Value);
            continue;
        }

        if(key=="leftLittleDistal"){
            value.LeftLittleDistal = Deserialize_LeftLittleDistal(kv.Value);
            continue;
        }

        if(key=="rightThumbProximal"){
            value.RightThumbProximal = Deserialize_RightThumbProximal(kv.Value);
            continue;
        }

        if(key=="rightThumbIntermediate"){
            value.RightThumbIntermediate = Deserialize_RightThumbIntermediate(kv.Value);
            continue;
        }

        if(key=="rightThumbDistal"){
            value.RightThumbDistal = Deserialize_RightThumbDistal(kv.Value);
            continue;
        }

        if(key=="rightIndexProximal"){
            value.RightIndexProximal = Deserialize_RightIndexProximal(kv.Value);
            continue;
        }

        if(key=="rightIndexIntermediate"){
            value.RightIndexIntermediate = Deserialize_RightIndexIntermediate(kv.Value);
            continue;
        }

        if(key=="rightIndexDistal"){
            value.RightIndexDistal = Deserialize_RightIndexDistal(kv.Value);
            continue;
        }

        if(key=="rightMiddleProximal"){
            value.RightMiddleProximal = Deserialize_RightMiddleProximal(kv.Value);
            continue;
        }

        if(key=="rightMiddleIntermediate"){
            value.RightMiddleIntermediate = Deserialize_RightMiddleIntermediate(kv.Value);
            continue;
        }

        if(key=="rightMiddleDistal"){
            value.RightMiddleDistal = Deserialize_RightMiddleDistal(kv.Value);
            continue;
        }

        if(key=="rightRingProximal"){
            value.RightRingProximal = Deserialize_RightRingProximal(kv.Value);
            continue;
        }

        if(key=="rightRingIntermediate"){
            value.RightRingIntermediate = Deserialize_RightRingIntermediate(kv.Value);
            continue;
        }

        if(key=="rightRingDistal"){
            value.RightRingDistal = Deserialize_RightRingDistal(kv.Value);
            continue;
        }

        if(key=="rightLittleProximal"){
            value.RightLittleProximal = Deserialize_RightLittleProximal(kv.Value);
            continue;
        }

        if(key=="rightLittleIntermediate"){
            value.RightLittleIntermediate = Deserialize_RightLittleIntermediate(kv.Value);
            continue;
        }

        if(key=="rightLittleDistal"){
            value.RightLittleDistal = Deserialize_RightLittleDistal(kv.Value);
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_Hips(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_Spine(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_Chest(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_UpperChest(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_Neck(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_Head(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftEye(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightEye(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_Jaw(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftUpperLeg(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftLowerLeg(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftFoot(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftToes(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightUpperLeg(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightLowerLeg(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightFoot(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightToes(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftShoulder(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftUpperArm(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftLowerArm(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftHand(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightShoulder(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightUpperArm(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightLowerArm(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightHand(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftThumbProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftThumbIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftThumbDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftIndexProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftIndexIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftIndexDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftMiddleProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftMiddleIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftMiddleDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftRingProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftRingIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftRingDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftLittleProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftLittleIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_LeftLittleDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightThumbProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightThumbIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightThumbDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightIndexProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightIndexIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightIndexDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightMiddleProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightMiddleIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightMiddleDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightRingProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightRingIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightRingDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightLittleProximal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightLittleIntermediate(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static HumanBone Deserialize_RightLittleDistal(ListTreeNode<JsonValue> parsed)
{
    var value = new HumanBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static FirstPerson Deserialize_FirstPerson(ListTreeNode<JsonValue> parsed)
{
    var value = new FirstPerson();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="meshAnnotations"){
            value.MeshAnnotations = Deserialize_MeshAnnotations(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<MeshAnnotation> Deserialize_MeshAnnotations(ListTreeNode<JsonValue> parsed)
{
    var value = new List<MeshAnnotation>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_MeshAnnotations_ITEM(x));
    }
	return value;
} 

public static MeshAnnotation Deserialize_MeshAnnotations_ITEM(ListTreeNode<JsonValue> parsed)
{
    var value = new MeshAnnotation();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.Node = kv.Value.GetInt32();
            continue;
        }

        if(key=="firstPersonType"){
            value.FirstPersonType = (FirstPersonType)Enum.Parse(typeof(FirstPersonType), kv.Value.GetString(), true);
            continue;
        }

    }
    return value;
}

public static LookAt Deserialize_LookAt(ListTreeNode<JsonValue> parsed)
{
    var value = new LookAt();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offsetFromHeadBone"){
            value.OffsetFromHeadBone = Deserialize_OffsetFromHeadBone(kv.Value);
            continue;
        }

        if(key=="lookAtType"){
            value.LookAtType = (LookAtType)Enum.Parse(typeof(LookAtType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="lookAtHorizontalInner"){
            value.LookAtHorizontalInner = Deserialize_LookAtHorizontalInner(kv.Value);
            continue;
        }

        if(key=="lookAtHorizontalOuter"){
            value.LookAtHorizontalOuter = Deserialize_LookAtHorizontalOuter(kv.Value);
            continue;
        }

        if(key=="lookAtVerticalDown"){
            value.LookAtVerticalDown = Deserialize_LookAtVerticalDown(kv.Value);
            continue;
        }

        if(key=="lookAtVerticalUp"){
            value.LookAtVerticalUp = Deserialize_LookAtVerticalUp(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_OffsetFromHeadBone(ListTreeNode<JsonValue> parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static LookAtRangeMap Deserialize_LookAtHorizontalInner(ListTreeNode<JsonValue> parsed)
{
    var value = new LookAtRangeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="inputMaxValue"){
            value.InputMaxValue = kv.Value.GetSingle();
            continue;
        }

        if(key=="outputScale"){
            value.OutputScale = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static LookAtRangeMap Deserialize_LookAtHorizontalOuter(ListTreeNode<JsonValue> parsed)
{
    var value = new LookAtRangeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="inputMaxValue"){
            value.InputMaxValue = kv.Value.GetSingle();
            continue;
        }

        if(key=="outputScale"){
            value.OutputScale = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static LookAtRangeMap Deserialize_LookAtVerticalDown(ListTreeNode<JsonValue> parsed)
{
    var value = new LookAtRangeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="inputMaxValue"){
            value.InputMaxValue = kv.Value.GetSingle();
            continue;
        }

        if(key=="outputScale"){
            value.OutputScale = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static LookAtRangeMap Deserialize_LookAtVerticalUp(ListTreeNode<JsonValue> parsed)
{
    var value = new LookAtRangeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="inputMaxValue"){
            value.InputMaxValue = kv.Value.GetSingle();
            continue;
        }

        if(key=="outputScale"){
            value.OutputScale = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<Expression> Deserialize_Expressions(ListTreeNode<JsonValue> parsed)
{
    var value = new List<Expression>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_Expressions_ITEM(x));
    }
	return value;
} 

public static Expression Deserialize_Expressions_ITEM(ListTreeNode<JsonValue> parsed)
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

        if(key=="name"){
            value.Name = kv.Value.GetString();
            continue;
        }

        if(key=="preset"){
            value.Preset = (ExpressionPreset)Enum.Parse(typeof(ExpressionPreset), kv.Value.GetString(), true);
            continue;
        }

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = Deserialize_TextureTransformBinds(kv.Value);
            continue;
        }

        if(key=="isBinary"){
            value.IsBinary = kv.Value.GetBoolean();
            continue;
        }

        if(key=="overrideBlink"){
            value.OverrideBlink = (ExpressionOverrideType)Enum.Parse(typeof(ExpressionOverrideType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="overrideLookAt"){
            value.OverrideLookAt = (ExpressionOverrideType)Enum.Parse(typeof(ExpressionOverrideType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="overrideMouth"){
            value.OverrideMouth = (ExpressionOverrideType)Enum.Parse(typeof(ExpressionOverrideType), kv.Value.GetString(), true);
            continue;
        }

    }
    return value;
}

public static List<MorphTargetBind> Deserialize_MorphTargetBinds(ListTreeNode<JsonValue> parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind Deserialize_MorphTargetBinds_ITEM(ListTreeNode<JsonValue> parsed)
{
    var value = new MorphTargetBind();

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

        if(key=="index"){
            value.Index = kv.Value.GetInt32();
            continue;
        }

        if(key=="weight"){
            value.Weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<MaterialColorBind> Deserialize_MaterialColorBinds(ListTreeNode<JsonValue> parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind Deserialize_MaterialColorBinds_ITEM(ListTreeNode<JsonValue> parsed)
{
    var value = new MaterialColorBind();

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

        if(key=="material"){
            value.Material = kv.Value.GetInt32();
            continue;
        }

        if(key=="type"){
            value.Type = (MaterialColorType)Enum.Parse(typeof(MaterialColorType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="targetValue"){
            value.TargetValue = Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_TargetValue(ListTreeNode<JsonValue> parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> Deserialize_TextureTransformBinds(ListTreeNode<JsonValue> parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind Deserialize_TextureTransformBinds_ITEM(ListTreeNode<JsonValue> parsed)
{
    var value = new TextureTransformBind();

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

        if(key=="material"){
            value.Material = kv.Value.GetInt32();
            continue;
        }

        if(key=="scaling"){
            value.Scaling = Deserialize_Scaling(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] Deserialize_Scaling(ListTreeNode<JsonValue> parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] Deserialize_Offset(ListTreeNode<JsonValue> parsed)
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
