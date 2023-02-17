using System;
using System.Collections.Generic;
using System.Linq;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// VRM 1.0 モデルインスタンスに対して、コントロールリグを生成します。
    /// これは VRM 0.x のような、正規化されたボーン操作を提供します。
    ///
    /// Create a control rig for the VRM 1.0 model instance.
    /// This provides the normalized operation of bones, like VRM 0.x.
    /// </summary>
    public sealed class Vrm10RuntimeControlRig : IDisposable, IControlRigSetter
    {
        private readonly Transform _controlRigRoot;
        private readonly Vrm10ControlBoneBind _hipBone;
        private readonly Dictionary<HumanBodyBones, Vrm10ControlBoneBind> _bones;
        private readonly Avatar _controlRigAvatar;

        public IReadOnlyDictionary<HumanBodyBones, Vrm10ControlBoneBind> Bones => _bones;
        public Animator ControlRigAnimator { get; }
        public float InitialHipsHeight { get; }

        /// <summary>
        /// humanoid に対して ControlRig を生成します
        /// </summary>
        /// <param name="humanoid">T-Pose である必要があります</param>
        /// <param name="vrmRoot"></param>
        public Vrm10RuntimeControlRig(UniHumanoid.Humanoid humanoid, Transform vrmRoot)
        {
            _controlRigRoot = new GameObject("Runtime Control Rig").transform;
            _controlRigRoot.SetParent(vrmRoot);

            _hipBone = Vrm10ControlBoneBind.Build(humanoid, out _bones);
            _hipBone.ControlBone.SetParent(_controlRigRoot);

            InitialHipsHeight = _hipBone.ControlTarget.position.y;

            var transformBonePairs = _bones.Select(kv => (kv.Value.ControlBone, kv.Key));
            _controlRigAvatar = HumanoidLoader.LoadHumanoidAvatar(vrmRoot, transformBonePairs);
            _controlRigAvatar.name = "Runtime Control Rig";

            ControlRigAnimator = vrmRoot.GetComponent<Animator>();
            ControlRigAnimator.avatar = _controlRigAvatar;
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_controlRigAvatar);
            UnityEngine.Object.Destroy(_controlRigRoot);
        }

        public bool TryGetRigBone(HumanBodyBones key, out Vrm10ControlBoneBind bone)
        {
            return _bones.TryGetValue(key, out bone);
        }

        internal void Process()
        {
            _hipBone.ControlTarget.position = _hipBone.ControlBone.position;
            _hipBone.ProcessRecursively();
        }

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            if (_bones.TryGetValue(bone, out var value))
            {
                return value.ControlBone;
            }
            else
            {
                return null;
            }
        }

        IEnumerable<(HumanBodyBones Head, HumanBodyBones Parent)> Traverse(Vrm10ControlBoneBind bone, Vrm10ControlBoneBind parent = null)
        {
            yield return (bone.BoneType, parent != null ? parent.BoneType : HumanBodyBones.LastBone);

            foreach (var child in bone.Children)
            {
                foreach (var headParent in Traverse(child, bone))
                {
                    yield return headParent;
                }
            }
        }

        public IEnumerable<(HumanBodyBones Head, HumanBodyBones Parent)> EnumerateBones()
        {
            foreach (var headParent in Traverse(_hipBone))
            {
                yield return headParent;
            }
        }

        public void SetNormalizedLocalRotation(HumanBodyBones bone, Quaternion normalizedLocalRotation)
        {
            if (TryGetRigBone(bone, out var t))
            {
                t.ControlBone.localRotation = normalizedLocalRotation;
            }
        }

        public void SetRootPosition(Vector3 position)
        {
            // TODO: position scaling. ? * InitialHipsHeight;
            _hipBone.ControlBone.localPosition = position;
        }

        public void EnforceTPose()
        {
            foreach (var bone in _bones.Values)
            {
                bone.ControlBone.localRotation = Quaternion.identity;
            }
        }
    }
}
