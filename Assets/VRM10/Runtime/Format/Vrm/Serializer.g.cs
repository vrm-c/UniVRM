// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_vrm {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, VRMC_vrm extension)
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
            extensions.Add(VRMC_vrm.ExtensionName, f.GetStoreBytes());
        }


public static void Serialize(JsonFormatter f, VRMC_vrm value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.SpecVersion)){
        f.Key("specVersion");                
        f.Value(value.SpecVersion);
    }

    if(value.Meta!=null){
        f.Key("meta");                
        Serialize_Meta(f, value.Meta);
    }

    if(value.Humanoid!=null){
        f.Key("humanoid");                
        Serialize_Humanoid(f, value.Humanoid);
    }

    if(value.FirstPerson!=null){
        f.Key("firstPerson");                
        Serialize_FirstPerson(f, value.FirstPerson);
    }

    if(value.LookAt!=null){
        f.Key("lookAt");                
        Serialize_LookAt(f, value.LookAt);
    }

    if(value.Expressions!=null&&value.Expressions.Count()>=0){
        f.Key("expressions");                
        Serialize_Expressions(f, value.Expressions);
    }

    f.EndMap();
}

public static void Serialize_Meta(JsonFormatter f, Meta value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.Name)){
        f.Key("name");                
        f.Value(value.Name);
    }

    if(!string.IsNullOrEmpty(value.Version)){
        f.Key("version");                
        f.Value(value.Version);
    }

    if(value.Authors!=null&&value.Authors.Count()>=1){
        f.Key("authors");                
        __meta_Serialize_Authors(f, value.Authors);
    }

    if(!string.IsNullOrEmpty(value.CopyrightInformation)){
        f.Key("copyrightInformation");                
        f.Value(value.CopyrightInformation);
    }

    if(!string.IsNullOrEmpty(value.ContactInformation)){
        f.Key("contactInformation");                
        f.Value(value.ContactInformation);
    }

    if(value.References!=null&&value.References.Count()>=0){
        f.Key("references");                
        __meta_Serialize_References(f, value.References);
    }

    if(!string.IsNullOrEmpty(value.ThirdPartyLicenses)){
        f.Key("thirdPartyLicenses");                
        f.Value(value.ThirdPartyLicenses);
    }

    if(value.ThumbnailImage.HasValue){
        f.Key("thumbnailImage");                
        f.Value(value.ThumbnailImage.GetValueOrDefault());
    }

    if(true){
        f.Key("avatarPermission");                
        f.Value(value.AvatarPermission.ToString());
    }

    if(value.AllowExcessivelyViolentUsage.HasValue){
        f.Key("allowExcessivelyViolentUsage");                
        f.Value(value.AllowExcessivelyViolentUsage.GetValueOrDefault());
    }

    if(value.AllowExcessivelySexualUsage.HasValue){
        f.Key("allowExcessivelySexualUsage");                
        f.Value(value.AllowExcessivelySexualUsage.GetValueOrDefault());
    }

    if(true){
        f.Key("commercialUsage");                
        f.Value(value.CommercialUsage.ToString());
    }

    if(value.AllowPoliticalOrReligiousUsage.HasValue){
        f.Key("allowPoliticalOrReligiousUsage");                
        f.Value(value.AllowPoliticalOrReligiousUsage.GetValueOrDefault());
    }

    if(true){
        f.Key("creditNotation");                
        f.Value(value.CreditNotation.ToString());
    }

    if(value.AllowRedistribution.HasValue){
        f.Key("allowRedistribution");                
        f.Value(value.AllowRedistribution.GetValueOrDefault());
    }

    if(true){
        f.Key("modification");                
        f.Value(value.Modification.ToString());
    }

    if(!string.IsNullOrEmpty(value.OtherLicenseUrl)){
        f.Key("otherLicenseUrl");                
        f.Value(value.OtherLicenseUrl);
    }

    f.EndMap();
}

