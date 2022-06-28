// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.VRMC_vrm {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(VRMC_vrm.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out VRMC_vrm extension)
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


public static VRMC_vrm Deserialize(JsonNode parsed)
{
    var value = new VRMC_vrm();

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

public static Meta Deserialize_Meta(JsonNode parsed)
{
    var value = new Meta();

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

        if(key=="version"){
            value.Version = kv.Value.GetString();
            continue;
        }

        if(key=="authors"){
            value.Authors = __meta_Deserialize_Authors(kv.Value);
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
            value.References = __meta_Deserialize_References(kv.Value);
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

        if(key=="licenseUrl"){
            value.LicenseUrl = kv.Value.GetString();
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

        if(key=="allowAntisocialOrHateUsage"){
            value.AllowAntisocialOrHateUsage = kv.Value.GetBoolean();
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

public static List<string> __meta_Deserialize_Authors(JsonNode parsed)
{
    var value = new List<string>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
} 

public static List<string> __meta_Deserialize_References(JsonNode parsed)
{
    var value = new List<string>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
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

        if(key=="leftEye"){
            value.LeftEye = __humanoid__humanBones_Deserialize_LeftEye(kv.Value);
            continue;
        }

        if(key=="rightEye"){
            value.RightEye = __humanoid__humanBones_Deserialize_RightEye(kv.Value);
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

public static HumanBone __humanoid__humanBones_Deserialize_LeftEye(JsonNode parsed)
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

public static HumanBone __humanoid__humanBones_Deserialize_RightEye(JsonNode parsed)
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

public static FirstPerson Deserialize_FirstPerson(JsonNode parsed)
{
    var value = new FirstPerson();

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

        if(key=="meshAnnotations"){
            value.MeshAnnotations = __firstPerson_Deserialize_MeshAnnotations(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<MeshAnnotation> __firstPerson_Deserialize_MeshAnnotations(JsonNode parsed)
{
    var value = new List<MeshAnnotation>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__firstPerson_Deserialize_MeshAnnotations_ITEM(x));
    }
	return value;
} 

public static MeshAnnotation __firstPerson_Deserialize_MeshAnnotations_ITEM(JsonNode parsed)
{
    var value = new MeshAnnotation();

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

        if(key=="type"){
            value.Type = (FirstPersonType)Enum.Parse(typeof(FirstPersonType), kv.Value.GetString(), true);
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

        if(key=="offsetFromHeadBone"){
            value.OffsetFromHeadBone = __lookAt_Deserialize_OffsetFromHeadBone(kv.Value);
            continue;
        }

        if(key=="type"){
            value.Type = (LookAtType)Enum.Parse(typeof(LookAtType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="rangeMapHorizontalInner"){
            value.RangeMapHorizontalInner = __lookAt_Deserialize_RangeMapHorizontalInner(kv.Value);
            continue;
        }

        if(key=="rangeMapHorizontalOuter"){
            value.RangeMapHorizontalOuter = __lookAt_Deserialize_RangeMapHorizontalOuter(kv.Value);
            continue;
        }

        if(key=="rangeMapVerticalDown"){
            value.RangeMapVerticalDown = __lookAt_Deserialize_RangeMapVerticalDown(kv.Value);
            continue;
        }

        if(key=="rangeMapVerticalUp"){
            value.RangeMapVerticalUp = __lookAt_Deserialize_RangeMapVerticalUp(kv.Value);
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

public static LookAtRangeMap __lookAt_Deserialize_RangeMapHorizontalInner(JsonNode parsed)
{
    var value = new LookAtRangeMap();

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

public static LookAtRangeMap __lookAt_Deserialize_RangeMapHorizontalOuter(JsonNode parsed)
{
    var value = new LookAtRangeMap();

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

public static LookAtRangeMap __lookAt_Deserialize_RangeMapVerticalDown(JsonNode parsed)
{
    var value = new LookAtRangeMap();

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

public static LookAtRangeMap __lookAt_Deserialize_RangeMapVerticalUp(JsonNode parsed)
{
    var value = new LookAtRangeMap();

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

        if(key=="lookUp"){
            value.LookUp = __expressions__preset_Deserialize_LookUp(kv.Value);
            continue;
        }

        if(key=="lookDown"){
            value.LookDown = __expressions__preset_Deserialize_LookDown(kv.Value);
            continue;
        }

        if(key=="lookLeft"){
            value.LookLeft = __expressions__preset_Deserialize_LookLeft(kv.Value);
            continue;
        }

        if(key=="lookRight"){
            value.LookRight = __expressions__preset_Deserialize_LookRight(kv.Value);
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__happy_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__happy_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__happy_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__happy_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__happy_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__happy_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__happy_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__happy_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__happy_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__happy__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__happy__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__happy_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__happy_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__happy_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__happy__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__happy__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__happy__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__happy__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__angry_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__angry_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__angry_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__angry_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__angry_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__angry_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__angry_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__angry_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__angry_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__angry__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__angry__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__angry_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__angry_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__angry_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__angry__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__angry__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__angry__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__angry__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__sad_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__sad_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__sad_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__sad_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__sad_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__sad_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__sad_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__sad_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__sad_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__sad__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__sad__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__sad_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__sad_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__sad_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__sad__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__sad__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__sad__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__sad__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__relaxed_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__relaxed_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__relaxed_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__relaxed_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__relaxed_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__relaxed_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__relaxed_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__relaxed_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__relaxed_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__relaxed__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__relaxed__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__relaxed_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__relaxed_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__relaxed_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__relaxed__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__relaxed__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__relaxed__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__relaxed__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__surprised_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__surprised_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__surprised_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__surprised_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__surprised_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__surprised_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__surprised_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__surprised_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__surprised_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__surprised__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__surprised__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__surprised_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__surprised_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__surprised_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__surprised__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__surprised__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__surprised__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__surprised__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__aa_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__aa_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__aa_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__aa_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__aa_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__aa_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__aa_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__aa_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__aa_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__aa__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__aa__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__aa_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__aa_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__aa_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__aa__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__aa__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__aa__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__aa__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__ih_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__ih_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__ih_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__ih_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ih_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__ih_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__ih_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ih_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__ih_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__ih__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__ih__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__ih_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ih_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__ih_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__ih__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__ih__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__ih__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__ih__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__ou_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__ou_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__ou_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__ou_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ou_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__ou_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__ou_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ou_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__ou_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__ou__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__ou__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__ou_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ou_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__ou_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__ou__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__ou__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__ou__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__ou__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__ee_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__ee_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__ee_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__ee_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ee_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__ee_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__ee_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ee_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__ee_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__ee__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__ee__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__ee_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__ee_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__ee_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__ee__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__ee__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__ee__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__ee__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__oh_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__oh_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__oh_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__oh_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__oh_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__oh_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__oh_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__oh_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__oh_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__oh__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__oh__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__oh_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__oh_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__oh_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__oh__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__oh__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__oh__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__oh__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__blink_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__blink_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__blink_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__blink_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blink_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__blink_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__blink_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blink_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__blink_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__blink__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__blink__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__blink_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blink_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__blink_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__blink__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__blink__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__blink__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__blink__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__blinkLeft_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__blinkLeft_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__blinkLeft_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__blinkLeft_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blinkLeft_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__blinkLeft_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__blinkLeft_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blinkLeft_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__blinkLeft_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__blinkLeft__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__blinkLeft__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__blinkLeft_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blinkLeft_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__blinkLeft_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__blinkLeft__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__blinkLeft__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__blinkLeft__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__blinkLeft__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__blinkRight_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__blinkRight_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__blinkRight_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__blinkRight_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blinkRight_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__blinkRight_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__blinkRight_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blinkRight_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__blinkRight_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__blinkRight__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__blinkRight__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__blinkRight_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__blinkRight_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__blinkRight_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__blinkRight__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__blinkRight__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__blinkRight__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__blinkRight__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Expression __expressions__preset_Deserialize_LookUp(JsonNode parsed)
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__lookUp_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__lookUp_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__lookUp_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__lookUp_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookUp_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__lookUp_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__lookUp_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookUp_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__lookUp_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__lookUp__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookUp__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__lookUp_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookUp_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__lookUp_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__lookUp__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__lookUp__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookUp__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__lookUp__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Expression __expressions__preset_Deserialize_LookDown(JsonNode parsed)
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__lookDown_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__lookDown_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__lookDown_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__lookDown_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookDown_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__lookDown_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__lookDown_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookDown_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__lookDown_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__lookDown__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookDown__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__lookDown_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookDown_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__lookDown_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__lookDown__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__lookDown__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookDown__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__lookDown__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Expression __expressions__preset_Deserialize_LookLeft(JsonNode parsed)
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__lookLeft_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__lookLeft_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__lookLeft_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__lookLeft_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookLeft_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__lookLeft_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__lookLeft_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookLeft_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__lookLeft_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__lookLeft__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookLeft__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__lookLeft_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookLeft_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__lookLeft_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__lookLeft__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__lookLeft__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookLeft__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__lookLeft__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Expression __expressions__preset_Deserialize_LookRight(JsonNode parsed)
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__lookRight_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__lookRight_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__lookRight_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__lookRight_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookRight_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__lookRight_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__lookRight_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookRight_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__lookRight_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__lookRight__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookRight__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__lookRight_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__lookRight_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__lookRight_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__lookRight__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__lookRight__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__lookRight__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__lookRight__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__preset__neutral_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__preset__neutral_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__preset__neutral_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__preset__neutral_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__neutral_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__preset__neutral_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__preset__neutral_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__neutral_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__preset__neutral_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__preset__neutral__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__neutral__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__preset__neutral_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__preset__neutral_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__preset__neutral_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__preset__neutral__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__preset__neutral__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__preset__neutral__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__preset__neutral__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
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

        if(key=="morphTargetBinds"){
            value.MorphTargetBinds = __expressions__custom_PROP_Deserialize_MorphTargetBinds(kv.Value);
            continue;
        }

        if(key=="materialColorBinds"){
            value.MaterialColorBinds = __expressions__custom_PROP_Deserialize_MaterialColorBinds(kv.Value);
            continue;
        }

        if(key=="textureTransformBinds"){
            value.TextureTransformBinds = __expressions__custom_PROP_Deserialize_TextureTransformBinds(kv.Value);
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

public static List<MorphTargetBind> __expressions__custom_PROP_Deserialize_MorphTargetBinds(JsonNode parsed)
{
    var value = new List<MorphTargetBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__custom_PROP_Deserialize_MorphTargetBinds_ITEM(x));
    }
	return value;
} 

public static MorphTargetBind __expressions__custom_PROP_Deserialize_MorphTargetBinds_ITEM(JsonNode parsed)
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

public static List<MaterialColorBind> __expressions__custom_PROP_Deserialize_MaterialColorBinds(JsonNode parsed)
{
    var value = new List<MaterialColorBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__custom_PROP_Deserialize_MaterialColorBinds_ITEM(x));
    }
	return value;
} 

public static MaterialColorBind __expressions__custom_PROP_Deserialize_MaterialColorBinds_ITEM(JsonNode parsed)
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
            value.TargetValue = __expressions__custom_PROP__materialColorBinds_ITEM_Deserialize_TargetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__custom_PROP__materialColorBinds_ITEM_Deserialize_TargetValue(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<TextureTransformBind> __expressions__custom_PROP_Deserialize_TextureTransformBinds(JsonNode parsed)
{
    var value = new List<TextureTransformBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(__expressions__custom_PROP_Deserialize_TextureTransformBinds_ITEM(x));
    }
	return value;
} 

public static TextureTransformBind __expressions__custom_PROP_Deserialize_TextureTransformBinds_ITEM(JsonNode parsed)
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

        if(key=="scale"){
            value.Scale = __expressions__custom_PROP__textureTransformBinds_ITEM_Deserialize_Scale(kv.Value);
            continue;
        }

        if(key=="offset"){
            value.Offset = __expressions__custom_PROP__textureTransformBinds_ITEM_Deserialize_Offset(kv.Value);
            continue;
        }

    }
    return value;
}

public static float[] __expressions__custom_PROP__textureTransformBinds_ITEM_Deserialize_Scale(JsonNode parsed)
{
    var value = new float[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static float[] __expressions__custom_PROP__textureTransformBinds_ITEM_Deserialize_Offset(JsonNode parsed)
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
