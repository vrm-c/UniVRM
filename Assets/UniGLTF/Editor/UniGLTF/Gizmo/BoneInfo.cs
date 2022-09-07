using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.Utils;
using UnityEngine;

namespace UniGLTF
{
    public class BoneInfo
    {
        private readonly Transform _head;
        private readonly Transform _tail;
        private readonly Vector3 _headLocalForward;
        private readonly Vector3 _headLocalUp;

        public HumanBodyBones HeadBone { get; private set; }
        public HumanBodyBones TailBone { get; private set; }

        public GameObject HeadObject { get { return _head.gameObject; } }

        public BoneInfo(Transform head, Transform tail, HumanBodyBones headBone, HumanBodyBones tailBone)
        {
            _head = head;
            _tail = tail;
            _headLocalForward = (_head != null && _tail != null) ?
                _head.InverseTransformPoint(_tail.position) :
                new Vector3(0, 0, 0.1f);
            _headLocalUp = CalculateLocalUpVector(_headLocalForward);

            HeadBone = headBone;
            TailBone = tailBone;
        }

        public BoneInfo(Transform head, Vector3 headLocalDirection, HumanBodyBones headBone)
        {
            _head = head;
            _tail = null;
            _headLocalForward = headLocalDirection;
            _headLocalUp = CalculateLocalUpVector(_headLocalForward);

            HeadBone = headBone;
        }

        public override string ToString()
        {
            return $"{_head}";
        }

        public Vector3 GetHeadPosition()
        {
            if (_head == null) return Vector3.zero;

            return _head.position;
        }

        public Vector3 GetTailPosition()
        {
            if (_tail == null)
            {
                return _head.TransformPoint(_headLocalForward);
            }
            else
            {
                return _tail.position;
            }
        }

        public Vector3 GetUpVector()
        {
            if (_head == null) return Vector3.zero;

            return _head.TransformVector(_headLocalUp);
        }

        private static Vector3 CalculateLocalUpVector(Vector3 localForward)
        {
            var dotX = Mathf.Abs(Vector3.Dot(localForward, new Vector3(1, 0, 0)));
            var dotY = Mathf.Abs(Vector3.Dot(localForward, new Vector3(0, 1, 0)));
            var dotZ = Mathf.Abs(Vector3.Dot(localForward, new Vector3(0, 0, 1)));
            if (dotX > dotY && dotX > dotZ)
            {
                return new Vector3(0, 1, 0);
            }
            if (dotY > dotX && dotY > dotZ)
            {
                return new Vector3(0, 0, 1);
            }
            else
            {
                return new Vector3(0, 1, 0);
            }
        }

        #region Humanoid Bone info
        private static readonly Dictionary<HumanBodyBones, Vector3> LeafBoneWithDirection =
                    new Dictionary<HumanBodyBones, Vector3>
                    {
                {HumanBodyBones.Head, Vector3.zero},
                {HumanBodyBones.LeftToes, Vector3.forward},
                {HumanBodyBones.RightToes, Vector3.forward},
                {HumanBodyBones.LeftEye, Vector3.forward},
                {HumanBodyBones.RightEye, Vector3.forward},
                {HumanBodyBones.Jaw, Vector3.forward},
                {HumanBodyBones.LeftThumbDistal,   Vector3.zero},
                {HumanBodyBones.LeftIndexDistal,   Vector3.zero},
                {HumanBodyBones.LeftMiddleDistal,  Vector3.zero},
                {HumanBodyBones.LeftRingDistal,    Vector3.zero},
                {HumanBodyBones.LeftLittleDistal,  Vector3.zero},
                {HumanBodyBones.RightThumbDistal,  Vector3.zero},
                {HumanBodyBones.RightIndexDistal,  Vector3.zero},
                {HumanBodyBones.RightMiddleDistal, Vector3.zero},
                {HumanBodyBones.RightRingDistal,   Vector3.zero},
                {HumanBodyBones.RightLittleDistal, Vector3.zero},
                    };
        static HumanBodyBones[] _NotConnectedBones = new HumanBodyBones[]{
            HumanBodyBones.LeftShoulder ,
            HumanBodyBones.RightShoulder,
            HumanBodyBones.LeftUpperLeg ,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftEye,
            HumanBodyBones.RightEye,
            HumanBodyBones.Jaw,
        };