public static void __meta_Serialize_Authors(JsonFormatter f, List<string> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __meta_Serialize_References(JsonFormatter f, List<string> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_Humanoid(JsonFormatter f, Humanoid value)
{
    f.BeginMap();


    if(value.HumanBones!=null){
        f.Key("humanBones");                
        __humanoid_Serialize_HumanBones(f, value.HumanBones);
    }

    f.EndMap();
}

public static void __humanoid_Serialize_HumanBones(JsonFormatter f, HumanBones value)
{
    f.BeginMap();


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

    if(value.LeftEye!=null){
        f.Key("leftEye");                
        __humanoid__humanBones_Serialize_LeftEye(f, value.LeftEye);
    }

    if(value.RightEye!=null){
        f.Key("rightEye");                
        __humanoid__humanBones_Serialize_RightEye(f, value.RightEye);
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

    if(value.LeftThumbProximal!=null){
        f.Key("leftThumbProximal");                
        __humanoid__humanBones_Serialize_LeftThumbProximal(f, value.LeftThumbProximal);
    }

    if(value.LeftThumbIntermediate!=null){
        f.Key("leftThumbIntermediate");                
        __humanoid__humanBones_Serialize_LeftThumbIntermediate(f, value.LeftThumbIntermediate);
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

    if(value.RightThumbProximal!=null){
        f.Key("rightThumbProximal");                
        __humanoid__humanBones_Serialize_RightThumbProximal(f, value.RightThumbProximal);
    }

    if(value.RightThumbIntermediate!=null){
        f.Key("rightThumbIntermediate");                
        __humanoid__humanBones_Serialize_RightThumbIntermediate(f, value.RightThumbIntermediate);
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


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Spine(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Chest(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_UpperChest(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Neck(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Head(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftEye(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightEye(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_Jaw(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftUpperLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLowerLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftFoot(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftToes(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightUpperLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLowerLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightFoot(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightToes(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftShoulder(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftUpperArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLowerArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftHand(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightShoulder(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightUpperArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLowerArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightHand(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftThumbProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftThumbIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftThumbDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftIndexProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftIndexIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftIndexDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftMiddleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftMiddleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftMiddleDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftRingProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftRingIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftRingDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLittleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLittleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_LeftLittleDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightThumbProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightThumbIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightThumbDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightIndexProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightIndexIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightIndexDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightMiddleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightMiddleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightMiddleDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightRingProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightRingIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightRingDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLittleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLittleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __humanoid__humanBones_Serialize_RightLittleDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_FirstPerson(JsonFormatter f, FirstPerson value)
{
    f.BeginMap();


    if(value.MeshAnnotations!=null&&value.MeshAnnotations.Count()>=0){
        f.Key("meshAnnotations");                
        __firstPerson_Serialize_MeshAnnotations(f, value.MeshAnnotations);
    }

    f.EndMap();
}

public static void __firstPerson_Serialize_MeshAnnotations(JsonFormatter f, List<MeshAnnotation> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    __firstPerson_Serialize_MeshAnnotations_ITEM(f, item);

    }
    f.EndList();
}

public static void __firstPerson_Serialize_MeshAnnotations_ITEM(JsonFormatter f, MeshAnnotation value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    if(true){
        f.Key("type");                
        f.Value(value.Type.ToString());
    }

    f.EndMap();
}

public static void Serialize_LookAt(JsonFormatter f, LookAt value)
{
    f.BeginMap();


    if(value.OffsetFromHeadBone!=null&&value.OffsetFromHeadBone.Count()>=0){
        f.Key("offsetFromHeadBone");                
        __lookAt_Serialize_OffsetFromHeadBone(f, value.OffsetFromHeadBone);
    }

    if(true){
        f.Key("type");                
        f.Value(value.Type.ToString());
    }

    if(value.RangeMapHorizontalInner!=null){
        f.Key("rangeMapHorizontalInner");                
        __lookAt_Serialize_RangeMapHorizontalInner(f, value.RangeMapHorizontalInner);
    }

    if(value.RangeMapHorizontalOuter!=null){
        f.Key("rangeMapHorizontalOuter");                
        __lookAt_Serialize_RangeMapHorizontalOuter(f, value.RangeMapHorizontalOuter);
    }

    if(value.RangeMapVerticalDown!=null){
        f.Key("rangeMapVerticalDown");                
        __lookAt_Serialize_RangeMapVerticalDown(f, value.RangeMapVerticalDown);
    }

    if(value.RangeMapVerticalUp!=null){
        f.Key("rangeMapVerticalUp");                
        __lookAt_Serialize_RangeMapVerticalUp(f, value.RangeMapVerticalUp);
    }

    f.EndMap();
}

public static void __lookAt_Serialize_OffsetFromHeadBone(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __lookAt_Serialize_RangeMapHorizontalInner(JsonFormatter f, LookAtRangeMap value)
{
    f.BeginMap();


    if(value.InputMaxValue.HasValue){
        f.Key("inputMaxValue");                
        f.Value(value.InputMaxValue.GetValueOrDefault());
    }

    if(value.OutputScale.HasValue){
        f.Key("outputScale");                
        f.Value(value.OutputScale.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __lookAt_Serialize_RangeMapHorizontalOuter(JsonFormatter f, LookAtRangeMap value)
{
    f.BeginMap();


    if(value.InputMaxValue.HasValue){
        f.Key("inputMaxValue");                
        f.Value(value.InputMaxValue.GetValueOrDefault());
    }

    if(value.OutputScale.HasValue){
        f.Key("outputScale");                
        f.Value(value.OutputScale.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __lookAt_Serialize_RangeMapVerticalDown(JsonFormatter f, LookAtRangeMap value)
{
    f.BeginMap();


    if(value.InputMaxValue.HasValue){
        f.Key("inputMaxValue");                
        f.Value(value.InputMaxValue.GetValueOrDefault());
    }

    if(value.OutputScale.HasValue){
        f.Key("outputScale");                
        f.Value(value.OutputScale.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __lookAt_Serialize_RangeMapVerticalUp(JsonFormatter f, LookAtRangeMap value)
{
    f.BeginMap();


    if(value.InputMaxValue.HasValue){
        f.Key("inputMaxValue");                
        f.Value(value.InputMaxValue.GetValueOrDefault());
    }

    if(value.OutputScale.HasValue){
        f.Key("outputScale");                
        f.Value(value.OutputScale.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_Expressions(JsonFormatter f, List<Expression> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_Expressions_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_Expressions_ITEM(JsonFormatter f, Expression value)
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

    if(!string.IsNullOrEmpty(value.Name)){
        f.Key("name");                
        f.Value(value.Name);
    }

    if(true){
        f.Key("preset");                
        f.Value(value.Preset.ToString());
    }

    if(value.MorphTargetBinds!=null&&value.MorphTargetBinds.Count()>=0){
        f.Key("morphTargetBinds");                
        __expressions_ITEM_Serialize_MorphTargetBinds(f, value.MorphTargetBinds);
    }

    if(value.MaterialColorBinds!=null&&value.MaterialColorBinds.Count()>=0){
        f.Key("materialColorBinds");                
        __expressions_ITEM_Serialize_MaterialColorBinds(f, value.MaterialColorBinds);
    }

    if(value.TextureTransformBinds!=null&&value.TextureTransformBinds.Count()>=0){
        f.Key("textureTransformBinds");                
        __expressions_ITEM_Serialize_TextureTransformBinds(f, value.TextureTransformBinds);
    }

    if(value.IsBinary.HasValue){
        f.Key("isBinary");                
        f.Value(value.IsBinary.GetValueOrDefault());
    }

    if(true){
        f.Key("overrideBlink");                
        f.Value(value.OverrideBlink.ToString());
    }

    if(true){
        f.Key("overrideLookAt");                
        f.Value(value.OverrideLookAt.ToString());
    }

    if(true){
        f.Key("overrideMouth");                
        f.Value(value.OverrideMouth.ToString());
    }

    f.EndMap();
}

public static void __expressions_ITEM_Serialize_MorphTargetBinds(JsonFormatter f, List<MorphTargetBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    __expressions_ITEM_Serialize_MorphTargetBinds_ITEM(f, item);

    }
    f.EndList();
}

public static void __expressions_ITEM_Serialize_MorphTargetBinds_ITEM(JsonFormatter f, MorphTargetBind value)
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

    if(value.Index.HasValue){
        f.Key("index");                
        f.Value(value.Index.GetValueOrDefault());
    }

    if(value.Weight.HasValue){
        f.Key("weight");                
        f.Value(value.Weight.GetValueOrDefault());
    }

    f.EndMap();
}

public static void __expressions_ITEM_Serialize_MaterialColorBinds(JsonFormatter f, List<MaterialColorBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    __expressions_ITEM_Serialize_MaterialColorBinds_ITEM(f, item);

    }
    f.EndList();
}

public static void __expressions_ITEM_Serialize_MaterialColorBinds_ITEM(JsonFormatter f, MaterialColorBind value)
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

    if(value.Material.HasValue){
        f.Key("material");                
        f.Value(value.Material.GetValueOrDefault());
    }

    if(true){
        f.Key("type");                
        f.Value(value.Type.ToString());
    }

    if(value.TargetValue!=null&&value.TargetValue.Count()>=4){
        f.Key("targetValue");                
        __expressions_ITEM__materialColorBinds_ITEM_Serialize_TargetValue(f, value.TargetValue);
    }

    f.EndMap();
}

public static void __expressions_ITEM__materialColorBinds_ITEM_Serialize_TargetValue(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __expressions_ITEM_Serialize_TextureTransformBinds(JsonFormatter f, List<TextureTransformBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    __expressions_ITEM_Serialize_TextureTransformBinds_ITEM(f, item);

    }
    f.EndList();
}

public static void __expressions_ITEM_Serialize_TextureTransformBinds_ITEM(JsonFormatter f, TextureTransformBind value)
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

    if(value.Material.HasValue){
        f.Key("material");                
        f.Value(value.Material.GetValueOrDefault());
    }

    if(value.Scaling!=null&&value.Scaling.Count()>=2){
        f.Key("scaling");                
        __expressions_ITEM__textureTransformBinds_ITEM_Serialize_Scaling(f, value.Scaling);
    }

    if(value.Offset!=null&&value.Offset.Count()>=2){
        f.Key("offset");                
        __expressions_ITEM__textureTransformBinds_ITEM_Serialize_Offset(f, value.Offset);
    }

    f.EndMap();
}

public static void __expressions_ITEM__textureTransformBinds_ITEM_Serialize_Scaling(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void __expressions_ITEM__textureTransformBinds_ITEM_Serialize_Offset(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

    } // class
} // namespace
