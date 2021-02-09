
using UniJSON;
using System;
using System.Collections.Generic;
using VRM;
using UnityEngine;

namespace VRM {

public static class VrmDeserializer
{


public static glTF_VRM_extensions Deserialize(JsonNode parsed)
{
    var value = new glTF_VRM_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="exporterVersion"){
            value.exporterVersion = kv.Value.GetString();
            continue;
        }

        if(key=="specVersion"){
            value.specVersion = kv.Value.GetString();
            continue;
        }

        if(key=="meta"){
            value.meta = Deserialize_vrm_meta(kv.Value);
            continue;
        }

        if(key=="humanoid"){
            value.humanoid = Deserialize_vrm_humanoid(kv.Value);
            continue;
        }

        if(key=="firstPerson"){
            value.firstPerson = Deserialize_vrm_firstPerson(kv.Value);
            continue;
        }

        if(key=="blendShapeMaster"){
            value.blendShapeMaster = Deserialize_vrm_blendShapeMaster(kv.Value);
            continue;
        }

        if(key=="secondaryAnimation"){
            value.secondaryAnimation = Deserialize_vrm_secondaryAnimation(kv.Value);
            continue;
        }

        if(key=="materialProperties"){
            value.materialProperties = Deserialize_vrm_materialProperties(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTF_VRM_Meta Deserialize_vrm_meta(JsonNode parsed)
{
    var value = new glTF_VRM_Meta();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="title"){
            value.title = kv.Value.GetString();
            continue;
        }

        if(key=="version"){
            value.version = kv.Value.GetString();
            continue;
        }

        if(key=="author"){
            value.author = kv.Value.GetString();
            continue;
        }

        if(key=="contactInformation"){
            value.contactInformation = kv.Value.GetString();
            continue;
        }

        if(key=="reference"){
            value.reference = kv.Value.GetString();
            continue;
        }

        if(key=="texture"){
            value.texture = kv.Value.GetInt32();
            continue;
        }

        if(key=="allowedUserName"){
            value.allowedUserName = kv.Value.GetString();
            continue;
        }

        if(key=="violentUssageName"){
            value.violentUssageName = kv.Value.GetString();
            continue;
        }

        if(key=="sexualUssageName"){
            value.sexualUssageName = kv.Value.GetString();
            continue;
        }

        if(key=="commercialUssageName"){
            value.commercialUssageName = kv.Value.GetString();
            continue;
        }

        if(key=="otherPermissionUrl"){
            value.otherPermissionUrl = kv.Value.GetString();
            continue;
        }

        if(key=="licenseName"){
            value.licenseName = kv.Value.GetString();
            continue;
        }

        if(key=="otherLicenseUrl"){
            value.otherLicenseUrl = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static glTF_VRM_Humanoid Deserialize_vrm_humanoid(JsonNode parsed)
{
    var value = new glTF_VRM_Humanoid();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="humanBones"){
            value.humanBones = Deserialize_vrm_humanoid_humanBones(kv.Value);
            continue;
        }

        if(key=="armStretch"){
            value.armStretch = kv.Value.GetSingle();
            continue;
        }

        if(key=="legStretch"){
            value.legStretch = kv.Value.GetSingle();
            continue;
        }

        if(key=="upperArmTwist"){
            value.upperArmTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="lowerArmTwist"){
            value.lowerArmTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="upperLegTwist"){
            value.upperLegTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="lowerLegTwist"){
            value.lowerLegTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="feetSpacing"){
            value.feetSpacing = kv.Value.GetSingle();
            continue;
        }

        if(key=="hasTranslationDoF"){
            value.hasTranslationDoF = kv.Value.GetBoolean();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_HumanoidBone> Deserialize_vrm_humanoid_humanBones(JsonNode parsed)
{
    var value = new List<glTF_VRM_HumanoidBone>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_humanoid_humanBones_LIST(x));
    }
	return value;
}
public static glTF_VRM_HumanoidBone Deserialize_vrm_humanoid_humanBones_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_HumanoidBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="bone"){
            value.bone = kv.Value.GetString();
            continue;
        }

        if(key=="node"){
            value.node = kv.Value.GetInt32();
            continue;
        }

        if(key=="useDefaultValues"){
            value.useDefaultValues = kv.Value.GetBoolean();
            continue;
        }

        if(key=="min"){
            value.min = Deserialize_vrm_humanoid_humanBones__min(kv.Value);
            continue;
        }

        if(key=="max"){
            value.max = Deserialize_vrm_humanoid_humanBones__max(kv.Value);
            continue;
        }

        if(key=="center"){
            value.center = Deserialize_vrm_humanoid_humanBones__center(kv.Value);
            continue;
        }

        if(key=="axisLength"){
            value.axisLength = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_vrm_humanoid_humanBones__min(JsonNode parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_vrm_humanoid_humanBones__max(JsonNode parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_vrm_humanoid_humanBones__center(JsonNode parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static glTF_VRM_Firstperson Deserialize_vrm_firstPerson(JsonNode parsed)
{
    var value = new glTF_VRM_Firstperson();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="firstPersonBone"){
            value.firstPersonBone = kv.Value.GetInt32();
            continue;
        }

        if(key=="firstPersonBoneOffset"){
            value.firstPersonBoneOffset = Deserialize_vrm_firstPerson_firstPersonBoneOffset(kv.Value);
            continue;
        }

        if(key=="meshAnnotations"){
            value.meshAnnotations = Deserialize_vrm_firstPerson_meshAnnotations(kv.Value);
            continue;
        }

        if(key=="lookAtTypeName"){
            value.lookAtTypeName = kv.Value.GetString();
            continue;
        }

        if(key=="lookAtHorizontalInner"){
            value.lookAtHorizontalInner = Deserialize_vrm_firstPerson_lookAtHorizontalInner(kv.Value);
            continue;
        }

        if(key=="lookAtHorizontalOuter"){
            value.lookAtHorizontalOuter = Deserialize_vrm_firstPerson_lookAtHorizontalOuter(kv.Value);
            continue;
        }

        if(key=="lookAtVerticalDown"){
            value.lookAtVerticalDown = Deserialize_vrm_firstPerson_lookAtVerticalDown(kv.Value);
            continue;
        }

        if(key=="lookAtVerticalUp"){
            value.lookAtVerticalUp = Deserialize_vrm_firstPerson_lookAtVerticalUp(kv.Value);
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_vrm_firstPerson_firstPersonBoneOffset(JsonNode parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_MeshAnnotation> Deserialize_vrm_firstPerson_meshAnnotations(JsonNode parsed)
{
    var value = new List<glTF_VRM_MeshAnnotation>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_firstPerson_meshAnnotations_LIST(x));
    }
	return value;
}
public static glTF_VRM_MeshAnnotation Deserialize_vrm_firstPerson_meshAnnotations_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_MeshAnnotation();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="mesh"){
            value.mesh = kv.Value.GetInt32();
            continue;
        }

        if(key=="firstPersonFlag"){
            value.firstPersonFlag = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static glTF_VRM_DegreeMap Deserialize_vrm_firstPerson_lookAtHorizontalInner(JsonNode parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_vrm_firstPerson_lookAtHorizontalInner_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_vrm_firstPerson_lookAtHorizontalInner_curve(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_DegreeMap Deserialize_vrm_firstPerson_lookAtHorizontalOuter(JsonNode parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_vrm_firstPerson_lookAtHorizontalOuter_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_vrm_firstPerson_lookAtHorizontalOuter_curve(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_DegreeMap Deserialize_vrm_firstPerson_lookAtVerticalDown(JsonNode parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_vrm_firstPerson_lookAtVerticalDown_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_vrm_firstPerson_lookAtVerticalDown_curve(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_DegreeMap Deserialize_vrm_firstPerson_lookAtVerticalUp(JsonNode parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_vrm_firstPerson_lookAtVerticalUp_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_vrm_firstPerson_lookAtVerticalUp_curve(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_BlendShapeMaster Deserialize_vrm_blendShapeMaster(JsonNode parsed)
{
    var value = new glTF_VRM_BlendShapeMaster();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="blendShapeGroups"){
            value.blendShapeGroups = Deserialize_vrm_blendShapeMaster_blendShapeGroups(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_BlendShapeGroup> Deserialize_vrm_blendShapeMaster_blendShapeGroups(JsonNode parsed)
{
    var value = new List<glTF_VRM_BlendShapeGroup>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_blendShapeMaster_blendShapeGroups_LIST(x));
    }
	return value;
}
public static glTF_VRM_BlendShapeGroup Deserialize_vrm_blendShapeMaster_blendShapeGroups_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_BlendShapeGroup();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="presetName"){
            value.presetName = kv.Value.GetString();
            continue;
        }

        if(key=="binds"){
            value.binds = Deserialize_vrm_blendShapeMaster_blendShapeGroups__binds(kv.Value);
            continue;
        }

        if(key=="materialValues"){
            value.materialValues = Deserialize_vrm_blendShapeMaster_blendShapeGroups__materialValues(kv.Value);
            continue;
        }

        if(key=="isBinary"){
            value.isBinary = kv.Value.GetBoolean();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_BlendShapeBind> Deserialize_vrm_blendShapeMaster_blendShapeGroups__binds(JsonNode parsed)
{
    var value = new List<glTF_VRM_BlendShapeBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_blendShapeMaster_blendShapeGroups__binds_LIST(x));
    }
	return value;
}
public static glTF_VRM_BlendShapeBind Deserialize_vrm_blendShapeMaster_blendShapeGroups__binds_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_BlendShapeBind();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="mesh"){
            value.mesh = kv.Value.GetInt32();
            continue;
        }

        if(key=="index"){
            value.index = kv.Value.GetInt32();
            continue;
        }

        if(key=="weight"){
            value.weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_MaterialValueBind> Deserialize_vrm_blendShapeMaster_blendShapeGroups__materialValues(JsonNode parsed)
{
    var value = new List<glTF_VRM_MaterialValueBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_blendShapeMaster_blendShapeGroups__materialValues_LIST(x));
    }
	return value;
}
public static glTF_VRM_MaterialValueBind Deserialize_vrm_blendShapeMaster_blendShapeGroups__materialValues_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_MaterialValueBind();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="materialName"){
            value.materialName = kv.Value.GetString();
            continue;
        }

        if(key=="propertyName"){
            value.propertyName = kv.Value.GetString();
            continue;
        }

        if(key=="targetValue"){
            value.targetValue = Deserialize_vrm_blendShapeMaster_blendShapeGroups__materialValues__targetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_vrm_blendShapeMaster_blendShapeGroups__materialValues__targetValue(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_SecondaryAnimation Deserialize_vrm_secondaryAnimation(JsonNode parsed)
{
    var value = new glTF_VRM_SecondaryAnimation();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="boneGroups"){
            value.boneGroups = Deserialize_vrm_secondaryAnimation_boneGroups(kv.Value);
            continue;
        }

        if(key=="colliderGroups"){
            value.colliderGroups = Deserialize_vrm_secondaryAnimation_colliderGroups(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_SecondaryAnimationGroup> Deserialize_vrm_secondaryAnimation_boneGroups(JsonNode parsed)
{
    var value = new List<glTF_VRM_SecondaryAnimationGroup>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_secondaryAnimation_boneGroups_LIST(x));
    }
	return value;
}
public static glTF_VRM_SecondaryAnimationGroup Deserialize_vrm_secondaryAnimation_boneGroups_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_SecondaryAnimationGroup();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="comment"){
            value.comment = kv.Value.GetString();
            continue;
        }

        if(key=="stiffiness"){
            value.stiffiness = kv.Value.GetSingle();
            continue;
        }

        if(key=="gravityPower"){
            value.gravityPower = kv.Value.GetSingle();
            continue;
        }

        if(key=="gravityDir"){
            value.gravityDir = Deserialize_vrm_secondaryAnimation_boneGroups__gravityDir(kv.Value);
            continue;
        }

        if(key=="dragForce"){
            value.dragForce = kv.Value.GetSingle();
            continue;
        }

        if(key=="center"){
            value.center = kv.Value.GetInt32();
            continue;
        }

        if(key=="hitRadius"){
            value.hitRadius = kv.Value.GetSingle();
            continue;
        }

        if(key=="bones"){
            value.bones = Deserialize_vrm_secondaryAnimation_boneGroups__bones(kv.Value);
            continue;
        }

        if(key=="colliderGroups"){
            value.colliderGroups = Deserialize_vrm_secondaryAnimation_boneGroups__colliderGroups(kv.Value);
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_vrm_secondaryAnimation_boneGroups__gravityDir(JsonNode parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_vrm_secondaryAnimation_boneGroups__bones(JsonNode parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static Int32[] Deserialize_vrm_secondaryAnimation_boneGroups__colliderGroups(JsonNode parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static List<VRM.glTF_VRM_SecondaryAnimationColliderGroup> Deserialize_vrm_secondaryAnimation_colliderGroups(JsonNode parsed)
{
    var value = new List<glTF_VRM_SecondaryAnimationColliderGroup>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_secondaryAnimation_colliderGroups_LIST(x));
    }
	return value;
}
public static glTF_VRM_SecondaryAnimationColliderGroup Deserialize_vrm_secondaryAnimation_colliderGroups_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_SecondaryAnimationColliderGroup();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.node = kv.Value.GetInt32();
            continue;
        }

        if(key=="colliders"){
            value.colliders = Deserialize_vrm_secondaryAnimation_colliderGroups__colliders(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_SecondaryAnimationCollider> Deserialize_vrm_secondaryAnimation_colliderGroups__colliders(JsonNode parsed)
{
    var value = new List<glTF_VRM_SecondaryAnimationCollider>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_secondaryAnimation_colliderGroups__colliders_LIST(x));
    }
	return value;
}
public static glTF_VRM_SecondaryAnimationCollider Deserialize_vrm_secondaryAnimation_colliderGroups__colliders_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_SecondaryAnimationCollider();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.offset = Deserialize_vrm_secondaryAnimation_colliderGroups__colliders__offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.radius = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_vrm_secondaryAnimation_colliderGroups__colliders__offset(JsonNode parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_Material> Deserialize_vrm_materialProperties(JsonNode parsed)
{
    var value = new List<glTF_VRM_Material>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_vrm_materialProperties_LIST(x));
    }
	return value;
}
public static glTF_VRM_Material Deserialize_vrm_materialProperties_LIST(JsonNode parsed)
{
    var value = new glTF_VRM_Material();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="shader"){
            value.shader = kv.Value.GetString();
            continue;
        }

        if(key=="renderQueue"){
            value.renderQueue = kv.Value.GetInt32();
            continue;
        }

        if(key=="floatProperties"){
            value.floatProperties = Deserialize_vrm_materialProperties__floatProperties(kv.Value);
            continue;
        }

        if(key=="vectorProperties"){
            value.vectorProperties = Deserialize_vrm_materialProperties__vectorProperties(kv.Value);
            continue;
        }

        if(key=="textureProperties"){
            value.textureProperties = Deserialize_vrm_materialProperties__textureProperties(kv.Value);
            continue;
        }

        if(key=="keywordMap"){
            value.keywordMap = Deserialize_vrm_materialProperties__keywordMap(kv.Value);
            continue;
        }

        if(key=="tagMap"){
            value.tagMap = Deserialize_vrm_materialProperties__tagMap(kv.Value);
            continue;
        }

    }
    return value;
}

 
public static Dictionary<String, Single> Deserialize_vrm_materialProperties__floatProperties(JsonNode parsed)
{
    var value = new Dictionary<string, Single>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetSingle());
    }
	return value;
}

 
public static Dictionary<String, Single[]> Deserialize_vrm_materialProperties__vectorProperties(JsonNode parsed)
{
    var value = new Dictionary<string, Single[]>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), Deserialize_vrm_materialProperties__vectorProperties_DICT(kv.Value));
    }
	return value;
}

public static Single[] Deserialize_vrm_materialProperties__vectorProperties_DICT(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

 
public static Dictionary<String, Int32> Deserialize_vrm_materialProperties__textureProperties(JsonNode parsed)
{
    var value = new Dictionary<string, Int32>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetInt32());
    }
	return value;
}

 
public static Dictionary<String, Boolean> Deserialize_vrm_materialProperties__keywordMap(JsonNode parsed)
{
    var value = new Dictionary<string, Boolean>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetBoolean());
    }
	return value;
}

 
public static Dictionary<String, String> Deserialize_vrm_materialProperties__tagMap(JsonNode parsed)
{
    var value = new Dictionary<string, String>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetString());
    }
	return value;
}

} // VrmfDeserializer
} // VRM