        public static List<BoneInfo> GetHumanoidBones(Animator _animator)
        {
            List<BoneInfo> _bones = new List<BoneInfo>();
            if (_animator == null || !_animator.isHuman)
            {
                throw new ArgumentException("not humanoid");
            }

            var validBones = CachedEnum.GetValues<HumanBodyBones>()
                .Where(x => x != HumanBodyBones.LastBone)
                .ToArray();
            var headSelectedBones = new HashSet<HumanBodyBones>();
            foreach (var x in validBones)
            {
                var tailTf = _animator.GetBoneTransform(x);
                if (tailTf == null) continue;
                if (_NotConnectedBones.Contains(x))
                {
                    // この組み合わせはHumanoidボーンではないので
                    continue;
                }

                var head = FindHeadBone(_animator, x);
                if (!head.HasValue) continue;

                var headTf = _animator.GetBoneTransform(head.Value);
                if (headTf == null) continue;

                _bones.Add(new BoneInfo(headTf, tailTf, head.Value, x));
                headSelectedBones.Add(head.Value);
            }

            foreach (var kv in LeafBoneWithDirection)
            {
                var head = kv.Key;
                var headTf = _animator.GetBoneTransform(head);
                if (headTf == null) continue;

                var parent = FindHeadBone(_animator, head);
                var parentTf = parent.HasValue ? _animator.GetBoneTransform(parent.Value) : null;

                if (kv.Value == Vector3.zero)
                {
                    if (parentTf == null) continue;

                    var direction = headTf.InverseTransformPoint(parentTf.position);
                    _bones.Add(new BoneInfo(headTf, -direction, parent.Value));
                }
                else
                {
                    var distance = 0.05f;
                    if (parentTf != null)
                    {
                        distance = Vector3.Distance(headTf.position, parentTf.position);
                    }
                    _bones.Add(new BoneInfo(headTf, kv.Value * distance, parent.Value));
                }

            }

            return _bones;
        }

        private static HumanBodyBones? FindHeadBone(Animator _animator, HumanBodyBones tail)
        {
            var tailTransform = _animator.GetBoneTransform(tail);
            if (tailTransform == null) return null;

            HumanBodyBones? headCandidate = tail;
            while (true)
            {
                headCandidate = GetParent(headCandidate.Value);
                if (!headCandidate.HasValue)
                {
                    // Root まで探しきった
                    return null;
                }

                var headCandidateTf = _animator.GetBoneTransform(headCandidate.Value);
                if (headCandidateTf != null)
                {
                    return headCandidate.Value;
                }
            }
        }

