
using System;
using System.Collections.Generic;
using UniJSON;
using UnityEngine;
using VRM;

namespace VRM {

    static public class IFormatterExtensionsGltf
    {


    /// vrm
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_extensions value)
    {
        f.BeginMap(0); // dummy

        if(value.exporterVersion!=null)
        {
            f.Key("exporterVersion"); f.GenSerialize(value.exporterVersion);
        }

        if(value.specVersion!=null)
        {
            f.Key("specVersion"); f.GenSerialize(value.specVersion);
        }

        if(value.meta!=null)
        {
            f.Key("meta"); f.GenSerialize(value.meta);
        }

        if(value.humanoid!=null)
        {
            f.Key("humanoid"); f.GenSerialize(value.humanoid);
        }

        if(value.firstPerson!=null)
        {
            f.Key("firstPerson"); f.GenSerialize(value.firstPerson);
        }

        if(value.blendShapeMaster!=null)
        {
            f.Key("blendShapeMaster"); f.GenSerialize(value.blendShapeMaster);
        }

        if(value.secondaryAnimation!=null)
        {
            f.Key("secondaryAnimation"); f.GenSerialize(value.secondaryAnimation);
        }

        if(value.materialProperties!=null)
        {
            f.Key("materialProperties"); f.GenSerialize(value.materialProperties);
        }

        f.EndMap();
    }

    public static void GenSerialize(this JsonFormatter f, String value)
    {
        f.Value(value);
    }

