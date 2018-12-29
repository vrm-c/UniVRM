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
        [JsonSchema(Description = "The local coordinate from the node of the collider group.")]
        public Vector3 offset;

        [JsonSchema(Description = "The radius of the collider.")]
        public float radius;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => offset);
            f.KeyValue(() => radius);
        }
    }


    [Serializable]
    [JsonSchema(Title = "vrm.secondaryanimation.collidergroup", Description = @"Set sphere balls for colliders used for collision detections with swaying objects.")]
    public class glTF_VRM_SecondaryAnimationColliderGroup : JsonSerializableBase
    {
        [JsonSchema(Description = "The node of the collider group for setting up collision detections.")]
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

        [JsonSchema(Description = "The direction of gravity. Set (0, -1, 0) for simulating the gravity. Set (1, 0, 0) for simulating the wind.")]
        public Vector3 gravityDir;

        [JsonSchema(Description = "The resistance (deceleration) of automatic animation.")]
        public float dragForce;

        [JsonSchema(Description = @"The reference point of a swaying object can be set at any location except the origin. When implementing UI moving with warp, the parent node to move with warp can be specified if you don't want to make the object swaying with warp movement.")]
        public int center;

        [JsonSchema(Description = "The radius of the sphere used for the collision detection with colliders.")]
        public float hitRadius;

        [JsonSchema(Description = "Specify the node index of the root bone of the swaying object.")]
        public int[] bones = new int[] { };

        [JsonSchema(Description = "Specify the index of the collider group for collisions with swaying objects.")]
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
    [JsonSchema(Title = "vrm.secondaryanimation", Description = "The setting of automatic animation of string-like objects such as tails and hairs.")]
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
