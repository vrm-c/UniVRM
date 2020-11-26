
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

    f.Key("exporterVersion");
    f.Value(value.exporterVersion);

    f.Key("specVersion");
    f.Value(value.specVersion);

    f.Key("meta");
    Serialize_vrm_meta(f, value.meta);

    f.Key("humanoid");
    Serialize_vrm_humanoid(f, value.humanoid);

    f.Key("firstPerson");
    Serialize_vrm_firstPerson(f, value.firstPerson);

    f.Key("blendShapeMaster");
    Serialize_vrm_blendShapeMaster(f, value.blendShapeMaster);

    f.Key("secondaryAnimation");
    Serialize_vrm_secondaryAnimation(f, value.secondaryAnimation);

    f.Key("materialProperties");
    Serialize_vrm_materialProperties(f, value.materialProperties);


    f.EndMap();
}

public static void Serialize_vrm_meta(JsonFormatter f, glTF_VRM_Meta value)
{
    f.BeginMap();

    f.Key("title");
    f.Value(value.title);

    f.Key("version");
    f.Value(value.version);

    f.Key("author");
    f.Value(value.author);

    f.Key("contactInformation");
    f.Value(value.contactInformation);

    f.Key("reference");
    f.Value(value.reference);

    f.Key("texture");
    f.Value(value.texture);

    f.Key("allowedUserName");
    f.Value(value.allowedUserName);

    f.Key("violentUssageName");
    f.Value(value.violentUssageName);

    f.Key("sexualUssageName");
    f.Value(value.sexualUssageName);

    f.Key("commercialUssageName");
    f.Value(value.commercialUssageName);

    f.Key("otherPermissionUrl");
    f.Value(value.otherPermissionUrl);

    f.Key("licenseName");
    f.Value(value.licenseName);

    f.Key("otherLicenseUrl");
    f.Value(value.otherLicenseUrl);


    f.EndMap();
}

public static void Serialize_vrm_humanoid(JsonFormatter f, glTF_VRM_Humanoid value)
{
    f.BeginMap();

    f.Key("humanBones");
    Serialize_vrm_humanoid_humanBones(f, value.humanBones);

    f.Key("armStretch");
    f.Value(value.armStretch);

    f.Key("legStretch");
    f.Value(value.legStretch);

    f.Key("upperArmTwist");
    f.Value(value.upperArmTwist);

    f.Key("lowerArmTwist");
    f.Value(value.lowerArmTwist);

    f.Key("upperLegTwist");
    f.Value(value.upperLegTwist);

    f.Key("lowerLegTwist");
    f.Value(value.lowerLegTwist);

    f.Key("feetSpacing");
    f.Value(value.feetSpacing);

    f.Key("hasTranslationDoF");
    f.Value(value.hasTranslationDoF);


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

    f.Key("bone");
    f.Value(value.bone);

    f.Key("node");
    f.Value(value.node);

    f.Key("useDefaultValues");
    f.Value(value.useDefaultValues);

    f.Key("min");
    Serialize_vrm_humanoid_humanBones__min(f, value.min);

    f.Key("max");
    Serialize_vrm_humanoid_humanBones__max(f, value.max);

    f.Key("center");
    Serialize_vrm_humanoid_humanBones__center(f, value.center);

    f.Key("axisLength");
    f.Value(value.axisLength);


    f.EndMap();
}

public static void Serialize_vrm_humanoid_humanBones__min(JsonFormatter f, Vector3 value)
{
    f.BeginMap();

    f.Key("x");
    f.Value(value.x);

    f.Key("y");
    f.Value(value.y);

    f.Key("z");
    f.Value(value.z);


    f.EndMap();
}

public static void Serialize_vrm_humanoid_humanBones__max(JsonFormatter f, Vector3 value)
{
    f.BeginMap();

    f.Key("x");
    f.Value(value.x);

    f.Key("y");
    f.Value(value.y);

    f.Key("z");
    f.Value(value.z);


    f.EndMap();
}

public static void Serialize_vrm_humanoid_humanBones__center(JsonFormatter f, Vector3 value)
{
    f.BeginMap();

    f.Key("x");
    f.Value(value.x);

    f.Key("y");
    f.Value(value.y);

    f.Key("z");
    f.Value(value.z);


    f.EndMap();
}

public static void Serialize_vrm_firstPerson(JsonFormatter f, glTF_VRM_Firstperson value)
{
    f.BeginMap();

    f.Key("firstPersonBone");
    f.Value(value.firstPersonBone);

    f.Key("firstPersonBoneOffset");
    Serialize_vrm_firstPerson_firstPersonBoneOffset(f, value.firstPersonBoneOffset);

    f.Key("meshAnnotations");
    Serialize_vrm_firstPerson_meshAnnotations(f, value.meshAnnotations);

    f.Key("lookAtTypeName");
    f.Value(value.lookAtTypeName);

    f.Key("lookAtHorizontalInner");
    Serialize_vrm_firstPerson_lookAtHorizontalInner(f, value.lookAtHorizontalInner);

    f.Key("lookAtHorizontalOuter");
    Serialize_vrm_firstPerson_lookAtHorizontalOuter(f, value.lookAtHorizontalOuter);

    f.Key("lookAtVerticalDown");
    Serialize_vrm_firstPerson_lookAtVerticalDown(f, value.lookAtVerticalDown);

    f.Key("lookAtVerticalUp");
    Serialize_vrm_firstPerson_lookAtVerticalUp(f, value.lookAtVerticalUp);


    f.EndMap();
}

