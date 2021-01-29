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
        Serialize_Authors(f, value.Authors);
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
        Serialize_References(f, value.References);
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

public static void Serialize_Authors(JsonFormatter f, List<string> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_References(JsonFormatter f, List<string> value)
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
        Serialize_HumanBones(f, value.HumanBones);
    }

    f.EndMap();
}

public static void Serialize_HumanBones(JsonFormatter f, HumanBones value)
{
    f.BeginMap();


    if(value.Hips!=null){
        f.Key("hips");                
        Serialize_Hips(f, value.Hips);
    }

    if(value.Spine!=null){
        f.Key("spine");                
        Serialize_Spine(f, value.Spine);
    }

    if(value.Chest!=null){
        f.Key("chest");                
        Serialize_Chest(f, value.Chest);
    }

    if(value.UpperChest!=null){
        f.Key("upperChest");                
        Serialize_UpperChest(f, value.UpperChest);
    }

    if(value.Neck!=null){
        f.Key("neck");                
        Serialize_Neck(f, value.Neck);
    }

    if(value.Head!=null){
        f.Key("head");                
        Serialize_Head(f, value.Head);
    }

    if(value.LeftEye!=null){
        f.Key("leftEye");                
        Serialize_LeftEye(f, value.LeftEye);
    }

    if(value.RightEye!=null){
        f.Key("rightEye");                
        Serialize_RightEye(f, value.RightEye);
    }

    if(value.Jaw!=null){
        f.Key("jaw");                
        Serialize_Jaw(f, value.Jaw);
    }

    if(value.LeftUpperLeg!=null){
        f.Key("leftUpperLeg");                
        Serialize_LeftUpperLeg(f, value.LeftUpperLeg);
    }

    if(value.LeftLowerLeg!=null){
        f.Key("leftLowerLeg");                
        Serialize_LeftLowerLeg(f, value.LeftLowerLeg);
    }

    if(value.LeftFoot!=null){
        f.Key("leftFoot");                
        Serialize_LeftFoot(f, value.LeftFoot);
    }

    if(value.LeftToes!=null){
        f.Key("leftToes");                
        Serialize_LeftToes(f, value.LeftToes);
    }

    if(value.RightUpperLeg!=null){
        f.Key("rightUpperLeg");                
        Serialize_RightUpperLeg(f, value.RightUpperLeg);
    }

    if(value.RightLowerLeg!=null){
        f.Key("rightLowerLeg");                
        Serialize_RightLowerLeg(f, value.RightLowerLeg);
    }

    if(value.RightFoot!=null){
        f.Key("rightFoot");                
        Serialize_RightFoot(f, value.RightFoot);
    }

    if(value.RightToes!=null){
        f.Key("rightToes");                
        Serialize_RightToes(f, value.RightToes);
    }

    if(value.LeftShoulder!=null){
        f.Key("leftShoulder");                
        Serialize_LeftShoulder(f, value.LeftShoulder);
    }

    if(value.LeftUpperArm!=null){
        f.Key("leftUpperArm");                
        Serialize_LeftUpperArm(f, value.LeftUpperArm);
    }

    if(value.LeftLowerArm!=null){
        f.Key("leftLowerArm");                
        Serialize_LeftLowerArm(f, value.LeftLowerArm);
    }

    if(value.LeftHand!=null){
        f.Key("leftHand");                
        Serialize_LeftHand(f, value.LeftHand);
    }

    if(value.RightShoulder!=null){
        f.Key("rightShoulder");                
        Serialize_RightShoulder(f, value.RightShoulder);
    }

    if(value.RightUpperArm!=null){
        f.Key("rightUpperArm");                
        Serialize_RightUpperArm(f, value.RightUpperArm);
    }

    if(value.RightLowerArm!=null){
        f.Key("rightLowerArm");                
        Serialize_RightLowerArm(f, value.RightLowerArm);
    }

    if(value.RightHand!=null){
        f.Key("rightHand");                
        Serialize_RightHand(f, value.RightHand);
    }

    if(value.LeftThumbProximal!=null){
        f.Key("leftThumbProximal");                
        Serialize_LeftThumbProximal(f, value.LeftThumbProximal);
    }

    if(value.LeftThumbIntermediate!=null){
        f.Key("leftThumbIntermediate");                
        Serialize_LeftThumbIntermediate(f, value.LeftThumbIntermediate);
    }

    if(value.LeftThumbDistal!=null){
        f.Key("leftThumbDistal");                
        Serialize_LeftThumbDistal(f, value.LeftThumbDistal);
    }

    if(value.LeftIndexProximal!=null){
        f.Key("leftIndexProximal");                
        Serialize_LeftIndexProximal(f, value.LeftIndexProximal);
    }

    if(value.LeftIndexIntermediate!=null){
        f.Key("leftIndexIntermediate");                
        Serialize_LeftIndexIntermediate(f, value.LeftIndexIntermediate);
    }

    if(value.LeftIndexDistal!=null){
        f.Key("leftIndexDistal");                
        Serialize_LeftIndexDistal(f, value.LeftIndexDistal);
    }

    if(value.LeftMiddleProximal!=null){
        f.Key("leftMiddleProximal");                
        Serialize_LeftMiddleProximal(f, value.LeftMiddleProximal);
    }

    if(value.LeftMiddleIntermediate!=null){
        f.Key("leftMiddleIntermediate");                
        Serialize_LeftMiddleIntermediate(f, value.LeftMiddleIntermediate);
    }

    if(value.LeftMiddleDistal!=null){
        f.Key("leftMiddleDistal");                
        Serialize_LeftMiddleDistal(f, value.LeftMiddleDistal);
    }

    if(value.LeftRingProximal!=null){
        f.Key("leftRingProximal");                
        Serialize_LeftRingProximal(f, value.LeftRingProximal);
    }

    if(value.LeftRingIntermediate!=null){
        f.Key("leftRingIntermediate");                
        Serialize_LeftRingIntermediate(f, value.LeftRingIntermediate);
    }

    if(value.LeftRingDistal!=null){
        f.Key("leftRingDistal");                
        Serialize_LeftRingDistal(f, value.LeftRingDistal);
    }

    if(value.LeftLittleProximal!=null){
        f.Key("leftLittleProximal");                
        Serialize_LeftLittleProximal(f, value.LeftLittleProximal);
    }

    if(value.LeftLittleIntermediate!=null){
        f.Key("leftLittleIntermediate");                
        Serialize_LeftLittleIntermediate(f, value.LeftLittleIntermediate);
    }

    if(value.LeftLittleDistal!=null){
        f.Key("leftLittleDistal");                
        Serialize_LeftLittleDistal(f, value.LeftLittleDistal);
    }

    if(value.RightThumbProximal!=null){
        f.Key("rightThumbProximal");                
        Serialize_RightThumbProximal(f, value.RightThumbProximal);
    }

    if(value.RightThumbIntermediate!=null){
        f.Key("rightThumbIntermediate");                
        Serialize_RightThumbIntermediate(f, value.RightThumbIntermediate);
    }

    if(value.RightThumbDistal!=null){
        f.Key("rightThumbDistal");                
        Serialize_RightThumbDistal(f, value.RightThumbDistal);
    }

    if(value.RightIndexProximal!=null){
        f.Key("rightIndexProximal");                
        Serialize_RightIndexProximal(f, value.RightIndexProximal);
    }

    if(value.RightIndexIntermediate!=null){
        f.Key("rightIndexIntermediate");                
        Serialize_RightIndexIntermediate(f, value.RightIndexIntermediate);
    }

    if(value.RightIndexDistal!=null){
        f.Key("rightIndexDistal");                
        Serialize_RightIndexDistal(f, value.RightIndexDistal);
    }

    if(value.RightMiddleProximal!=null){
        f.Key("rightMiddleProximal");                
        Serialize_RightMiddleProximal(f, value.RightMiddleProximal);
    }

    if(value.RightMiddleIntermediate!=null){
        f.Key("rightMiddleIntermediate");                
        Serialize_RightMiddleIntermediate(f, value.RightMiddleIntermediate);
    }

    if(value.RightMiddleDistal!=null){
        f.Key("rightMiddleDistal");                
        Serialize_RightMiddleDistal(f, value.RightMiddleDistal);
    }

    if(value.RightRingProximal!=null){
        f.Key("rightRingProximal");                
        Serialize_RightRingProximal(f, value.RightRingProximal);
    }

    if(value.RightRingIntermediate!=null){
        f.Key("rightRingIntermediate");                
        Serialize_RightRingIntermediate(f, value.RightRingIntermediate);
    }

    if(value.RightRingDistal!=null){
        f.Key("rightRingDistal");                
        Serialize_RightRingDistal(f, value.RightRingDistal);
    }

    if(value.RightLittleProximal!=null){
        f.Key("rightLittleProximal");                
        Serialize_RightLittleProximal(f, value.RightLittleProximal);
    }

    if(value.RightLittleIntermediate!=null){
        f.Key("rightLittleIntermediate");                
        Serialize_RightLittleIntermediate(f, value.RightLittleIntermediate);
    }

    if(value.RightLittleDistal!=null){
        f.Key("rightLittleDistal");                
        Serialize_RightLittleDistal(f, value.RightLittleDistal);
    }

    f.EndMap();
}

