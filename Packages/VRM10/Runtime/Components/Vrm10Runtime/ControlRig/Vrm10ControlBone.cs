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

        private readonly Quaternion _initialTargetLocalRotation;
        private readonly Quaternion _initialTargetGlobalRotation;
        public Vector3 InitialTargetGlobalPosition { get; }
        private readonly List<Vrm10ControlBone> _children = new List<Vrm10ControlBone>();
        public IReadOnlyList<Vrm10ControlBone> Children => _children;

        private Vrm10ControlBone(Transform controlTarget, HumanBodyBones boneType, Vrm10ControlBone parent, Transform root)
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

            // 回転とスケールが除去されたTPoseを構築
            // NOTE: bone name must be unique in the vrm instance.
            ControlBone = new GameObject($"{nameof(Vrm10ControlBone)}:{boneType.ToString()}").transform;
            ControlBone.position = controlTarget.position;
            if (parent != null)
            {
                ControlBone.SetParent(parent.ControlBone, true);
                parent._children.Add(this);
            }

            _initialTargetLocalRotation = controlTarget.localRotation;
            _initialTargetGlobalRotation = controlTarget.rotation;
            InitialTargetGlobalPosition = root.worldToLocalMatrix.MultiplyPoint(controlTarget.position);
        }

        /// <summary>
        /// 親から再帰的にNormalized の ローカル回転を初期回転を加味して Target に適用する。
        /// </summary>
        internal void ProcessRecursively()
        {
            ControlTarget.localRotation = _initialTargetLocalRotation * (Quaternion.Inverse(_initialTargetGlobalRotation) * ControlBone.localRotation * _initialTargetGlobalRotation);
            foreach (var child in _children)
            {
                child.ProcessRecursively();
            }
        }

        public static Vrm10ControlBone Build(UniHumanoid.Humanoid humanoid, out Dictionary<HumanBodyBones, Vrm10ControlBone> boneMap, Transform root)
        {
            var hips = new Vrm10ControlBone(humanoid.Hips, HumanBodyBones.Hips, null, root);
            boneMap = new Dictionary<HumanBodyBones, Vrm10ControlBone>();
            boneMap.Add(HumanBodyBones.Hips, hips);

            foreach (Transform child in humanoid.Hips)
            {
                BuildRecursively(humanoid, child, hips, boneMap, root);
            }

            return hips;
        }

        private static void BuildRecursively(UniHumanoid.Humanoid humanoid, Transform current, Vrm10ControlBone parent, Dictionary<HumanBodyBones, Vrm10ControlBone> boneMap, Transform root)
        {
            if (humanoid.TryGetBoneForTransform(current, out var bone))
            {
                var newBone = new Vrm10ControlBone(current, bone, parent, root);
                parent = newBone;
                boneMap.Add(bone, newBone);
            }

            foreach (Transform child in current)
            {
                BuildRecursively(humanoid, child, parent, boneMap, root);
            }
        }
    }
}
