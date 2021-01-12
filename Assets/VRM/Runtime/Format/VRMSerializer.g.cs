
using System;
using System.Collections.Generic;
using UniJSON;
using UnityEngine;
using VRM;

namespace VRM {

    static public class VRMSerializer
    {


public static void Serialize(JsonFormatter f, glTF_VRM_extensions value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.exporterVersion)){
        f.Key("exporterVersion");                
        f.Value(value.exporterVersion);
    }

    if(!string.IsNullOrEmpty(value.specVersion)){
        f.Key("specVersion");                
        f.Value(value.specVersion);
    }

    if(value.meta!=null){
        f.Key("meta");                
        Serialize_vrm_meta(f, value.meta);
    }

    if(value.humanoid!=null){
        f.Key("humanoid");                
        Serialize_vrm_humanoid(f, value.humanoid);
    }

    if(value.firstPerson!=null){
        f.Key("firstPerson");                
        Serialize_vrm_firstPerson(f, value.firstPerson);
    }

    if(value.blendShapeMaster!=null){
        f.Key("blendShapeMaster");                
        Serialize_vrm_blendShapeMaster(f, value.blendShapeMaster);
    }

    if(value.secondaryAnimation!=null){
        f.Key("secondaryAnimation");                
        Serialize_vrm_secondaryAnimation(f, value.secondaryAnimation);
    }

    if(value.materialProperties!=null&&value.materialProperties.Count>=0){
        f.Key("materialProperties");                
        Serialize_vrm_materialProperties(f, value.materialProperties);
    }

    f.EndMap();
}

public static void Serialize_vrm_meta(JsonFormatter f, glTF_VRM_Meta value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.title)){
        f.Key("title");                
        f.Value(value.title);
    }

    if(!string.IsNullOrEmpty(value.version)){
        f.Key("version");                
        f.Value(value.version);
    }

    if(!string.IsNullOrEmpty(value.author)){
        f.Key("author");                
        f.Value(value.author);
    }

    if(!string.IsNullOrEmpty(value.contactInformation)){
        f.Key("contactInformation");                
        f.Value(value.contactInformation);
    }

    if(!string.IsNullOrEmpty(value.reference)){
        f.Key("reference");                
        f.Value(value.reference);
    }

    if(value.texture>=0){
        f.Key("texture");                
        f.Value(value.texture);
    }

    if(!string.IsNullOrEmpty(value.allowedUserName)){
        f.Key("allowedUserName");                
        f.Value(value.allowedUserName);
    }

    if(!string.IsNullOrEmpty(value.violentUssageName)){
        f.Key("violentUssageName");                
        f.Value(value.violentUssageName);
    }

    if(!string.IsNullOrEmpty(value.sexualUssageName)){
        f.Key("sexualUssageName");                
        f.Value(value.sexualUssageName);
    }

    if(!string.IsNullOrEmpty(value.commercialUssageName)){
        f.Key("commercialUssageName");                
        f.Value(value.commercialUssageName);
    }

    if(!string.IsNullOrEmpty(value.otherPermissionUrl)){
        f.Key("otherPermissionUrl");                
        f.Value(value.otherPermissionUrl);
    }

    if(!string.IsNullOrEmpty(value.licenseName)){
        f.Key("licenseName");                
        f.Value(value.licenseName);
    }

    if(!string.IsNullOrEmpty(value.otherLicenseUrl)){
        f.Key("otherLicenseUrl");                
        f.Value(value.otherLicenseUrl);
    }

    f.EndMap();
}

public static void Serialize_vrm_humanoid(JsonFormatter f, glTF_VRM_Humanoid value)
{
    f.BeginMap();


    if(value.humanBones!=null&&value.humanBones.Count>=0){
        f.Key("humanBones");                
        Serialize_vrm_humanoid_humanBones(f, value.humanBones);
    }

    if(true){
        f.Key("armStretch");                
        f.Value(value.armStretch);
    }

    if(true){
        f.Key("legStretch");                
        f.Value(value.legStretch);
    }

    if(true){
        f.Key("upperArmTwist");                
        f.Value(value.upperArmTwist);
    }

    if(true){
        f.Key("lowerArmTwist");                
        f.Value(value.lowerArmTwist);
    }

    if(true){
        f.Key("upperLegTwist");                
        f.Value(value.upperLegTwist);
    }

    if(true){
        f.Key("lowerLegTwist");                
        f.Value(value.lowerLegTwist);
    }

    if(true){
        f.Key("feetSpacing");                
        f.Value(value.feetSpacing);
    }

    if(true){
        f.Key("hasTranslationDoF");                
        f.Value(value.hasTranslationDoF);
    }

    f.EndMap();
}

