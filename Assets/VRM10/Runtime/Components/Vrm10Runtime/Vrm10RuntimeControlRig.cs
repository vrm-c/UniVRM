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
    public sealed class Vrm10RuntimeControlRig
    {
        private readonly Vrm10ControlBone _rootBone;
        private readonly Dictionary<HumanBodyBones, Vrm10ControlBone> _bones;

        public float InitialHipsHeight { get; }

        /// <summary>
        /// コンストラクタ。
        /// humanoid は VRM T-Pose でなければならない。
        /// </summary>
        public Vrm10RuntimeControlRig(UniHumanoid.Humanoid humanoid)
        {
            _rootBone = Vrm10ControlBone.Build(humanoid, out _bones);
            InitialHipsHeight = _rootBone.ControlTarget.position.y;
        }

        internal void Process()
        {
            _rootBone.ControlTarget.position = _rootBone.ControlBone.position;
            _rootBone.ProcessRecursively();
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
