using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public class glTF_VRM_SecondaryAnimationCollider : JsonSerializableBase
    {
        public Vector3 offset;
        public float radius;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => offset);
            f.KeyValue(() => radius);
        }
    }

    [Serializable]
    public class glTF_VRM_SecondaryAnimationColliderGroup : JsonSerializableBase
    {
        public int node;
        public List<glTF_VRM_SecondaryAnimationCollider> colliders = new List<glTF_VRM_SecondaryAnimationCollider>();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => node);
            f.KeyValue(() => colliders);
        }
    }


    [Serializable]
    public class glTF_VRM_SecondaryAnimationGroup : JsonSerializableBase
    {
        public string comment;
        public float stiffiness;
        public float gravityPower;
        public Vector3 gravityDir;
        public float dragForce;
        public int center;
        public float hitRadius;
        public int[] bones = new int[] { };
        public int[] colliderGroups = new int[] { };

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => comment);
            f.KeyValue(() => stiffiness);
            f.KeyValue(() => gravityPower);
            f.KeyValue(() => gravityDir);
            f.KeyValue(() => dragForce);
            f.KeyValue(() => center);
            f.KeyValue(() => hitRadius);
            f.KeyValue(() => bones);
            f.KeyValue(() => colliderGroups);
        }
    }

    [Serializable]
    [JsonSchema(Title = "vrm.secondaryanimation")]
    public class glTF_VRM_SecondaryAnimation : JsonSerializableBase
    {
        public List<glTF_VRM_SecondaryAnimationGroup> boneGroups = new List<glTF_VRM_SecondaryAnimationGroup>();
        public List<glTF_VRM_SecondaryAnimationColliderGroup> colliderGroups = new List<glTF_VRM_SecondaryAnimationColliderGroup>();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => boneGroups);
            f.KeyValue(() => colliderGroups);
        }
    }
}
