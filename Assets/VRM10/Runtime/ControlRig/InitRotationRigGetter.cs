using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 初期回転を持っているヒエラルキーから、NormalizedLocalRotation(VRM0互換)を取り出す
    /// </summary>
    public class InitRotationGetter : IControlRigGetter
    {
        Transform m_root;
        private readonly Dictionary<HumanBodyBones, Vrm10ControlBoneBind> _bones;

        /// <param name="tpose">TPoseのヒエラルキー</param>
        public InitRotationGetter(Transform root, UniHumanoid.Humanoid humanoid)
        {
            m_root = root;

            var _controlRigRoot = new GameObject("Runtime Control Rig").transform;
            _controlRigRoot.SetParent(m_root);

            var controlRigInitialRotations = humanoid.BoneMap.ToDictionary(tb => tb.Item2, tb => tb.Item1.rotation);

            var _hipBone = Vrm10ControlBoneBind.Build(humanoid, controlRigInitialRotations, out _bones);
            _hipBone.ControlBone.Transform.SetParent(_controlRigRoot);
        }

        public Quaternion GetNormalizedLocalRotation(HumanBodyBones bone, HumanBodyBones parentBone)
        {
            if (_bones.TryGetValue(bone, out var boneBind))
            {
                // TODO: 逆コピー
                boneBind.ControlBone.Transform.localRotation = boneBind.ControlTarget.localRotation;
                if (bone == HumanBodyBones.Hips)
                {
                    boneBind.ControlBone.Transform.localPosition = boneBind.ControlTarget.localPosition;
                }

                return boneBind.ControlBone.NormalizedLocalRotation;
            }
            else
            {
                // Debug.LogWarning($"${bone} not found");
                return Quaternion.identity;
            }
        }

        public Vector3 GetRootPosition()
        {
            // TODO: from m_root relative ? scaling ?
            return _bones[HumanBodyBones.Hips].ControlBone.Transform.localPosition;
        }
    }
}