public static void Serialize_Hips(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_Spine(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_Chest(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_UpperChest(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_Neck(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_Head(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftEye(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightEye(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_Jaw(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftUpperLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftLowerLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftFoot(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftToes(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightUpperLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightLowerLeg(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightFoot(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightToes(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftShoulder(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftUpperArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftLowerArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftHand(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightShoulder(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightUpperArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightLowerArm(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightHand(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftThumbProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftThumbIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftThumbDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftIndexProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftIndexIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftIndexDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftMiddleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftMiddleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftMiddleDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftRingProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftRingIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftRingDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftLittleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftLittleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_LeftLittleDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightThumbProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightThumbIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightThumbDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightIndexProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightIndexIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightIndexDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightMiddleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightMiddleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightMiddleDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightRingProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightRingIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightRingDistal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightLittleProximal(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightLittleIntermediate(JsonFormatter f, HumanBone value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    f.EndMap();
}

public static void Serialize_RightLittleDistal(JsonFormatter f, HumanBone value)
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
        Serialize_MeshAnnotations(f, value.MeshAnnotations);
    }

    f.EndMap();
}

public static void Serialize_MeshAnnotations(JsonFormatter f, List<MeshAnnotation> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_MeshAnnotations_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_MeshAnnotations_ITEM(JsonFormatter f, MeshAnnotation value)
{
    f.BeginMap();


    if(value.Node.HasValue){
        f.Key("node");                
        f.Value(value.Node.GetValueOrDefault());
    }

    if(true){
        f.Key("firstPersonType");                
        f.Value(value.FirstPersonType.ToString());
    }

    f.EndMap();
}

public static void Serialize_LookAt(JsonFormatter f, LookAt value)
{
    f.BeginMap();


    if(value.OffsetFromHeadBone!=null&&value.OffsetFromHeadBone.Count()>=0){
        f.Key("offsetFromHeadBone");                
        Serialize_OffsetFromHeadBone(f, value.OffsetFromHeadBone);
    }

    if(true){
        f.Key("lookAtType");                
        f.Value(value.LookAtType.ToString());
    }

    if(value.LookAtHorizontalInner!=null){
        f.Key("lookAtHorizontalInner");                
        Serialize_LookAtHorizontalInner(f, value.LookAtHorizontalInner);
    }

    if(value.LookAtHorizontalOuter!=null){
        f.Key("lookAtHorizontalOuter");                
        Serialize_LookAtHorizontalOuter(f, value.LookAtHorizontalOuter);
    }

    if(value.LookAtVerticalDown!=null){
        f.Key("lookAtVerticalDown");                
        Serialize_LookAtVerticalDown(f, value.LookAtVerticalDown);
    }

    if(value.LookAtVerticalUp!=null){
        f.Key("lookAtVerticalUp");                
        Serialize_LookAtVerticalUp(f, value.LookAtVerticalUp);
    }

    f.EndMap();
}

public static void Serialize_OffsetFromHeadBone(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_LookAtHorizontalInner(JsonFormatter f, LookAtRangeMap value)
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

public static void Serialize_LookAtHorizontalOuter(JsonFormatter f, LookAtRangeMap value)
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

public static void Serialize_LookAtVerticalDown(JsonFormatter f, LookAtRangeMap value)
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

public static void Serialize_LookAtVerticalUp(JsonFormatter f, LookAtRangeMap value)
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
        value.Extensions.Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        value.Extras.Serialize(f);
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
        Serialize_MorphTargetBinds(f, value.MorphTargetBinds);
    }

    if(value.MaterialColorBinds!=null&&value.MaterialColorBinds.Count()>=0){
        f.Key("materialColorBinds");                
        Serialize_MaterialColorBinds(f, value.MaterialColorBinds);
    }

    if(value.TextureTransformBinds!=null&&value.TextureTransformBinds.Count()>=0){
        f.Key("textureTransformBinds");                
        Serialize_TextureTransformBinds(f, value.TextureTransformBinds);
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

public static void Serialize_MorphTargetBinds(JsonFormatter f, List<MorphTargetBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_MorphTargetBinds_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_MorphTargetBinds_ITEM(JsonFormatter f, MorphTargetBind value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        value.Extensions.Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        value.Extras.Serialize(f);
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

public static void Serialize_MaterialColorBinds(JsonFormatter f, List<MaterialColorBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_MaterialColorBinds_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_MaterialColorBinds_ITEM(JsonFormatter f, MaterialColorBind value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        value.Extensions.Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        value.Extras.Serialize(f);
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
        Serialize_TargetValue(f, value.TargetValue);
    }

    f.EndMap();
}

public static void Serialize_TargetValue(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_TextureTransformBinds(JsonFormatter f, List<TextureTransformBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_TextureTransformBinds_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_TextureTransformBinds_ITEM(JsonFormatter f, TextureTransformBind value)
{
    f.BeginMap();


    if(value.Extensions!=null){
        f.Key("extensions");                
        value.Extensions.Serialize(f);
    }

    if(value.Extras!=null){
        f.Key("extras");                
        value.Extras.Serialize(f);
    }

    if(value.Material.HasValue){
        f.Key("material");                
        f.Value(value.Material.GetValueOrDefault());
    }

    if(value.Scaling!=null&&value.Scaling.Count()>=2){
        f.Key("scaling");                
        Serialize_Scaling(f, value.Scaling);
    }

    if(value.Offset!=null&&value.Offset.Count()>=2){
        f.Key("offset");                
        Serialize_Offset(f, value.Offset);
    }

    f.EndMap();
}

public static void Serialize_Scaling(JsonFormatter f, float[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_Offset(JsonFormatter f, float[] value)
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
