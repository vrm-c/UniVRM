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
        [JsonSchema(Description = "The local coordinate from the collision detection node.")]
        public Vector3 offset;

        [JsonSchema(Description = "The radius of the sphere (specified body part) used for collision detection with swaying objects.")]
        public float radius;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => offset);
            f.KeyValue(() => radius);
        }
    }


    [Serializable]
    [JsonSchema(Title = "vrm.secondaryanimation.collidergroup", Description = "Set sphere balls (specified body parts) to collide with swaying objects.")]
    public class glTF_VRM_SecondaryAnimationColliderGroup : JsonSerializableBase
    {
        [JsonSchema(Description = "Where the collision detection is set up.")]
        public int node;

        public List<glTF_VRM_SecondaryAnimationCollider> colliders = new List<glTF_VRM_SecondaryAnimationCollider>();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => node);
            f.KeyValue(() => colliders);
        }
    }


    [Serializable]
    [JsonSchema(Title = "vrm.secondaryanimation.spring")]
    public class glTF_VRM_SecondaryAnimationGroup : JsonSerializableBase
    {
        [JsonSchema(Description = "Annotation comment")]
        public string comment;

        [JsonSchema(Description = "The resilience of the swaying object (the power of returning to the initial pose).")]
        public float stiffiness;

        [JsonSchema(Description = "The strength of gravity.")]
        public float gravityPower;

        [JsonSchema(Description = "The direction of gravity. Gravity can be enabled by (0, -1, 0) and setting (1, 0, 0) will act like wind.")]
        public Vector3 gravityDir;

        [JsonSchema(Description = "The resistance (deceleration) of automatic animation.")]
        public float dragForce;

        [JsonSchema(Description = @"The reference point of a swaying object can be set at any location except the origin. When implementing UI moving with warp, the parent node to be moved with warp can be specified if you don’t want to make the object moving by warp movement.")]
        public int center;

        [JsonSchema(Description = "The radius of the sphere (swaying object) used for collision detection with colliders (specified body parts).")]
        public float hitRadius;

        [JsonSchema(Description = "Specify the node index of the root bone of the swaying object.")]
        public int[] bones = new int[] { };

        [JsonSchema(Description = "Specify the index of the collision detection group for the swaying object.")]
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
    [JsonSchema(Title = "vrm.secondaryanimation", Description = "Automatic animation setting for string-shaped objects such as tail and hair.")]
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
