// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_vrm
{

    public enum AvatarPermissionType
    {
        onlyAuthor,
        onlySeparatelyLicensedPerson,
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

    }

    public enum ModificationType
    {
        prohibited,
        allowModification,
        allowModificationRedistribution,

    }

    public class Meta
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

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

        // The index to the thumbnail image of the model in gltf.images
        public int? ThumbnailImage;

        // A URL towards the license document this model refers to
        public string LicenseUrl;

        // A person who can perform as an avatar with this model
        public AvatarPermissionType AvatarPermission;

        // A flag that permits to use this model in excessively violent contents
        public bool? AllowExcessivelyViolentUsage;

        // A flag that permits to use this model in excessively sexual contents
        public bool? AllowExcessivelySexualUsage;

        // An option that permits to use this model in commercial products
        public CommercialUsageType CommercialUsage;

        // A flag that permits to use this model in political or religious contents
        public bool? AllowPoliticalOrReligiousUsage;

        // A flag that permits to use this model in contents contain anti-social activities or hate speeches
        public bool? AllowAntisocialOrHateUsage;

        // An option that forces or abandons to display the credit of this model
        public CreditNotationType CreditNotation;

        // A flag that permits to redistribute this model
        public bool? AllowRedistribution;

        // An option that controls the condition to modify this model
        public ModificationType Modification;

        // Describe the URL links of other license
        public string OtherLicenseUrl;
    }

    public class HumanBone
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

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
        public HumanBone LeftThumbMetacarpal;

        // Represents a single bone of a Humanoid.
        public HumanBone LeftThumbProximal;

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
        public HumanBone RightThumbMetacarpal;

        // Represents a single bone of a Humanoid.
        public HumanBone RightThumbProximal;

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
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

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
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The index of the node that attached to target mesh.
        public int? Node;

        // How the camera interprets the mesh.
        public FirstPersonType Type;
    }

    public class FirstPerson
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Mesh rendering annotation for cameras.
        public List<MeshAnnotation> MeshAnnotations;
    }

    public enum LookAtType
    {
        bone,
        expression,

    }

    public class LookAtRangeMap
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Yaw and pitch angles  ( degrees )  between the head bone forward vector and the eye gaze LookAt vector
        public float? InputMaxValue;

        // Degree for type.bone, Weight for type.expressions
        public float? OutputScale;
    }

    public class LookAt
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The origin of LookAt. Position offset from the head bone
        public float[] OffsetFromHeadBone;

        public LookAtType Type;

        // Horizontal inward movement. The left eye moves right. The right eye moves left.
        public LookAtRangeMap RangeMapHorizontalInner;

        // Horizontal outward movement. The left eye moves left. The right eye moves right.
        public LookAtRangeMap RangeMapHorizontalOuter;

        // Vertical downward movement. Both eyes move upwards
        public LookAtRangeMap RangeMapVerticalDown;

        // Vertical upward movement. Both eyes move downwards
        public LookAtRangeMap RangeMapVerticalUp;
    }

    public class MorphTargetBind
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

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
        matcapColor,
        rimColor,
        outlineColor,

    }

    public class MaterialColorBind
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // target material
        public int? Material;

        public MaterialColorType Type;

        // target color
        public float[] TargetValue;
    }

    public class TextureTransformBind
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // target material
        public int? Material;

        // uv scaling for TEXCOORD_0
        public float[] Scale;

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
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specify a morph target
        public List<MorphTargetBind> MorphTargetBinds;

        // Material color animation references
        public List<MaterialColorBind> MaterialColorBinds;

        // Texture transform animation references
        public List<TextureTransformBind> TextureTransformBinds;

        // A value greater than 0.5 is 1.0, otherwise 0.0
        public bool? IsBinary;

        // Override values of Blink expressions when this Expression is enabled
        public ExpressionOverrideType OverrideBlink;

        // Override values of LookAt expressions when this Expression is enabled
        public ExpressionOverrideType OverrideLookAt;

        // Override values of Mouth expressions when this Expression is enabled
        public ExpressionOverrideType OverrideMouth;
    }

    public class Preset
    {
        // Definition of expression by weighted animation
        public Expression Happy;

        // Definition of expression by weighted animation
        public Expression Angry;

        // Definition of expression by weighted animation
        public Expression Sad;

        // Definition of expression by weighted animation
        public Expression Relaxed;

        // Definition of expression by weighted animation
        public Expression Surprised;

        // Definition of expression by weighted animation
        public Expression Aa;

        // Definition of expression by weighted animation
        public Expression Ih;

        // Definition of expression by weighted animation
        public Expression Ou;

        // Definition of expression by weighted animation
        public Expression Ee;

        // Definition of expression by weighted animation
        public Expression Oh;

        // Definition of expression by weighted animation
        public Expression Blink;

        // Definition of expression by weighted animation
        public Expression BlinkLeft;

        // Definition of expression by weighted animation
        public Expression BlinkRight;

        // Definition of expression by weighted animation
        public Expression LookUp;

        // Definition of expression by weighted animation
        public Expression LookDown;

        // Definition of expression by weighted animation
        public Expression LookLeft;

        // Definition of expression by weighted animation
        public Expression LookRight;

        // Definition of expression by weighted animation
        public Expression Neutral;
    }

    public class Expressions
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Preset expressions
        public Preset Preset;

        // Custom expressions
        public Dictionary<string, Expression> Custom;
    }

    public class VRMC_vrm
    {
        public const string ExtensionName = "VRMC_vrm";

        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specification version of VRMC_vrm
        public string SpecVersion;

        // Meta informations of the VRM model
        public Meta Meta;

        // Correspondence between nodes and human bones
        public Humanoid Humanoid;

        // First-person perspective settings
        public FirstPerson FirstPerson;

        // Eye gaze control
        public LookAt LookAt;

        // Definition of expressions
        public Expressions Expressions;
    }
}
