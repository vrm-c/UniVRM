using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using UniJSON;


namespace VRM
{
    [Serializable]
    [JsonSchema(Title = "vrm.firstperson.degreemap")]
    public class glTF_VRM_DegreeMap
    {
        [JsonSchema(Description = "None linear mapping params. time, value, inTangent, outTangent")]
        public float[] curve;

        [JsonSchema(Description = "Look at input clamp range degree.")]
        public float xRange = 90.0f;

        [JsonSchema(Description = "Look at map range degree from xRange.")]
        public float yRange = 10.0f;
    }

    public enum FirstPersonFlag
    {
        Auto, // Create headlessModel
        Both, // Default layer
        ThirdPersonOnly,
        FirstPersonOnly,
    }

    [Serializable]
    [JsonSchema(Title = "vrm.firstperson.meshannotation")]
    public class glTF_VRM_MeshAnnotation
    {
        // When the value is -1, it means that no target mesh is found.
        [JsonSchema(Minimum = 0)]
        public int mesh;

        public string firstPersonFlag;
    }

    public enum LookAtType
    {
        None,
        Bone,
        BlendShape,
    }

    [Serializable]
    [JsonSchema(Title = "vrm.firstperson")]
    public class glTF_VRM_Firstperson
    {
        // When the value is -1, it means that no bone for first person is found.
        [JsonSchema(Description = "The bone whose rendering should be turned off in first-person view. Usually Head is specified.", Minimum = 0, ExplicitIgnorableValue = -1)]
        public int firstPersonBone = -1;

        [JsonSchema(Description = @"The target position of the VR headset in first-person view. It is assumed that an offset from the head bone to the VR headset is added."
        )]
        public Vector3 firstPersonBoneOffset;

        [JsonSchema(Description = "Switch display / undisplay for each mesh in first-person view or the others.")]
        public List<glTF_VRM_MeshAnnotation> meshAnnotations = new List<glTF_VRM_MeshAnnotation>();

        // lookat
        [JsonSchema(Description = "Eye controller mode.", EnumValues = new object[] {
            "Bone",
            "BlendShape",
        }, EnumSerializationType = EnumSerializationType.AsString)]
        public string lookAtTypeName = "Bone";
        public LookAtType lookAtType
        {
            get
            {
                return CacheEnum.TryParseOrDefault<LookAtType>(lookAtTypeName, true);
            }
            set { lookAtTypeName = value.ToString(); }
        }

        [JsonSchema(Description = "Eye controller setting.")]
        public glTF_VRM_DegreeMap lookAtHorizontalInner = new glTF_VRM_DegreeMap();

        [JsonSchema(Description = "Eye controller setting.")]
        public glTF_VRM_DegreeMap lookAtHorizontalOuter = new glTF_VRM_DegreeMap();

        [JsonSchema(Description = "Eye controller setting.")]
        public glTF_VRM_DegreeMap lookAtVerticalDown = new glTF_VRM_DegreeMap();

        [JsonSchema(Description = "Eye controller setting.")]
        public glTF_VRM_DegreeMap lookAtVerticalUp = new glTF_VRM_DegreeMap();
    }
}