public static void Serialize_vrm_firstPerson_firstPersonBoneOffset(JsonFormatter f, Vector3 value)
{
    f.BeginMap();

    f.Key("x");
    f.Value(value.x);

    f.Key("y");
    f.Value(value.y);

    f.Key("z");
    f.Value(value.z);


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

    f.Key("mesh");
    f.Value(value.mesh);

    f.Key("firstPersonFlag");
    f.Value(value.firstPersonFlag);


    f.EndMap();
}

public static void Serialize_vrm_firstPerson_lookAtHorizontalInner(JsonFormatter f, glTF_VRM_DegreeMap value)
{
    f.BeginMap();

    f.Key("curve");
    Serialize_vrm_firstPerson_lookAtHorizontalInner_curve(f, value.curve);

    f.Key("xRange");
    f.Value(value.xRange);

    f.Key("yRange");
    f.Value(value.yRange);


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

    f.Key("curve");
    Serialize_vrm_firstPerson_lookAtHorizontalOuter_curve(f, value.curve);

    f.Key("xRange");
    f.Value(value.xRange);

    f.Key("yRange");
    f.Value(value.yRange);


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

    f.Key("curve");
    Serialize_vrm_firstPerson_lookAtVerticalDown_curve(f, value.curve);

    f.Key("xRange");
    f.Value(value.xRange);

    f.Key("yRange");
    f.Value(value.yRange);


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

    f.Key("curve");
    Serialize_vrm_firstPerson_lookAtVerticalUp_curve(f, value.curve);

    f.Key("xRange");
    f.Value(value.xRange);

    f.Key("yRange");
    f.Value(value.yRange);


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

    f.Key("blendShapeGroups");
    Serialize_vrm_blendShapeMaster_blendShapeGroups(f, value.blendShapeGroups);


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

    f.Key("name");
    f.Value(value.name);

    f.Key("presetName");
    f.Value(value.presetName);

    f.Key("binds");
    Serialize_vrm_blendShapeMaster_blendShapeGroups__binds(f, value.binds);

    f.Key("materialValues");
    Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues(f, value.materialValues);

    f.Key("isBinary");
    f.Value(value.isBinary);


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

    f.Key("mesh");
    f.Value(value.mesh);

    f.Key("index");
    f.Value(value.index);

    f.Key("weight");
    f.Value(value.weight);


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

    f.Key("materialName");
    f.Value(value.materialName);

    f.Key("propertyName");
    f.Value(value.propertyName);

    f.Key("targetValue");
    Serialize_vrm_blendShapeMaster_blendShapeGroups__materialValues__targetValue(f, value.targetValue);


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

    f.Key("boneGroups");
    Serialize_vrm_secondaryAnimation_boneGroups(f, value.boneGroups);

    f.Key("colliderGroups");
    Serialize_vrm_secondaryAnimation_colliderGroups(f, value.colliderGroups);


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

    f.Key("comment");
    f.Value(value.comment);

    f.Key("stiffiness");
    f.Value(value.stiffiness);

    f.Key("gravityPower");
    f.Value(value.gravityPower);

    f.Key("gravityDir");
    Serialize_vrm_secondaryAnimation_boneGroups__gravityDir(f, value.gravityDir);

    f.Key("dragForce");
    f.Value(value.dragForce);

    f.Key("center");
    f.Value(value.center);

    f.Key("hitRadius");
    f.Value(value.hitRadius);

    f.Key("bones");
    Serialize_vrm_secondaryAnimation_boneGroups__bones(f, value.bones);

    f.Key("colliderGroups");
    Serialize_vrm_secondaryAnimation_boneGroups__colliderGroups(f, value.colliderGroups);


    f.EndMap();
}

public static void Serialize_vrm_secondaryAnimation_boneGroups__gravityDir(JsonFormatter f, Vector3 value)
{
    f.BeginMap();

    f.Key("x");
    f.Value(value.x);

    f.Key("y");
    f.Value(value.y);

    f.Key("z");
    f.Value(value.z);


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

    f.Key("node");
    f.Value(value.node);

    f.Key("colliders");
    Serialize_vrm_secondaryAnimation_colliderGroups__colliders(f, value.colliders);


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

    f.Key("offset");
    Serialize_vrm_secondaryAnimation_colliderGroups__colliders__offset(f, value.offset);

    f.Key("radius");
    f.Value(value.radius);


    f.EndMap();
}

public static void Serialize_vrm_secondaryAnimation_colliderGroups__colliders__offset(JsonFormatter f, Vector3 value)
{
    f.BeginMap();

    f.Key("x");
    f.Value(value.x);

    f.Key("y");
    f.Value(value.y);

    f.Key("z");
    f.Value(value.z);


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

    f.Key("name");
    f.Value(value.name);

    f.Key("shader");
    f.Value(value.shader);

    f.Key("renderQueue");
    f.Value(value.renderQueue);

    f.Key("floatProperties");
    Serialize_vrm_materialProperties__floatProperties(f, value.floatProperties);

    f.Key("vectorProperties");
    Serialize_vrm_materialProperties__vectorProperties(f, value.vectorProperties);

    f.Key("textureProperties");
    Serialize_vrm_materialProperties__textureProperties(f, value.textureProperties);

    f.Key("keywordMap");
    Serialize_vrm_materialProperties__keywordMap(f, value.keywordMap);

    f.Key("tagMap");
    Serialize_vrm_materialProperties__tagMap(f, value.tagMap);


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
        Serialize_vrm_materialProperties__vectorProperties_DICT(f, kv.Value);
    }
    f.EndMap();
}

public static void Serialize_vrm_materialProperties__vectorProperties_DICT(JsonFormatter f, Single[] value)
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