public static void Serialize_vrm_humanoid_humanBones(JsonFormatter f, List<glTF_VRM_HumanoidBone> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_humanoid_humanBones_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_humanoid_humanBones_ITEM(JsonFormatter f, glTF_VRM_HumanoidBone value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.bone)){
        f.Key("bone");                
        f.Value(value.bone);
    }

    if(value.node>=0){
        f.Key("node");                
        f.Value(value.node);
    }

    if(true){
        f.Key("useDefaultValues");                
        f.Value(value.useDefaultValues);
    }

    if(value.min!=null&&value.min!=Vector3.zero){
        f.Key("min");                
        Serialize_vrm_humanoid_humanBones__min(f, value.min);
    }

    if(value.max!=null&&value.min!=Vector3.zero){
        f.Key("max");                
        Serialize_vrm_humanoid_humanBones__max(f, value.max);
    }

    if(value.center!=null&&value.min!=Vector3.zero){
        f.Key("center");                
        Serialize_vrm_humanoid_humanBones__center(f, value.center);
    }

    if(value.axisLength>0){
        f.Key("axisLength");                
        f.Value(value.axisLength);
    }

    f.EndMap();
}

public static void Serialize_vrm_humanoid_humanBones__min(JsonFormatter f, Vector3 value)
{
    f.BeginMap();


    if(true){
        f.Key("x");                
        f.Value(value.x);
    }

    if(true){
        f.Key("y");                
        f.Value(value.y);
    }

    if(true){
        f.Key("z");                
        f.Value(value.z);
    }

    f.EndMap();
}

public static void Serialize_vrm_humanoid_humanBones__max(JsonFormatter f, Vector3 value)
{
    f.BeginMap();


    if(true){
        f.Key("x");                
        f.Value(value.x);
    }

    if(true){
        f.Key("y");                
        f.Value(value.y);
    }

    if(true){
        f.Key("z");                
        f.Value(value.z);
    }

    f.EndMap();
}

