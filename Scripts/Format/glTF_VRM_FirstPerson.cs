using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using UniJSON;

namespace VRM
{
    [Serializable]
    public class glTF_VRM_DegreeMap : UniGLTF.JsonSerializableBase
    {
        // time, value, inTangent, outTangent
        public float[] curve;
        public float xRange = 90.0f;
        public float yRange = 10.0f;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            if (curve != null)
            {
                f.KeyValue(() => curve);
            }
            f.KeyValue(() => xRange);
            f.KeyValue(() => yRange);
        }

        public void Apply(CurveMapper mapper)
        {
            curve = mapper.Curve.keys.SelectMany(x => new float[] { x.time, x.value, x.inTangent, x.outTangent }).ToArray();
            xRange = mapper.CurveXRangeDegree;
            yRange = mapper.CurveYRangeDegree;
        }
    }

    public enum FirstPersonFlag
    {
        Auto, // Create headlessModel
        Both, // Default layer
        ThirdPersonOnly,
        FirstPersonOnly,
    }

    [Serializable]
    public class glTF_VRM_MeshAnnotation : JsonSerializableBase
    {
        public int mesh;

        public string firstPersonFlag;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => mesh);
            f.KeyValue(() => firstPersonFlag);
        }
    }

    public enum LookAtType
    {
        None,
        Bone,
        BlendShape,
    }

    [Serializable]
    [JsonSchema(Title = "vrm.firstperson")]
    public class glTF_VRM_Firstperson : UniGLTF.JsonSerializableBase
    {
        public int firstPersonBone = -1;

        public Vector3 firstPersonBoneOffset;

        public List<glTF_VRM_MeshAnnotation> meshAnnotations = new List<glTF_VRM_MeshAnnotation>();

        // lookat
        public string lookAtTypeName = "Bone";
        public LookAtType lookAtType
        {
            get
            {
                return EnumUtil.TryParseOrDefault<LookAtType>(lookAtTypeName);
            }
            set { lookAtTypeName = value.ToString(); }
        }
        public glTF_VRM_DegreeMap lookAtHorizontalInner = new glTF_VRM_DegreeMap();
        public glTF_VRM_DegreeMap lookAtHorizontalOuter = new glTF_VRM_DegreeMap();
        public glTF_VRM_DegreeMap lookAtVerticalDown = new glTF_VRM_DegreeMap();
        public glTF_VRM_DegreeMap lookAtVerticalUp = new glTF_VRM_DegreeMap();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => firstPersonBone);
            f.KeyValue(() => firstPersonBoneOffset);
            f.KeyValue(() => meshAnnotations);

            f.KeyValue(() => lookAtTypeName);
            f.KeyValue(() => lookAtHorizontalInner);
            f.KeyValue(() => lookAtHorizontalOuter);
            f.KeyValue(() => lookAtVerticalDown);
            f.KeyValue(() => lookAtVerticalUp);
        }
    }
}
