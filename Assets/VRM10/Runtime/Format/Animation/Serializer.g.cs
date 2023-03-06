// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_vrm_animation {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_vrm_animation extension)
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
            extensions.Add(VRMC_vrm_animation.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_vrm_animation value)
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

    if(!string.IsNullOrEmpty(value.SpecVersion)){
        f.Key("specVersion");                
        f.Value(value.SpecVersion);
    }

    if(value.Humanoid!=null){
        f.Key("humanoid");                
        Serialize_Humanoid(f, value.Humanoid);
    }

    if(value.Expressions!=null){
        f.Key("expressions");                
        Serialize_Expressions(f, value.Expressions);
    }

    if(value.LookAt!=null){
        f.Key("lookAt");                
        Serialize_LookAt(f, value.LookAt);
    }

    f.EndMap();
}

public static void Serialize_Humanoid(JsonFormatter f, Humanoid value)
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

    if(value.HumanBones!=null){
        f.Key("humanBones");                
        __humanoid_Serialize_HumanBones(f, value.HumanBones);
    }

    f.EndMap();
}

public static void __humanoid_Serialize_HumanBones(JsonFormatter f, HumanBones value)
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

    if(value.Hips!=null){
        f.Key("hips");                
        __humanoid__humanBones_Serialize_Hips(f, value.Hips);
    }

    if(value.Spine!=null){
        f.Key("spine");                
        __humanoid__humanBones_Serialize_Spine(f, value.Spine);
    }

    if(value.Chest!=null){
        f.Key("chest");                
        __humanoid__humanBones_Serialize_Chest(f, value.Chest);
    }

    if(value.UpperChest!=null){
        f.Key("upperChest");                
        __humanoid__humanBones_Serialize_UpperChest(f, value.UpperChest);
    }

    if(value.Neck!=null){
        f.Key("neck");                
        __humanoid__humanBones_Serialize_Neck(f, value.Neck);
    }

    if(value.Head!=null){
        f.Key("head");                
        __humanoid__humanBones_Serialize_Head(f, value.Head);
    }

    if(value.Jaw!=null){
        f.Key("jaw");                
        __humanoid__humanBones_Serialize_Jaw(f, value.Jaw);
    }

    if(value.LeftUpperLeg!=null){
        f.Key("leftUpperLeg");                
        __humanoid__humanBones_Serialize_LeftUpperLeg(f, value.LeftUpperLeg);
    }

    if(value.LeftLowerLeg!=null){
        f.Key("leftLowerLeg");                
        __humanoid__humanBones_Serialize_LeftLowerLeg(f, value.LeftLowerLeg);
    }

    if(value.LeftFoot!=null){
        f.Key("leftFoot");                
        __humanoid__humanBones_Serialize_LeftFoot(f, value.LeftFoot);
    }

    if(value.LeftToes!=null){
        f.Key("leftToes");                
        __humanoid__humanBones_Serialize_LeftToes(f, value.LeftToes);
    }

    if(value.RightUpperLeg!=null){
        f.Key("rightUpperLeg");                
        __humanoid__humanBones_Serialize_RightUpperLeg(f, value.RightUpperLeg);
    }

    if(value.RightLowerLeg!=null){
        f.Key("rightLowerLeg");                
        __humanoid__humanBones_Serialize_RightLowerLeg(f, value.RightLowerLeg);
    }

    if(value.RightFoot!=null){
        f.Key("rightFoot");                
        __humanoid__humanBones_Serialize_RightFoot(f, value.RightFoot);
    }

    if(value.RightToes!=null){
        f.Key("rightToes");                
        __humanoid__humanBones_Serialize_RightToes(f, value.RightToes);
    }

    if(value.LeftShoulder!=null){
        f.Key("leftShoulder");                
        __humanoid__humanBones_Serialize_LeftShoulder(f, value.LeftShoulder);
    }

    if(value.LeftUpperArm!=null){
        f.Key("leftUpperArm");                
        __humanoid__humanBones_Serialize_LeftUpperArm(f, value.LeftUpperArm);
    }

    if(value.LeftLowerArm!=null){
        f.Key("leftLowerArm");                
        __humanoid__humanBones_Serialize_LeftLowerArm(f, value.LeftLowerArm);
    }

    if(value.LeftHand!=null){
        f.Key("leftHand");                
        __humanoid__humanBones_Serialize_LeftHand(f, value.LeftHand);
    }

    if(value.RightShoulder!=null){
        f.Key("rightShoulder");                
        __humanoid__humanBones_Serialize_RightShoulder(f, value.RightShoulder);
    }

    if(value.RightUpperArm!=null){
        f.Key("rightUpperArm");                
        __humanoid__humanBones_Serialize_RightUpperArm(f, value.RightUpperArm);
    }

    if(value.RightLowerArm!=null){
        f.Key("rightLowerArm");                
        __humanoid__humanBones_Serialize_RightLowerArm(f, value.RightLowerArm);
    }

    if(value.RightHand!=null){
        f.Key("rightHand");                
        __humanoid__humanBones_Serialize_RightHand(f, value.RightHand);
    }

    if(value.LeftThumbMetacarpal!=null){
        f.Key("leftThumbMetacarpal");                
        __humanoid__humanBones_Serialize_LeftThumbMetacarpal(f, value.LeftThumbMetacarpal);
    }

    if(value.LeftThumbProximal!=null){
        f.Key("leftThumbProximal");                
        __humanoid__humanBones_Serialize_LeftThumbProximal(f, value.LeftThumbProximal);
    }

    if(value.LeftThumbDistal!=null){
        f.Key("leftThumbDistal");                
        __humanoid__humanBones_Serialize_LeftThumbDistal(f, value.LeftThumbDistal);
    }

    if(value.LeftIndexProximal!=null){
        f.Key("leftIndexProximal");                
        __humanoid__humanBones_Serialize_LeftIndexProximal(f, value.LeftIndexProximal);
    }

    if(value.LeftIndexIntermediate!=null){
        f.Key("leftIndexIntermediate");                
        __humanoid__humanBones_Serialize_LeftIndexIntermediate(f, value.LeftIndexIntermediate);
    }

    if(value.LeftIndexDistal!=null){
        f.Key("leftIndexDistal");                
        __humanoid__humanBones_Serialize_LeftIndexDistal(f, value.LeftIndexDistal);
    }

    if(value.LeftMiddleProximal!=null){
        f.Key("leftMiddleProximal");                
        __humanoid__humanBones_Serialize_LeftMiddleProximal(f, value.LeftMiddleProximal);
    }

    if(value.LeftMiddleIntermediate!=null){
        f.Key("leftMiddleIntermediate");                
        __humanoid__humanBones_Serialize_LeftMiddleIntermediate(f, value.LeftMiddleIntermediate);
    }

    if(value.LeftMiddleDistal!=null){
        f.Key("leftMiddleDistal");                
        __humanoid__humanBones_Serialize_LeftMiddleDistal(f, value.LeftMiddleDistal);
    }

    if(value.LeftRingProximal!=null){
        f.Key("leftRingProximal");                
        __humanoid__humanBones_Serialize_LeftRingProximal(f, value.LeftRingProximal);
    }

    if(value.LeftRingIntermediate!=null){
        f.Key("leftRingIntermediate");                
        __humanoid__humanBones_Serialize_LeftRingIntermediate(f, value.LeftRingIntermediate);
    }

    if(value.LeftRingDistal!=null){
        f.Key("leftRingDistal");                
        __humanoid__humanBones_Serialize_LeftRingDistal(f, value.LeftRingDistal);
    }

    if(value.LeftLittleProximal!=null){
        f.Key("leftLittleProximal");                
        __humanoid__humanBones_Serialize_LeftLittleProximal(f, value.LeftLittleProximal);
    }

    if(value.LeftLittleIntermediate!=null){
        f.Key("leftLittleIntermediate");                
        __humanoid__humanBones_Serialize_LeftLittleIntermediate(f, value.LeftLittleIntermediate);
    }

    if(value.LeftLittleDistal!=null){
        f.Key("leftLittleDistal");                
        __humanoid__humanBones_Serialize_LeftLittleDistal(f, value.LeftLittleDistal);
    }

    if(value.RightThumbMetacarpal!=null){
        f.Key("rightThumbMetacarpal");                
        __humanoid__humanBones_Serialize_RightThumbMetacarpal(f, value.RightThumbMetacarpal);
    }

    if(value.RightThumbProximal!=null){
        f.Key("rightThumbProximal");                
        __humanoid__humanBones_Serialize_RightThumbProximal(f, value.RightThumbProximal);
    }

    if(value.RightThumbDistal!=null){
        f.Key("rightThumbDistal");                
        __humanoid__humanBones_Serialize_RightThumbDistal(f, value.RightThumbDistal);
    }

    if(value.RightIndexProximal!=null){
        f.Key("rightIndexProximal");                
        __humanoid__humanBones_Serialize_RightIndexProximal(f, value.RightIndexProximal);
    }

    if(value.RightIndexIntermediate!=null){
        f.Key("rightIndexIntermediate");                
        __humanoid__humanBones_Serialize_RightIndexIntermediate(f, value.RightIndexIntermediate);
    }

    if(value.RightIndexDistal!=null){
        f.Key("rightIndexDistal");                
        __humanoid__humanBones_Serialize_RightIndexDistal(f, value.RightIndexDistal);
    }

    if(value.RightMiddleProximal!=null){
        f.Key("rightMiddleProximal");                
        __humanoid__humanBones_Serialize_RightMiddleProximal(f, value.RightMiddleProximal);
    }

    if(value.RightMiddleIntermediate!=null){
        f.Key("rightMiddleIntermediate");                
        __humanoid__humanBones_Serialize_RightMiddleIntermediate(f, value.RightMiddleIntermediate);
    }

    if(value.RightMiddleDistal!=null){
        f.Key("rightMiddleDistal");                
        __humanoid__humanBones_Serialize_RightMiddleDistal(f, value.RightMiddleDistal);
    }

    if(value.RightRingProximal!=null){
        f.Key("rightRingProximal");                
        __humanoid__humanBones_Serialize_RightRingProximal(f, value.RightRingProximal);
    }

    if(value.RightRingIntermediate!=null){
        f.Key("rightRingIntermediate");                
        __humanoid__humanBones_Serialize_RightRingIntermediate(f, value.RightRingIntermediate);
    }

    if(value.RightRingDistal!=null){
        f.Key("rightRingDistal");                
        __humanoid__humanBones_Serialize_RightRingDistal(f, value.RightRingDistal);
    }

    if(value.RightLittleProximal!=null){
        f.Key("rightLittleProximal");                
        __humanoid__humanBones_Serialize_RightLittleProximal(f, value.RightLittleProximal);
    }

    if(value.RightLittleIntermediate!=null){
        f.Key("rightLittleIntermediate");                
        __humanoid__humanBones_Serialize_RightLittleIntermediate(f, value.RightLittleIntermediate);
    }

    if(value.RightLittleDistal!=null){
        f.Key("rightLittleDistal");                
        __humanoid__humanBones_Serialize_RightLittleDistal(f, value.RightLittleDistal);
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Hips(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Spine(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Chest(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_UpperChest(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Neck(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Head(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Jaw(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftUpperLeg(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLowerLeg(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftFoot(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftToes(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightUpperLeg(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLowerLeg(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightFoot(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightToes(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftShoulder(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftUpperArm(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLowerArm(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftHand(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightShoulder(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightUpperArm(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLowerArm(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightHand(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftThumbMetacarpal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftThumbProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftThumbDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftIndexProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftIndexIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftIndexDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftMiddleProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftMiddleIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftMiddleDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftRingProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftRingIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftRingDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLittleProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLittleIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLittleDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightThumbMetacarpal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightThumbProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightThumbDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightIndexProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightIndexIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightIndexDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightMiddleProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightMiddleIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightMiddleDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightRingProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightRingIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightRingDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLittleProximal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLittleIntermediate(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLittleDistal(JsonFormatter f, HumanBone value)
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

    f.EndMap();
}

public static void Serialize_Expressions(JsonFormatter f, Expressions value)
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

    if(value.Preset!=null){
        f.Key("preset");                
        __expressions_Serialize_Preset(f, value.Preset);
    }

    if(value.Custom!=null&&value.Custom.Count()>0){
        f.Key("custom");                
        __expressions_Serialize_Custom(f, value.Custom);
    }

    f.EndMap();
}

public static void __expressions_Serialize_Preset(JsonFormatter f, Preset value)
{
    f.BeginMap();


    if(value.Happy!=null){
        f.Key("happy");                
        __expressions__preset_Serialize_Happy(f, value.Happy);
    }

    if(value.Angry!=null){
        f.Key("angry");                
        __expressions__preset_Serialize_Angry(f, value.Angry);
    }

    if(value.Sad!=null){
        f.Key("sad");                
        __expressions__preset_Serialize_Sad(f, value.Sad);
    }

    if(value.Relaxed!=null){
        f.Key("relaxed");                
        __expressions__preset_Serialize_Relaxed(f, value.Relaxed);
    }

    if(value.Surprised!=null){
        f.Key("surprised");                
        __expressions__preset_Serialize_Surprised(f, value.Surprised);
    }

    if(value.Aa!=null){
        f.Key("aa");                
        __expressions__preset_Serialize_Aa(f, value.Aa);
    }

    if(value.Ih!=null){
        f.Key("ih");                
        __expressions__preset_Serialize_Ih(f, value.Ih);
    }

    if(value.Ou!=null){
        f.Key("ou");                
        __expressions__preset_Serialize_Ou(f, value.Ou);
    }

    if(value.Ee!=null){
        f.Key("ee");                
        __expressions__preset_Serialize_Ee(f, value.Ee);
    }

    if(value.Oh!=null){
        f.Key("oh");                
        __expressions__preset_Serialize_Oh(f, value.Oh);
    }

    if(value.Blink!=null){
        f.Key("blink");                
        __expressions__preset_Serialize_Blink(f, value.Blink);
    }

    if(value.BlinkLeft!=null){
        f.Key("blinkLeft");                
        __expressions__preset_Serialize_BlinkLeft(f, value.BlinkLeft);
    }

    if(value.BlinkRight!=null){
        f.Key("blinkRight");                
        __expressions__preset_Serialize_BlinkRight(f, value.BlinkRight);
    }

    if(value.LookUp!=null){
        f.Key("lookUp");                
        __expressions__preset_Serialize_LookUp(f, value.LookUp);
    }

    if(value.LookDown!=null){
        f.Key("lookDown");                
        __expressions__preset_Serialize_LookDown(f, value.LookDown);
    }

    if(value.LookLeft!=null){
        f.Key("lookLeft");                
        __expressions__preset_Serialize_LookLeft(f, value.LookLeft);
    }

    if(value.LookRight!=null){
        f.Key("lookRight");                
        __expressions__preset_Serialize_LookRight(f, value.LookRight);
    }

    if(value.Neutral!=null){
        f.Key("neutral");                
        __expressions__preset_Serialize_Neutral(f, value.Neutral);
    }

    f.EndMap();
}

public static void __expressions__preset_Serialize_Happy(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Angry(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Sad(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Relaxed(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Surprised(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Aa(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Ih(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Ou(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Ee(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Oh(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Blink(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_BlinkLeft(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_BlinkRight(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_LookUp(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_LookDown(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_LookLeft(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_LookRight(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions__preset_Serialize_Neutral(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void __expressions_Serialize_Custom(JsonFormatter f, Dictionary<string, Expression> value)
{
    f.BeginMap();

    foreach(var kv in value)
    {
        f.Key(kv.Key);
    __expressions_Serialize_Custom_ITEM(f, kv.Value);

    }
    f.EndMap();
}

public static void __expressions_Serialize_Custom_ITEM(JsonFormatter f, Expression value)
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

    f.EndMap();
}

public static void Serialize_LookAt(JsonFormatter f, LookAt value)
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

    f.EndMap();
}

    } // class
} // namespace