public static void Serialize_vrm_humanoid_humanBones__center(JsonFormatter f, Vector3 value)
{
    f.BeginMap();


    if(true){
        f.Key("x");                
        f.Value(value.x);
    }

    if(true){
        f.Key("y");                
        f.Value(value.y);
    }

    if(true){
        f.Key("z");                
        f.Value(value.z);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson(JsonFormatter f, glTF_VRM_Firstperson value)
{
    f.BeginMap();


    if(value.firstPersonBone>=0){
        f.Key("firstPersonBone");                
        f.Value(value.firstPersonBone);
    }

    if(value.firstPersonBoneOffset!=null){
        f.Key("firstPersonBoneOffset");                
        Serialize_vrm_firstPerson_firstPersonBoneOffset(f, value.firstPersonBoneOffset);
    }

    if(value.meshAnnotations!=null&&value.meshAnnotations.Count>=0){
        f.Key("meshAnnotations");                
        Serialize_vrm_firstPerson_meshAnnotations(f, value.meshAnnotations);
    }

    if(!string.IsNullOrEmpty(value.lookAtTypeName)){
        f.Key("lookAtTypeName");                
        f.Value(value.lookAtTypeName);
    }

    if(value.lookAtHorizontalInner!=null){
        f.Key("lookAtHorizontalInner");                
        Serialize_vrm_firstPerson_lookAtHorizontalInner(f, value.lookAtHorizontalInner);
    }

    if(value.lookAtHorizontalOuter!=null){
        f.Key("lookAtHorizontalOuter");                
        Serialize_vrm_firstPerson_lookAtHorizontalOuter(f, value.lookAtHorizontalOuter);
    }

    if(value.lookAtVerticalDown!=null){
        f.Key("lookAtVerticalDown");                
        Serialize_vrm_firstPerson_lookAtVerticalDown(f, value.lookAtVerticalDown);
    }

    if(value.lookAtVerticalUp!=null){
        f.Key("lookAtVerticalUp");                
        Serialize_vrm_firstPerson_lookAtVerticalUp(f, value.lookAtVerticalUp);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson_firstPersonBoneOffset(JsonFormatter f, Vector3 value)
{
    f.BeginMap();


    if(true){
        f.Key("x");                
        f.Value(value.x);
    }

    if(true){
        f.Key("y");                
        f.Value(value.y);
    }

    if(true){
        f.Key("z");                
        f.Value(value.z);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson_meshAnnotations(JsonFormatter f, List<glTF_VRM_MeshAnnotation> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_firstPerson_meshAnnotations_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_firstPerson_meshAnnotations_ITEM(JsonFormatter f, glTF_VRM_MeshAnnotation value)
{
    f.BeginMap();


    if(value.mesh>=0){
        f.Key("mesh");                
        f.Value(value.mesh);
    }

    if(!string.IsNullOrEmpty(value.firstPersonFlag)){
        f.Key("firstPersonFlag");                
        f.Value(value.firstPersonFlag);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson_lookAtHorizontalInner(JsonFormatter f, glTF_VRM_DegreeMap value)
{
    f.BeginMap();


    if(value.curve!=null&&value.curve.Length>=0){
        f.Key("curve");                
        Serialize_vrm_firstPerson_lookAtHorizontalInner_curve(f, value.curve);
    }

    if(true){
        f.Key("xRange");                
        f.Value(value.xRange);
    }

    if(true){
        f.Key("yRange");                
        f.Value(value.yRange);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson_lookAtHorizontalInner_curve(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_firstPerson_lookAtHorizontalOuter(JsonFormatter f, glTF_VRM_DegreeMap value)
{
    f.BeginMap();


    if(value.curve!=null&&value.curve.Length>=0){
        f.Key("curve");                
        Serialize_vrm_firstPerson_lookAtHorizontalOuter_curve(f, value.curve);
    }

    if(true){
        f.Key("xRange");                
        f.Value(value.xRange);
    }

    if(true){
        f.Key("yRange");                
        f.Value(value.yRange);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson_lookAtHorizontalOuter_curve(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_firstPerson_lookAtVerticalDown(JsonFormatter f, glTF_VRM_DegreeMap value)
{
    f.BeginMap();


    if(value.curve!=null&&value.curve.Length>=0){
        f.Key("curve");                
        Serialize_vrm_firstPerson_lookAtVerticalDown_curve(f, value.curve);
    }

    if(true){
        f.Key("xRange");                
        f.Value(value.xRange);
    }

    if(true){
        f.Key("yRange");                
        f.Value(value.yRange);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson_lookAtVerticalDown_curve(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_firstPerson_lookAtVerticalUp(JsonFormatter f, glTF_VRM_DegreeMap value)
{
    f.BeginMap();


    if(value.curve!=null&&value.curve.Length>=0){
        f.Key("curve");                
        Serialize_vrm_firstPerson_lookAtVerticalUp_curve(f, value.curve);
    }

    if(true){
        f.Key("xRange");                
        f.Value(value.xRange);
    }

    if(true){
        f.Key("yRange");                
        f.Value(value.yRange);
    }

    f.EndMap();
}

public static void Serialize_vrm_firstPerson_lookAtVerticalUp_curve(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_blendShapeMaster(JsonFormatter f, glTF_VRM_BlendShapeMaster value)
{
    f.BeginMap();


    if(value.blendShapeGroups!=null&&value.blendShapeGroups.Count>=0){
        f.Key("blendShapeGroups");                
        Serialize_vrm_blendShapeMaster_blendShapeGroups(f, value.blendShapeGroups);
    }

    f.EndMap();
}

public static void Serialize_vrm_blendShapeMaster_blendShapeGroups(JsonFormatter f, List<glTF_VRM_BlendShapeGroup> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_blendShapeMaster_blendShapeGroups_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_blendShapeMaster_blendShapeGroups_ITEM(JsonFormatter f, glTF_VRM_BlendShapeGroup value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");                
        f.Value(value.name);
    }

    if(!string.IsNullOrEmpty(value.presetName)){
        f.Key("presetName");                
        f.Value(value.presetName);
    }

    if(value.binds!=null&&value.binds.Count>=0){
        f.Key("binds");                
        Serialize_vrm_blendShapeMaster_blendShapeGroups__binds(f, value.binds);
    }

    if(value.materialValues!=null&&value.materialValues.Count>=0){
        f.Key("materialValues");                
        Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues(f, value.materialValues);
    }

    if(true){
        f.Key("isBinary");                
        f.Value(value.isBinary);
    }

    f.EndMap();
}

public static void Serialize_vrm_blendShapeMaster_blendShapeGroups__binds(JsonFormatter f, List<glTF_VRM_BlendShapeBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_blendShapeMaster_blendShapeGroups__binds_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_blendShapeMaster_blendShapeGroups__binds_ITEM(JsonFormatter f, glTF_VRM_BlendShapeBind value)
{
    f.BeginMap();


    if(value.mesh>=0){
        f.Key("mesh");                
        f.Value(value.mesh);
    }

    if(value.index>=0){
        f.Key("index");                
        f.Value(value.index);
    }

    if(value.weight>=0){
        f.Key("weight");                
        f.Value(value.weight);
    }

    f.EndMap();
}

public static void Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues(JsonFormatter f, List<glTF_VRM_MaterialValueBind> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues_ITEM(JsonFormatter f, glTF_VRM_MaterialValueBind value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.materialName)){
        f.Key("materialName");                
        f.Value(value.materialName);
    }

    if(!string.IsNullOrEmpty(value.propertyName)){
        f.Key("propertyName");                
        f.Value(value.propertyName);
    }

    if(value.targetValue!=null&&value.targetValue.Length>=0){
        f.Key("targetValue");                
        Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues__targetValue(f, value.targetValue);
    }

    f.EndMap();
}

public static void Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues__targetValue(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_secondaryAnimation(JsonFormatter f, glTF_VRM_SecondaryAnimation value)
{
    f.BeginMap();


    if(value.boneGroups!=null&&value.boneGroups.Count>=0){
        f.Key("boneGroups");                
        Serialize_vrm_secondaryAnimation_boneGroups(f, value.boneGroups);
    }

    if(value.colliderGroups!=null&&value.colliderGroups.Count>=0){
        f.Key("colliderGroups");                
        Serialize_vrm_secondaryAnimation_colliderGroups(f, value.colliderGroups);
    }

    f.EndMap();
}

public static void Serialize_vrm_secondaryAnimation_boneGroups(JsonFormatter f, List<glTF_VRM_SecondaryAnimationGroup> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_secondaryAnimation_boneGroups_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_secondaryAnimation_boneGroups_ITEM(JsonFormatter f, glTF_VRM_SecondaryAnimationGroup value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.comment)){
        f.Key("comment");                
        f.Value(value.comment);
    }

    if(true){
        f.Key("stiffiness");                
        f.Value(value.stiffiness);
    }

    if(true){
        f.Key("gravityPower");                
        f.Value(value.gravityPower);
    }

    if(value.gravityDir!=null){
        f.Key("gravityDir");                
        Serialize_vrm_secondaryAnimation_boneGroups__gravityDir(f, value.gravityDir);
    }

    if(true){
        f.Key("dragForce");                
        f.Value(value.dragForce);
    }

    if(true){
        f.Key("center");                
        f.Value(value.center);
    }

    if(true){
        f.Key("hitRadius");                
        f.Value(value.hitRadius);
    }

    if(value.bones!=null&&value.bones.Length>=0){
        f.Key("bones");                
        Serialize_vrm_secondaryAnimation_boneGroups__bones(f, value.bones);
    }

    if(value.colliderGroups!=null&&value.colliderGroups.Length>=0){
        f.Key("colliderGroups");                
        Serialize_vrm_secondaryAnimation_boneGroups__colliderGroups(f, value.colliderGroups);
    }

    f.EndMap();
}

public static void Serialize_vrm_secondaryAnimation_boneGroups__gravityDir(JsonFormatter f, Vector3 value)
{
    f.BeginMap();


    if(true){
        f.Key("x");                
        f.Value(value.x);
    }

    if(true){
        f.Key("y");                
        f.Value(value.y);
    }

    if(true){
        f.Key("z");                
        f.Value(value.z);
    }

    f.EndMap();
}

public static void Serialize_vrm_secondaryAnimation_boneGroups__bones(JsonFormatter f, Int32[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_secondaryAnimation_boneGroups__colliderGroups(JsonFormatter f, Int32[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_secondaryAnimation_colliderGroups(JsonFormatter f, List<glTF_VRM_SecondaryAnimationColliderGroup> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_secondaryAnimation_colliderGroups_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_secondaryAnimation_colliderGroups_ITEM(JsonFormatter f, glTF_VRM_SecondaryAnimationColliderGroup value)
{
    f.BeginMap();


    if(value.node>=0){
        f.Key("node");                
        f.Value(value.node);
    }

    if(value.colliders!=null&&value.colliders.Count>=0){
        f.Key("colliders");                
        Serialize_vrm_secondaryAnimation_colliderGroups__colliders(f, value.colliders);
    }

    f.EndMap();
}

public static void Serialize_vrm_secondaryAnimation_colliderGroups__colliders(JsonFormatter f, List<glTF_VRM_SecondaryAnimationCollider> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_secondaryAnimation_colliderGroups__colliders_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_secondaryAnimation_colliderGroups__colliders_ITEM(JsonFormatter f, glTF_VRM_SecondaryAnimationCollider value)
{
    f.BeginMap();


    if(value.offset!=null){
        f.Key("offset");                
        Serialize_vrm_secondaryAnimation_colliderGroups__colliders__offset(f, value.offset);
    }

    if(true){
        f.Key("radius");                
        f.Value(value.radius);
    }

    f.EndMap();
}

public static void Serialize_vrm_secondaryAnimation_colliderGroups__colliders__offset(JsonFormatter f, Vector3 value)
{
    f.BeginMap();


    if(true){
        f.Key("x");                
        f.Value(value.x);
    }

    if(true){
        f.Key("y");                
        f.Value(value.y);
    }

    if(true){
        f.Key("z");                
        f.Value(value.z);
    }

    f.EndMap();
}

public static void Serialize_vrm_materialProperties(JsonFormatter f, List<glTF_VRM_Material> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_vrm_materialProperties_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_vrm_materialProperties_ITEM(JsonFormatter f, glTF_VRM_Material value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");                
        f.Value(value.name);
    }

    if(!string.IsNullOrEmpty(value.shader)){
        f.Key("shader");                
        f.Value(value.shader);
    }

    if(true){
        f.Key("renderQueue");                
        f.Value(value.renderQueue);
    }

    if(value.floatProperties!=null){
        f.Key("floatProperties");                
        Serialize_vrm_materialProperties__floatProperties(f, value.floatProperties);
    }

    if(value.vectorProperties!=null){
        f.Key("vectorProperties");                
        Serialize_vrm_materialProperties__vectorProperties(f, value.vectorProperties);
    }

    if(value.textureProperties!=null){
        f.Key("textureProperties");                
        Serialize_vrm_materialProperties__textureProperties(f, value.textureProperties);
    }

    if(value.keywordMap!=null){
        f.Key("keywordMap");                
        Serialize_vrm_materialProperties__keywordMap(f, value.keywordMap);
    }

    if(value.tagMap!=null){
        f.Key("tagMap");                
        Serialize_vrm_materialProperties__tagMap(f, value.tagMap);
    }

    f.EndMap();
}

public static void Serialize_vrm_materialProperties__floatProperties(JsonFormatter f, Dictionary<string, Single> value)
{
    f.BeginMap();
    foreach(var kv in value)
    {
        f.Key(kv.Key);
        f.Value(kv.Value);
    }
    f.EndMap();
}

public static void Serialize_vrm_materialProperties__vectorProperties(JsonFormatter f, Dictionary<string, Single[]> value)
{
    f.BeginMap();
    foreach(var kv in value)
    {
        f.Key(kv.Key);
        Serialize_vrm_materialProperties__vectorProperties_ITEM(f, kv.Value);
    }
    f.EndMap();
}

public static void Serialize_vrm_materialProperties__vectorProperties_ITEM(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_vrm_materialProperties__textureProperties(JsonFormatter f, Dictionary<string, Int32> value)
{
    f.BeginMap();
    foreach(var kv in value)
    {
        f.Key(kv.Key);
        f.Value(kv.Value);
    }
    f.EndMap();
}

public static void Serialize_vrm_materialProperties__keywordMap(JsonFormatter f, Dictionary<string, Boolean> value)
{
    f.BeginMap();
    foreach(var kv in value)
    {
        f.Key(kv.Key);
        f.Value(kv.Value);
    }
    f.EndMap();
}

public static void Serialize_vrm_materialProperties__tagMap(JsonFormatter f, Dictionary<string, String> value)
{
    f.BeginMap();
    foreach(var kv in value)
    {
        f.Key(kv.Key);
        f.Value(kv.Value);
    }
    f.EndMap();
}

    } // class
} // namespace