        private static HumanBodyBones? GetParent(HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Hips: return null;
                case HumanBodyBones.LeftUpperLeg: return HumanBodyBones.Hips;
                case HumanBodyBones.RightUpperLeg: return HumanBodyBones.Hips;
                case HumanBodyBones.LeftLowerLeg: return HumanBodyBones.LeftUpperLeg;
                case HumanBodyBones.RightLowerLeg: return HumanBodyBones.RightUpperLeg;
                case HumanBodyBones.LeftFoot: return HumanBodyBones.LeftLowerLeg;
                case HumanBodyBones.RightFoot: return HumanBodyBones.RightLowerLeg;
                case HumanBodyBones.Spine: return HumanBodyBones.Hips;
                case HumanBodyBones.Chest: return HumanBodyBones.Spine;
                case HumanBodyBones.UpperChest: return HumanBodyBones.Chest;
                case HumanBodyBones.Neck: return HumanBodyBones.UpperChest;
                case HumanBodyBones.Head: return HumanBodyBones.Neck;
                case HumanBodyBones.LeftShoulder: return HumanBodyBones.UpperChest;
                case HumanBodyBones.RightShoulder: return HumanBodyBones.UpperChest;
                case HumanBodyBones.LeftUpperArm: return HumanBodyBones.LeftShoulder;
                case HumanBodyBones.RightUpperArm: return HumanBodyBones.RightShoulder;
                case HumanBodyBones.LeftLowerArm: return HumanBodyBones.LeftUpperArm;
                case HumanBodyBones.RightLowerArm: return HumanBodyBones.RightUpperArm;
                case HumanBodyBones.LeftHand: return HumanBodyBones.LeftLowerArm;
                case HumanBodyBones.RightHand: return HumanBodyBones.RightLowerArm;
                case HumanBodyBones.LeftToes: return HumanBodyBones.LeftFoot;
                case HumanBodyBones.RightToes: return HumanBodyBones.RightFoot;
                case HumanBodyBones.LeftEye: return HumanBodyBones.Head;
                case HumanBodyBones.RightEye: return HumanBodyBones.Head;
                case HumanBodyBones.Jaw: return HumanBodyBones.Head;
                case HumanBodyBones.LeftThumbProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftThumbIntermediate: return HumanBodyBones.LeftThumbProximal;
                case HumanBodyBones.LeftThumbDistal: return HumanBodyBones.LeftThumbIntermediate;
                case HumanBodyBones.LeftIndexProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftIndexIntermediate: return HumanBodyBones.LeftIndexProximal;
                case HumanBodyBones.LeftIndexDistal: return HumanBodyBones.LeftIndexIntermediate;
                case HumanBodyBones.LeftMiddleProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftMiddleIntermediate: return HumanBodyBones.LeftMiddleProximal;
                case HumanBodyBones.LeftMiddleDistal: return HumanBodyBones.LeftMiddleIntermediate;
                case HumanBodyBones.LeftRingProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftRingIntermediate: return HumanBodyBones.LeftRingProximal;
                case HumanBodyBones.LeftRingDistal: return HumanBodyBones.LeftRingIntermediate;
                case HumanBodyBones.LeftLittleProximal: return HumanBodyBones.LeftHand;
                case HumanBodyBones.LeftLittleIntermediate: return HumanBodyBones.LeftLittleProximal;
                case HumanBodyBones.LeftLittleDistal: return HumanBodyBones.LeftLittleIntermediate;
                case HumanBodyBones.RightThumbProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightThumbIntermediate: return HumanBodyBones.RightThumbProximal;
                case HumanBodyBones.RightThumbDistal: return HumanBodyBones.RightThumbIntermediate;
                case HumanBodyBones.RightIndexProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightIndexIntermediate: return HumanBodyBones.RightIndexProximal;
                case HumanBodyBones.RightIndexDistal: return HumanBodyBones.RightIndexIntermediate;
                case HumanBodyBones.RightMiddleProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightMiddleIntermediate: return HumanBodyBones.RightMiddleProximal;
                case HumanBodyBones.RightMiddleDistal: return HumanBodyBones.RightMiddleIntermediate;
                case HumanBodyBones.RightRingProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightRingIntermediate: return HumanBodyBones.RightRingProximal;
                case HumanBodyBones.RightRingDistal: return HumanBodyBones.RightRingIntermediate;
                case HumanBodyBones.RightLittleProximal: return HumanBodyBones.RightHand;
                case HumanBodyBones.RightLittleIntermediate: return HumanBodyBones.RightLittleProximal;
                case HumanBodyBones.RightLittleDistal: return HumanBodyBones.RightLittleIntermediate;
                case HumanBodyBones.LastBone: return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bone), bone, null);
            }
        }
        #endregion
    }
}