    /// vrm/meta
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_Meta value)
    {
        f.BeginMap(0); // dummy

        if(value.title!=null)
        {
            f.Key("title"); f.GenSerialize(value.title);
        }

        if(value.version!=null)
        {
            f.Key("version"); f.GenSerialize(value.version);
        }

        if(value.author!=null)
        {
            f.Key("author"); f.GenSerialize(value.author);
        }

        if(value.contactInformation!=null)
        {
            f.Key("contactInformation"); f.GenSerialize(value.contactInformation);
        }

        if(value.reference!=null)
        {
            f.Key("reference"); f.GenSerialize(value.reference);
        }

        
        {
            f.Key("texture"); f.GenSerialize(value.texture);
        }

        if(value.allowedUserName!=null)
        {
            f.Key("allowedUserName"); f.GenSerialize(value.allowedUserName);
        }

        if(value.violentUssageName!=null)
        {
            f.Key("violentUssageName"); f.GenSerialize(value.violentUssageName);
        }

        if(value.sexualUssageName!=null)
        {
            f.Key("sexualUssageName"); f.GenSerialize(value.sexualUssageName);
        }

        if(value.commercialUssageName!=null)
        {
            f.Key("commercialUssageName"); f.GenSerialize(value.commercialUssageName);
        }

        if(value.otherPermissionUrl!=null)
        {
            f.Key("otherPermissionUrl"); f.GenSerialize(value.otherPermissionUrl);
        }

        if(value.licenseName!=null)
        {
            f.Key("licenseName"); f.GenSerialize(value.licenseName);
        }

        if(value.otherLicenseUrl!=null)
        {
            f.Key("otherLicenseUrl"); f.GenSerialize(value.otherLicenseUrl);
        }

        f.EndMap();
    }

    public static void GenSerialize(this JsonFormatter f, Int32 value)
    {
        f.Value(value);
    }

    /// vrm/humanoid
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_Humanoid value)
    {
        f.BeginMap(0); // dummy

        if(value.humanBones!=null)
        {
            f.Key("humanBones"); f.GenSerialize(value.humanBones);
        }

        
        {
            f.Key("armStretch"); f.GenSerialize(value.armStretch);
        }

        
        {
            f.Key("legStretch"); f.GenSerialize(value.legStretch);
        }

        
        {
            f.Key("upperArmTwist"); f.GenSerialize(value.upperArmTwist);
        }

        
        {
            f.Key("lowerArmTwist"); f.GenSerialize(value.lowerArmTwist);
        }

        
        {
            f.Key("upperLegTwist"); f.GenSerialize(value.upperLegTwist);
        }

        
        {
            f.Key("lowerLegTwist"); f.GenSerialize(value.lowerLegTwist);
        }

        
        {
            f.Key("feetSpacing"); f.GenSerialize(value.feetSpacing);
        }

        
        {
            f.Key("hasTranslationDoF"); f.GenSerialize(value.hasTranslationDoF);
        }

        f.EndMap();
    }

    /// vrm/humanoid/humanBones
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_HumanoidBone> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/humanoid/humanBones[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_HumanoidBone value)
    {
        f.BeginMap(0); // dummy

        if(value.bone!=null)
        {
            f.Key("bone"); f.GenSerialize(value.bone);
        }

        
        {
            f.Key("node"); f.GenSerialize(value.node);
        }

        
        {
            f.Key("useDefaultValues"); f.GenSerialize(value.useDefaultValues);
        }

        if(value.min!=Vector3.zero)
        {
            f.Key("min"); f.GenSerialize(value.min);
        }

        if(value.max!=Vector3.zero)
        {
            f.Key("max"); f.GenSerialize(value.max);
        }

        if(value.center!=Vector3.zero)
        {
            f.Key("center"); f.GenSerialize(value.center);
        }

        if(value.axisLength>0)
        {
            f.Key("axisLength"); f.GenSerialize(value.axisLength);
        }

        f.EndMap();
    }

    public static void GenSerialize(this JsonFormatter f, Boolean value)
    {
        f.Value(value);
    }

    /// vrm/humanoid/humanBones[]/min
    public static void GenSerialize(this JsonFormatter f, Vector3 value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("x"); f.GenSerialize(value.x);
        }

        
        {
            f.Key("y"); f.GenSerialize(value.y);
        }

        
        {
            f.Key("z"); f.GenSerialize(value.z);
        }

        f.EndMap();
    }

    public static void GenSerialize(this JsonFormatter f, Single value)
    {
        f.Value(value);
    }

    /// vrm/firstPerson
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_Firstperson value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("firstPersonBone"); f.GenSerialize(value.firstPersonBone);
        }

        
        {
            f.Key("firstPersonBoneOffset"); f.GenSerialize(value.firstPersonBoneOffset);
        }

        if(value.meshAnnotations!=null)
        {
            f.Key("meshAnnotations"); f.GenSerialize(value.meshAnnotations);
        }

        if(value.lookAtTypeName!=null)
        {
            f.Key("lookAtTypeName"); f.GenSerialize(value.lookAtTypeName);
        }

        if(value.lookAtHorizontalInner!=null)
        {
            f.Key("lookAtHorizontalInner"); f.GenSerialize(value.lookAtHorizontalInner);
        }

        if(value.lookAtHorizontalOuter!=null)
        {
            f.Key("lookAtHorizontalOuter"); f.GenSerialize(value.lookAtHorizontalOuter);
        }

        if(value.lookAtVerticalDown!=null)
        {
            f.Key("lookAtVerticalDown"); f.GenSerialize(value.lookAtVerticalDown);
        }

        if(value.lookAtVerticalUp!=null)
        {
            f.Key("lookAtVerticalUp"); f.GenSerialize(value.lookAtVerticalUp);
        }

        f.EndMap();
    }

    /// vrm/firstPerson/meshAnnotations
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_MeshAnnotation> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/firstPerson/meshAnnotations[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_MeshAnnotation value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("mesh"); f.GenSerialize(value.mesh);
        }

        if(value.firstPersonFlag!=null)
        {
            f.Key("firstPersonFlag"); f.GenSerialize(value.firstPersonFlag);
        }

        f.EndMap();
    }

    /// vrm/firstPerson/lookAtHorizontalInner
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_DegreeMap value)
    {
        f.BeginMap(0); // dummy

        if(value.curve!=null)
        {
            f.Key("curve"); f.GenSerialize(value.curve);
        }

        
        {
            f.Key("xRange"); f.GenSerialize(value.xRange);
        }

        
        {
            f.Key("yRange"); f.GenSerialize(value.yRange);
        }

        f.EndMap();
    }

    /// vrm/firstPerson/lookAtHorizontalInner/curve
    public static void GenSerialize(this JsonFormatter f, Single[] value)
    {
        f.BeginList(value.Length);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
                    
    /// vrm/blendShapeMaster
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_BlendShapeMaster value)
    {
        f.BeginMap(0); // dummy

        if(value.blendShapeGroups!=null)
        {
            f.Key("blendShapeGroups"); f.GenSerialize(value.blendShapeGroups);
        }

        f.EndMap();
    }

    /// vrm/blendShapeMaster/blendShapeGroups
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_BlendShapeGroup> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/blendShapeMaster/blendShapeGroups[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_BlendShapeGroup value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.presetName!=null)
        {
            f.Key("presetName"); f.GenSerialize(value.presetName);
        }

        if(value.binds!=null)
        {
            f.Key("binds"); f.GenSerialize(value.binds);
        }

        if(value.materialValues!=null)
        {
            f.Key("materialValues"); f.GenSerialize(value.materialValues);
        }

        
        {
            f.Key("isBinary"); f.GenSerialize(value.isBinary);
        }

        f.EndMap();
    }

    /// vrm/blendShapeMaster/blendShapeGroups[]/binds
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_BlendShapeBind> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/blendShapeMaster/blendShapeGroups[]/binds[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_BlendShapeBind value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("mesh"); f.GenSerialize(value.mesh);
        }

        
        {
            f.Key("index"); f.GenSerialize(value.index);
        }

        
        {
            f.Key("weight"); f.GenSerialize(value.weight);
        }

        f.EndMap();
    }

    /// vrm/blendShapeMaster/blendShapeGroups[]/materialValues
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_MaterialValueBind> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/blendShapeMaster/blendShapeGroups[]/materialValues[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_MaterialValueBind value)
    {
        f.BeginMap(0); // dummy

        if(value.materialName!=null)
        {
            f.Key("materialName"); f.GenSerialize(value.materialName);
        }

        if(value.propertyName!=null)
        {
            f.Key("propertyName"); f.GenSerialize(value.propertyName);
        }

        if(value.targetValue!=null)
        {
            f.Key("targetValue"); f.GenSerialize(value.targetValue);
        }

        f.EndMap();
    }

    /// vrm/secondaryAnimation
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_SecondaryAnimation value)
    {
        f.BeginMap(0); // dummy

        if(value.boneGroups!=null)
        {
            f.Key("boneGroups"); f.GenSerialize(value.boneGroups);
        }

        if(value.colliderGroups!=null)
        {
            f.Key("colliderGroups"); f.GenSerialize(value.colliderGroups);
        }

        f.EndMap();
    }

    /// vrm/secondaryAnimation/boneGroups
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_SecondaryAnimationGroup> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/secondaryAnimation/boneGroups[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_SecondaryAnimationGroup value)
    {
        f.BeginMap(0); // dummy

        if(value.comment!=null)
        {
            f.Key("comment"); f.GenSerialize(value.comment);
        }

        
        {
            f.Key("stiffiness"); f.GenSerialize(value.stiffiness);
        }

        
        {
            f.Key("gravityPower"); f.GenSerialize(value.gravityPower);
        }

        
        {
            f.Key("gravityDir"); f.GenSerialize(value.gravityDir);
        }

        
        {
            f.Key("dragForce"); f.GenSerialize(value.dragForce);
        }

        
        {
            f.Key("center"); f.GenSerialize(value.center);
        }

        
        {
            f.Key("hitRadius"); f.GenSerialize(value.hitRadius);
        }

        if(value.bones!=null)
        {
            f.Key("bones"); f.GenSerialize(value.bones);
        }

        if(value.colliderGroups!=null)
        {
            f.Key("colliderGroups"); f.GenSerialize(value.colliderGroups);
        }

        f.EndMap();
    }

    /// vrm/secondaryAnimation/boneGroups[]/bones
    public static void GenSerialize(this JsonFormatter f, Int32[] value)
    {
        f.BeginList(value.Length);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
                    
    /// vrm/secondaryAnimation/colliderGroups
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_SecondaryAnimationColliderGroup> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/secondaryAnimation/colliderGroups[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_SecondaryAnimationColliderGroup value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("node"); f.GenSerialize(value.node);
        }

        if(value.colliders!=null)
        {
            f.Key("colliders"); f.GenSerialize(value.colliders);
        }

        f.EndMap();
    }

    /// vrm/secondaryAnimation/colliderGroups[]/colliders
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_SecondaryAnimationCollider> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/secondaryAnimation/colliderGroups[]/colliders[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_SecondaryAnimationCollider value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("offset"); f.GenSerialize(value.offset);
        }

        
        {
            f.Key("radius"); f.GenSerialize(value.radius);
        }

        f.EndMap();
    }

    /// vrm/materialProperties
    public static void GenSerialize(this JsonFormatter f, List<glTF_VRM_Material> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// vrm/materialProperties[]
    public static void GenSerialize(this JsonFormatter f, glTF_VRM_Material value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.shader!=null)
        {
            f.Key("shader"); f.GenSerialize(value.shader);
        }

        
        {
            f.Key("renderQueue"); f.GenSerialize(value.renderQueue);
        }

        if(value.floatProperties!=null)
        {
            f.Key("floatProperties"); f.GenSerialize(value.floatProperties);
        }

        if(value.vectorProperties!=null)
        {
            f.Key("vectorProperties"); f.GenSerialize(value.vectorProperties);
        }

        if(value.textureProperties!=null)
        {
            f.Key("textureProperties"); f.GenSerialize(value.textureProperties);
        }

        if(value.keywordMap!=null)
        {
            f.Key("keywordMap"); f.GenSerialize(value.keywordMap);
        }

        if(value.tagMap!=null)
        {
            f.Key("tagMap"); f.GenSerialize(value.tagMap);
        }

        f.EndMap();
    }

    /// vrm/materialProperties[]/floatProperties
    public static void GenSerialize(this JsonFormatter f, Dictionary<string, Single> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// vrm/materialProperties[]/vectorProperties
    public static void GenSerialize(this JsonFormatter f, Dictionary<string, Single[]> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// vrm/materialProperties[]/textureProperties
    public static void GenSerialize(this JsonFormatter f, Dictionary<string, Int32> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// vrm/materialProperties[]/keywordMap
    public static void GenSerialize(this JsonFormatter f, Dictionary<string, Boolean> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// vrm/materialProperties[]/tagMap
    public static void GenSerialize(this JsonFormatter f, Dictionary<string, String> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    } // class
} // namespace
