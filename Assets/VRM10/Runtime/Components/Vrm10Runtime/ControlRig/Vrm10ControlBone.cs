using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// このクラスのヒエラルキーが 正規化された TPose を表している。
    /// 同時に、元のヒエラルキーの初期回転を保持する。
    /// Apply 関数で、再帰的に正規化済みのローカル回転から初期回転を加味したローカル回転を作って適用する。
    /// </summary>
    public class Vrm10ControlBone
    {
        public readonly HumanBodyBones Bone;

        /// <summary>
        /// 元のヒエラルキーの対応ボーン
        /// </summary>
        public readonly Transform Target;

        /// <summary>
        /// 回転と拡大縮小を除去した(正規化された)ボーン。
        /// このボーンに対して localRotation を代入する。
        /// </summary>
        public readonly Transform Normalized;

        /// <summary>
        /// 元のボーンの初期回転。
        /// </summary>
        public readonly Quaternion InitialLocalRotation;

        public readonly Quaternion ToLocal;

        public List<Vrm10ControlBone> Children = new List<Vrm10ControlBone>();

        public Vrm10ControlBone(Transform current, HumanBodyBones bone)
        {
            if (bone == HumanBodyBones.LastBone)
            {
                throw new ArgumentNullException();
            }
            if (current == null)
            {
                throw new ArgumentNullException();
            }
            Bone = bone;
            Target = current;
            Normalized = new GameObject(bone.ToString()).transform;
            Normalized.position = current.position;
            // InitialLocalRotation = parentInverse * current.rotation;
            InitialLocalRotation = current.localRotation;
            // InitialLocalRotation = current.rotation;
            ToLocal = current.rotation;
        }

        public static Vrm10ControlBone Build(UniHumanoid.Humanoid humanoid, Dictionary<HumanBodyBones, Vrm10ControlBone> boneMap)
        {
            var hips = new Vrm10ControlBone(humanoid.Hips, HumanBodyBones.Hips);
            boneMap.Add(HumanBodyBones.Hips, hips);

            foreach (Transform child in humanoid.Hips)
            {
                Traverse(humanoid, child, hips, boneMap);
            }

            return hips;
        }

        private static void Traverse(UniHumanoid.Humanoid humanoid, Transform current, Vrm10ControlBone parent, Dictionary<HumanBodyBones, Vrm10ControlBone> boneMap)
        {
            if (humanoid.TryGetBoneForTransform(current, out var bone))
            {

                // ヒューマンボーンだけを対象にするので、
                // parent が current の直接の親でない場合がある。
                // ワールド回転 parent^-1 * current からローカル回転を算出する。
                var parentInverse = Quaternion.Inverse(parent.Target.rotation);

                var newBone = new Vrm10ControlBone(current, bone);
                newBone.Normalized.SetParent(parent.Normalized, true);
                parent.Children.Add(newBone);
                parent = newBone;
                boneMap.Add(bone, newBone);
            }

            foreach (Transform child in current)
            {
                Traverse(humanoid, child, parent, boneMap);
            }
        }

        /// <summary>
        /// 親から再帰的にNormalized の ローカル回転を初期回転を加味して Target に適用する。
        /// </summary>
        public void ApplyRecursively()
        {
            Target.localRotation = InitialLocalRotation * Quaternion.Inverse(ToLocal) * Normalized.localRotation * ToLocal;
            foreach (var child in Children)
            {
                child.ApplyRecursively();
            }
        }
    }
}
