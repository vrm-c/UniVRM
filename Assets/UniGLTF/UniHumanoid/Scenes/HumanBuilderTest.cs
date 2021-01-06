using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniHumanoid
{
    [RequireComponent(typeof(Animator))]
    public class HumanBuilderTest : MonoBehaviour
    {
        [SerializeField]
        Material m_material;

        class SkeletonBuilder
        {
            Dictionary<HumanBodyBones, Transform> m_skeleton = new Dictionary<HumanBodyBones, Transform>();
            public IDictionary<HumanBodyBones, Transform> Skeleton
            {
                get { return m_skeleton; }
            }

            Dictionary<HumanBodyBones, Vector3> m_boneTail = new Dictionary<HumanBodyBones, Vector3>();
            Transform m_root;

            public SkeletonBuilder(Transform root)
            {
                m_root = root;
            }

            void Add(HumanBodyBones key, Transform parent, Vector3 headPosition, Vector3 tailPosition)
            {
                var bone = new GameObject(key.ToString()).transform;
                bone.SetParent(parent, false);
                bone.localPosition = headPosition;
                m_skeleton[key] = bone;
                m_boneTail[key] = tailPosition;
            }

            void Add(HumanBodyBones key, HumanBodyBones parentKey, Vector3 tailPosition)
            {
                Add(key, m_skeleton[parentKey], m_boneTail[parentKey], tailPosition);
            }

            #region Spine
            public void AddHips(float height, float len)
            {
                Add(HumanBodyBones.Hips, m_root, new Vector3(0, height, 0), new Vector3(0, len, 0));
            }

            public void AddSpine(float len)
            {
                Add(HumanBodyBones.Spine, HumanBodyBones.Hips, new Vector3(0, len, 0));
            }

            public void AddChest(float len)
            {
                Add(HumanBodyBones.Chest, HumanBodyBones.Spine, new Vector3(0, len, 0));
            }

            public void AddNeck(float len)
            {
                Add(HumanBodyBones.Neck, HumanBodyBones.Chest, new Vector3(0, len, 0));
            }

            public void AddHead(float len)
            {
                Add(HumanBodyBones.Head, HumanBodyBones.Neck, new Vector3(0, len, 0));
            }
            #endregion

            public void AddArm(float shoulder, float upper, float lower, float hand)
            {
                Add(HumanBodyBones.LeftShoulder, HumanBodyBones.Chest, new Vector3(-shoulder, 0, 0));
                Add(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder, new Vector3(-upper, 0, 0));
                Add(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, new Vector3(-lower, 0, 0));
                Add(HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, new Vector3(-hand, 0, 0));

                Add(HumanBodyBones.RightShoulder, HumanBodyBones.Chest, new Vector3(shoulder, 0, 0));
                Add(HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder, new Vector3(upper, 0, 0));
                Add(HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, new Vector3(lower, 0, 0));
                Add(HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, new Vector3(hand, 0, 0));
            }

            public void AddLeg(float distance, float upper, float lower, float foot, float toe)
            {
                Add(HumanBodyBones.LeftUpperLeg, m_skeleton[HumanBodyBones.Hips], new Vector3(-distance, 0, 0), new Vector3(0, -upper, 0));
                Add(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, new Vector3(0, -lower, 0));
                Add(HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg, new Vector3(0, -foot, foot));
                Add(HumanBodyBones.LeftToes, HumanBodyBones.LeftFoot, new Vector3(0, 0, toe));

                Add(HumanBodyBones.RightUpperLeg, m_skeleton[HumanBodyBones.Hips], new Vector3(distance, 0, 0), new Vector3(0, -upper, 0));
                Add(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg, new Vector3(0, -lower, 0));
                Add(HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg, new Vector3(0, -foot, foot));
                Add(HumanBodyBones.RightToes, HumanBodyBones.RightFoot, new Vector3(0, 0, toe));
            }
        }

        void OnEnable()
        {
            BuildSkeleton(transform);
        }

        private void BuildSkeleton(Transform root)
        {
            var position = root.position;
            root.position = Vector3.zero;

            try
            {
                // hips -> spine -> chest
                var builder = new SkeletonBuilder(root);
                builder.AddHips(0.8f, 0.2f);
                builder.AddSpine(0.1f);
                builder.AddChest(0.2f);
                builder.AddNeck(0.1f);
                builder.AddHead(0.2f);
                builder.AddArm(0.1f, 0.3f, 0.3f, 0.1f);
                builder.AddLeg(0.1f, 0.3f, 0.4f, 0.1f, 0.1f);

                var description = AvatarDescription.Create(builder.Skeleton);
                var animator = GetComponent<Animator>();
                animator.avatar = description.CreateAvatar(root);

                // create SkinnedMesh for bone visualize
                var renderer = SkeletonMeshUtility.CreateRenderer(animator);

                if (m_material == null)
                {
                    m_material = new Material(Shader.Find("Standard"));
                }
                renderer.sharedMaterial = m_material;
                //root.gameObject.AddComponent<BoneMapping>();

                var transfer = GetComponent<HumanPoseTransfer>();
                if (transfer != null)
                {
                    transfer.Avatar = animator.avatar;
                    transfer.Setup();
                }
            }
            finally
            {
                // restore position
                root.position = position;
            }
        }
    }
}
