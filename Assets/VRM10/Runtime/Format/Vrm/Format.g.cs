// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_vrm
{

    public enum AvatarPermissionType
    {
        onlyAuthor,
        explicitlyLicensedPerson,
        everyone,

    }

    public enum CommercialUsageType
    {
        personalNonProfit,
        personalProfit,
        corporation,

    }

    public enum CreditNotationType
    {
        required,
        unnecessary,
        abandoned,

    }

    public enum ModificationType
    {
        prohibited,
        inherited,
        notInherited,

    }

    public class Meta
    {
        // The name of the model
        public string Name;

        // The version of the model
        public string Version;

        // Authors of the model
        public List<string> Authors;

        // An information that describes the copyright of the model
        public string CopyrightInformation;

        // An information that describes the contact information of the author
        public string ContactInformation;

        // References / original works of the model
        public List<string> References;

        // Third party licenses of the model, if required. You can use line breaks
        public string ThirdPartyLicenses;

        // The index to access the thumbnail image of the avatar model in gltf.images. The texture resolution of 1024x1024 is recommended. It must be square. Preferable resolution is 1024 x 1024. This is for the application to use as an icon.
        public int? ThumbnailImage;

        // A person who can perform with this avatars
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public AvatarPermissionType AvatarPermission;

        // A flag that permits to use this avatar in excessively violent contents
        public bool? AllowExcessivelyViolentUsage;

        // A flag that permits to use this avatar in excessively sexual contents
        public bool? AllowExcessivelySexualUsage;

        // An option that permits to use this avatar in commercial products
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public CommercialUsageType CommercialUsage;

        // A flag that permits to use this avatar in political or religious contents
        public bool? AllowPoliticalOrReligiousUsage;

        // An option that forces or abandons to display the credit of this avatar
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public CreditNotationType CreditNotation;

        // A flag that permits to redistribute this avatar
        public bool? AllowRedistribution;

        // An option that controls the condition to modify this avatar
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ModificationType Modification;

        // Describe the URL links of other license
        public string OtherLicenseUrl;
    }

    public class HumanBone
    {
        // Represents a single glTF node tied to this humanBone.
        public int? Node;
    }

    public class HumanBones
    {
        // Represents a single bone of a Humanoid.
        public HumanBone Hips;

        // Represents a single bone of a Humanoid.
        public HumanBone Spine;

        // Represents a single bone of a Humanoid.
        public HumanBone Chest;

        // Represents a single bone of a Humanoid.
        public HumanBone UpperChest;

        // Represents a single bone of a Humanoid.
        public HumanBone Neck;

        // Represents a single bone of a Humanoid.
        public HumanBone Head;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftEye;

        // Represents a single bone of a Humanoid.
        public HumanBone RightEye;

        // Represents a single bone of a Humanoid.
        public HumanBone Jaw;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftUpperLeg;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftLowerLeg;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftFoot;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftToes;

        // Represents a single bone of a Humanoid.
        public HumanBone RightUpperLeg;

        // Represents a single bone of a Humanoid.
        public HumanBone RightLowerLeg;

        // Represents a single bone of a Humanoid.
        public HumanBone RightFoot;

        // Represents a single bone of a Humanoid.
        public HumanBone RightToes;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftShoulder;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftUpperArm;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftLowerArm;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftHand;

        // Represents a single bone of a Humanoid.
        public HumanBone RightShoulder;

        // Represents a single bone of a Humanoid.
        public HumanBone RightUpperArm;

        // Represents a single bone of a Humanoid.
        public HumanBone RightLowerArm;

        // Represents a single bone of a Humanoid.
        public HumanBone RightHand;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftThumbProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftThumbIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftThumbDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftIndexProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftIndexIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftIndexDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftMiddleProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftMiddleIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftMiddleDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftRingProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftRingIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftRingDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftLittleProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftLittleIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftLittleDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightThumbProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightThumbIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone RightThumbDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightIndexProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightIndexIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone RightIndexDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightMiddleProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightMiddleIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone RightMiddleDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightRingProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightRingIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone RightRingDistal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightLittleProximal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightLittleIntermediate;

        // Represents a single bone of a Humanoid.
        public HumanBone RightLittleDistal;
    }

    public class Humanoid
    {
        // Represents a set of humanBones of a humanoid.
        public HumanBones HumanBones;
    }

    public enum FirstPersonType
    {
        auto,
        both,
        thirdPersonOnly,
        firstPersonOnly,

    }

    public class MeshAnnotation
    {
        // The index of the node that attached to target mesh.
        public int? Node;

        // How the camera interprets the mesh.
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public FirstPersonType FirstPersonType;
    }

    public class FirstPerson
    {
        // Mesh rendering annotation for cameras. 'required' :   [  'mesh' ,  'firstPersonType'  ]
        public List<MeshAnnotation> MeshAnnotations;
    }

    public enum LookAtType
    {
        bone,
        expression,

    }

    public class LookAtRangeMap
    {
        // Yaw and pitch angles  ( degrees )  between the head bone forward vector and the eye gaze LookAt vector
        public float? InputMaxValue;

        // Degree for LookAtType.bone ,  Weight for LookAtType.blendShape
        public float? OutputScale;
    }

    public class LookAt
    {
        // The origin of LookAt. Position offset from the head bone
        public float[] OffsetFromHeadBone;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public LookAtType LookAtType;

        // Horizontal inward movement. The left eye moves right. The right eye moves left.
        public LookAtRangeMap LookAtHorizontalInner;

        // Horizontal outward movement. The left eye moves left. The right eye moves right.
        public LookAtRangeMap LookAtHorizontalOuter;

        // Vertical downward movement. Both eyes move upwards
        public LookAtRangeMap LookAtVerticalDown;

        // Vertical upward movement. Both eyes move downwards
        public LookAtRangeMap LookAtVerticalUp;
    }

    public enum ExpressionPreset
    {
        custom,
        happy,
        angry,
        sad,
        relaxed,
        surprised,
        aa,
        ih,
        ou,
        ee,
        oh,
        blink,
        blinkLeft,
        blinkRight,
        lookUp,
        lookDown,
        lookLeft,
        lookRight,
        neutral,

    }

    public class MorphTargetBind
    {
        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // The index of the node that attached to target mesh.
        public int? Node;

        // The index of the morph target in the mesh.
        public int? Index;

        // The weight value of target morph target.
        public float? Weight;
    }

    public enum MaterialColorType
    {
        color,
        emissionColor,
        shadeColor,
        rimColor,
        outlineColor,

    }

    public class MaterialColorBind
    {
        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // target material
        public int? Material;

        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public MaterialColorType Type;

        // target color
        public float[] TargetValue;
    }

    public class TextureTransformBind
    {
        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // target material
        public int? Material;

        // uv scaling for TEXCOORD_0
        public float[] Scaling;

        // uv offset for TEXCOORD_0
        public float[] Offset;
    }

    public enum ExpressionOverrideType
    {
        none,
        block,
        blend,

    }

    public class Expression
    {
        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        // Use only if the preset is custom. Unique within the model
        public string Name;

        // Functions of Expression
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ExpressionPreset Preset;

        // Specify a morph target
        public List<MorphTargetBind> MorphTargetBinds;

        // Material color animation references
        public List<MaterialColorBind> MaterialColorBinds;

        // Texture transform animation references
        public List<TextureTransformBind> TextureTransformBinds;

        // Interpret non-zero values as 1
        public bool? IsBinary;

        // Override values of Blink expressions when this Expression is enabled
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ExpressionOverrideType OverrideBlink;

        // Override values of LookAt expressions when this Expression is enabled
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ExpressionOverrideType OverrideLookAt;

        // Override values of Mouth expressions when this Expression is enabled
        [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
        public ExpressionOverrideType OverrideMouth;
    }

    public class VRMC_vrm
    {
        public const string ExtensionName = "VRMC_vrm";
        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);

        public string SpecVersion;

        public Meta Meta;

        // Correspondence between nodes and human bones
        public Humanoid Humanoid;

        // First-person perspective settings
        public FirstPerson FirstPerson;

        // Eye gaze control
        public LookAt LookAt;

        // Definitions of expressions
        public List<Expression> Expressions;
    }
}
