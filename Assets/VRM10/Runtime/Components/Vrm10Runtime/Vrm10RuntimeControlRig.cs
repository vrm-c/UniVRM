using System;
using System.Collections.Generic;
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
    public sealed class Vrm10RuntimeControlRig : IDisposable
    {
        private readonly Transform _controlRigRoot;
        private readonly Vrm10ControlBone _hipBone;
        private readonly Dictionary<HumanBodyBones, Vrm10ControlBone> _bones;

        public float InitialHipsHeight { get; }

        /// <summary>
        /// コンストラクタ。
        /// humanoid は VRM T-Pose でなければならない。
        /// </summary>
        public Vrm10RuntimeControlRig(UniHumanoid.Humanoid humanoid, Transform vrmRoot)
        {
            _controlRigRoot = new GameObject("Runtime Control Rig").transform;
            _controlRigRoot.SetParent(vrmRoot);

            _hipBone = Vrm10ControlBone.Build(humanoid, out _bones);
            _hipBone.ControlBone.SetParent(_controlRigRoot);

            InitialHipsHeight = _hipBone.ControlTarget.position.y;
        }

        public void Dispose()
        {
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

        public void EnforceTPose()
        {
            foreach (var bone in _bones.Values)
            {
                bone.ControlBone.localPosition = bone.InitialControlBoneLocalPosition;
                bone.ControlBone.localRotation = bone.InitialControlBoneLocalRotation;
            }
        }
    }
}
