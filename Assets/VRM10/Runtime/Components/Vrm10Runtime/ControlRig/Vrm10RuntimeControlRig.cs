using System;
using System.Collections.Generic;
using System.Linq;
using UniHumanoid;
using UnityEngine;
using UniGLTF.Utils;

namespace UniVRM10
{
    /// <summary>
    /// VRM 1.0 モデルインスタンスに対して、コントロールリグを生成します。
    /// これは VRM 0.x のような、正規化されたボーン操作を提供します。
    ///
    /// Create a control rig for the VRM 1.0 model instance.
    /// This provides the normalized operation of bones, like VRM 0.x.
    /// </summary>
    public sealed class Vrm10RuntimeControlRig : IDisposable, INormalizedPoseApplicable, ITPoseProvider
    {
        private readonly Transform _controlRigRoot;
        private readonly Vrm10ControlBone _hipBone;
        private readonly Dictionary<HumanBodyBones, Vrm10ControlBone> _bones;
        private readonly Avatar _controlRigAvatar;

        public IReadOnlyDictionary<HumanBodyBones, Vrm10ControlBone> Bones => _bones;
        public Animator ControlRigAnimator { get; }

        /// <summary>
        /// humanoid に対して ControlRig を生成します
        /// </summary>
        /// <param name="humanoid">T-Pose である必要があります</param>
        /// <param name="vrmRoot"></param>
        public Vrm10RuntimeControlRig(UniHumanoid.Humanoid humanoid, Transform vrmRoot)
        {
            _controlRigRoot = new GameObject("Runtime Control Rig").transform;
            _controlRigRoot.SetParent(vrmRoot);

            _hipBone = Vrm10ControlBone.Build(humanoid, out _bones, _controlRigRoot);
            _hipBone.ControlBone.SetParent(_controlRigRoot);

            var transformBonePairs = _bones.Select(kv => (kv.Value.ControlBone, kv.Key));
            _controlRigAvatar = HumanoidLoader.LoadHumanoidAvatar(vrmRoot, transformBonePairs);
            _controlRigAvatar.name = "Runtime Control Rig";

            if (vrmRoot.TryGetComponent<Animator>(out var animator))
            {
                ControlRigAnimator = animator;
                ControlRigAnimator.avatar = _controlRigAvatar;
            }
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_controlRigAvatar);
            UnityEngine.Object.Destroy(_controlRigRoot);
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

        void INormalizedPoseApplicable.SetRawHipsPosition(Vector3 position)
        {
            var world = _controlRigRoot.TransformPoint(position);
            _hipBone.ControlBone.position = world;
        }

        void INormalizedPoseApplicable.SetNormalizedLocalRotation(HumanBodyBones bone, Quaternion normalizedLocalRotation)
        {
            if (_bones.TryGetValue(bone, out var t))
            {
                t.ControlBone.localRotation = normalizedLocalRotation;
            }
        }

        EuclideanTransform? ITPoseProvider.GetWorldTransform(HumanBodyBones bone)
        {
            if (_bones.TryGetValue(bone, out var t))
            {
                return new EuclideanTransform(t.InitialTargetGlobalPosition);
            }
            else
            {
                return default;
            }
        }
    }
}
