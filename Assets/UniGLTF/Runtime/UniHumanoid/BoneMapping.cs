using UnityEngine;
using System.Linq;
using UniGLTF.Utils;
using UniGLTF;

namespace UniHumanoid
{
    public class BoneMapping : MonoBehaviour
    {
        [SerializeField]
        public GameObject[] Bones = new GameObject[(int)HumanBodyBones.LastBone];

        [SerializeField]
        public AvatarDescription Description;

        private void Reset()
        {
            GetBones();
        }

        private void GetBones()
        {
            Bones = new GameObject[(int)HumanBodyBones.LastBone];

            if (TryGetComponent<Animator>(out var animator))
            {
                if (animator.avatar != null)
                {
                    foreach (var key in CachedEnum.GetValues<HumanBodyBones>())
                    {
                        if (key == HumanBodyBones.LastBone)
                        {
                            break;
                        }
                        var transform = animator.GetBoneTransform(key);
                        if (transform != null)
                        {
                            Bones[(int)key] = transform.gameObject;
                        }
                    }
                }
            }
        }

        public void GuessBoneMapping()
        {
            var hips = Bones[(int)HumanBodyBones.Hips];
            if (hips == null)
            {
                UniGLTFLogger.Warning("require hips");
                return;
            }

            var estimator = new BvhSkeletonEstimator();
            var skeleton = estimator.Detect(hips.transform);
            var bones = hips.transform.Traverse().ToArray();
            for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i)
            {
                var index = skeleton.GetBoneIndex((HumanBodyBones)i);
                if (index >= 0)
                {
                    Bones[i] = bones[index].gameObject;
                }
            }
        }

        public void EnsureTPose()
        {
            var map = Bones
                .Select((x, i) => new { i, x })
                .Where(x => x.x != null)
                .ToDictionary(x => (HumanBodyBones)x.i, x => x.x.transform)
                ;
            {
                var left = (map[HumanBodyBones.LeftLowerArm].position - map[HumanBodyBones.LeftUpperArm].position).normalized;
                map[HumanBodyBones.LeftUpperArm].rotation = Quaternion.FromToRotation(left, Vector3.left) * map[HumanBodyBones.LeftUpperArm].rotation;
            }
            {
                var right = (map[HumanBodyBones.RightLowerArm].position - map[HumanBodyBones.RightUpperArm].position).normalized;
                map[HumanBodyBones.RightUpperArm].rotation = Quaternion.FromToRotation(right, Vector3.right) * map[HumanBodyBones.RightUpperArm].rotation;
            }
        }

        public static void SetBonesToDescription(BoneMapping mapping, AvatarDescription description)
        {
            var map = mapping.Bones
                .Select((x, i) => new { i, x })
                .Where(x => x.x != null)
                .ToDictionary(x => (HumanBodyBones)x.i, x => x.x.transform)
                ;
            description.SetHumanBones(map);
        }

        private void Awake()
        {
            if (Bones == null
                || Bones.All(x => x == null))
            {
                GetBones();
            }
        }
    }
}
