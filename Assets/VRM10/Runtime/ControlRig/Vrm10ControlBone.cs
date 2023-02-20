using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// The control bone of the control rig.
    ///
    /// このクラスのヒエラルキーが 正規化された TPose を表している。
    /// 同時に、元のヒエラルキーの初期回転を保持する。
    /// Apply 関数で、再帰的に正規化済みのローカル回転から初期回転を加味したローカル回転を作って適用する。
    /// </summary>
    public sealed class Vrm10ControlBone
    {
        /// <summary>
        /// このコントロールボーンに紐づくボーンの種類。
        /// </summary>
        public HumanBodyBones BoneType { get; }

        /// <summary>
        /// コントロール対象のボーン Transform。
        /// </summary>
        public Transform ControlTarget { get; }

        /// <summary>
        /// コントロールボーンの Transform。
        ///
        /// VRM の T-Pose 姿勢をしているときに、回転とスケールが初期値になっている（正規化）。
        /// このボーンに対して localRotation を代入し、コントロールを行う。
        /// </summary>
        public Transform ControlBone { get; }

        /// <summary>
        /// コントロールボーンの初期ローカル位置。
        /// </summary>
        public Vector3 InitialControlBoneLocalPosition { get; }

        /// <summary>
        /// コントロールボーンの初期ローカル回転。
        /// </summary>
        public Quaternion InitialControlBoneLocalRotation { get; }

        private readonly Quaternion _initialControlBoneGlobalRotation;
        private readonly Quaternion _initialTargetLocalRotation;
        private readonly Quaternion _initialTargetGlobalRotation;
        private readonly List<Vrm10ControlBone> _children = new List<Vrm10ControlBone>();

        private Vrm10ControlBone(Transform controlTarget, HumanBodyBones boneType, Vrm10ControlBone parent, IReadOnlyDictionary<HumanBodyBones, Quaternion> controlRigInitialRotations)
        {
            if (boneType == HumanBodyBones.LastBone)
            {
                throw new ArgumentNullException();
            }
            if (controlTarget == null)
            {
                throw new ArgumentNullException();
            }

            BoneType = boneType;
            ControlTarget = controlTarget;
            // NOTE: bone name must be unique in the vrm instance.
            ControlBone = new GameObject($"{nameof(Vrm10ControlBone)}:{boneType.ToString()}").transform;
            if (controlRigInitialRotations != null)
            {
                if (controlRigInitialRotations.TryGetValue(boneType, out var rotation))
                {
                    ControlBone.rotation = rotation;
                }
            }
            ControlBone.position = controlTarget.position;

            if (parent != null)
            {
                ControlBone.SetParent(parent.ControlBone, true);
                parent._children.Add(this);
            }

            InitialControlBoneLocalPosition = ControlBone.localPosition;
            InitialControlBoneLocalRotation = ControlBone.localRotation;
            _initialControlBoneGlobalRotation = ControlBone.rotation;
            _initialTargetLocalRotation = controlTarget.localRotation;
            _initialTargetGlobalRotation = controlTarget.rotation;
        }

        /// <summary>
        /// 初期姿勢からの相対的な回転。
        /// 
        /// VRM-0.X 互換リグでは localRotation と同じ値を示す。
        /// </summary>
        Quaternion NormalizedLocalRotation
        {
            get
            {
                return _initialControlBoneGlobalRotation * Quaternion.Inverse(InitialControlBoneLocalRotation) * ControlBone.localRotation * Quaternion.Inverse(_initialControlBoneGlobalRotation);
            }
        }

        /// <summary>
        /// 親から再帰的にNormalized の ローカル回転を初期回転を加味して Target に適用する。
        /// </summary>
        internal void ProcessRecursively()
        {
            ControlTarget.localRotation = _initialTargetLocalRotation * (Quaternion.Inverse(_initialTargetGlobalRotation) * NormalizedLocalRotation * _initialTargetGlobalRotation);
            foreach (var child in _children)
            {
                child.ProcessRecursively();
            }
        }

        public static Vrm10ControlBone Build(UniHumanoid.Humanoid humanoid, IReadOnlyDictionary<HumanBodyBones, Quaternion> controlRigInitialRotations, out Dictionary<HumanBodyBones, Vrm10ControlBone> boneMap)
        {
            var hips = new Vrm10ControlBone(humanoid.Hips, HumanBodyBones.Hips, null, controlRigInitialRotations);
            boneMap = new Dictionary<HumanBodyBones, Vrm10ControlBone>();
            boneMap.Add(HumanBodyBones.Hips, hips);

            foreach (Transform child in humanoid.Hips)
            {
                BuildRecursively(humanoid, child, hips, controlRigInitialRotations, boneMap);
            }

            return hips;
        }

        private static void BuildRecursively(UniHumanoid.Humanoid humanoid, Transform current, Vrm10ControlBone parent, IReadOnlyDictionary<HumanBodyBones, Quaternion> controlRigInitialRotations, Dictionary<HumanBodyBones, Vrm10ControlBone> boneMap)
        {
            if (humanoid.TryGetBoneForTransform(current, out var bone))
            {
                var newBone = new Vrm10ControlBone(current, bone, parent, controlRigInitialRotations);
                parent = newBone;
                boneMap.Add(bone, newBone);
            }

            foreach (Transform child in current)
            {
                BuildRecursively(humanoid, child, parent, controlRigInitialRotations, boneMap);
            }
        }
    }
}
