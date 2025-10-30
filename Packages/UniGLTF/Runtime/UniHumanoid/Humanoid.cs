using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.Utils;
using UnityEngine;

namespace UniHumanoid
{
    /// <summary>
    /// Bone割り当てを保持する。
    /// ヒエラルキーのルートにアタッチする。
    /// root は以下の条件を満たすべし。
    ///   * root は 原点に配置、回転なし、スケールなし。
    ///   * root は Hips の祖先
    ///   * root の親は null
    /// </summary>
    [DisallowMultipleComponent]
    public class Humanoid : MonoBehaviour
    {
        [SerializeField] private Transform m_Hips; public Transform Hips => m_Hips;

        #region leg
        [SerializeField] private Transform m_LeftUpperLeg; public Transform LeftUpperLeg => m_LeftUpperLeg;
        [SerializeField] private Transform m_RightUpperLeg; public Transform RightUpperLeg => m_RightUpperLeg;
        [SerializeField] private Transform m_LeftLowerLeg; public Transform LeftLowerLeg => m_LeftLowerLeg;
        [SerializeField] private Transform m_RightLowerLeg; public Transform RightLowerLeg => m_RightLowerLeg;
        [SerializeField] private Transform m_LeftFoot; public Transform LeftFoot => m_LeftFoot;
        [SerializeField] private Transform m_RightFoot; public Transform RightFoot => m_RightFoot;
        [SerializeField] private Transform m_LeftToes; public Transform LeftToes => m_LeftToes;
        [SerializeField] private Transform m_RightToes; public Transform RightToes => m_RightToes;
        #endregion

        #region spine
        [SerializeField] private Transform m_Spine; public Transform Spine => m_Spine;
        [SerializeField] private Transform m_Chest; public Transform Chest => m_Chest;
        [SerializeField] private Transform m_UpperChest; public Transform UpperChest => m_UpperChest;
        [SerializeField] private Transform m_Neck; public Transform Neck => m_Neck;
        [SerializeField] private Transform m_Head; public Transform Head => m_Head;
        [SerializeField] private Transform m_LeftEye; public Transform LeftEye => m_LeftEye;
        [SerializeField] private Transform m_RightEye; public Transform RightEye => m_RightEye;
        [SerializeField] private Transform m_Jaw; public Transform Jaw => m_Jaw;
        #endregion

        #region arm
        [SerializeField] private Transform m_LeftShoulder; public Transform LeftShoulder => m_LeftShoulder;
        [SerializeField] private Transform m_RightShoulder; public Transform RightShoulder => m_RightShoulder;
        [SerializeField] private Transform m_LeftUpperArm; public Transform LeftUpperArm => m_LeftUpperArm;
        [SerializeField] private Transform m_RightUpperArm; public Transform RightUpperArm => m_RightUpperArm;
        [SerializeField] private Transform m_LeftLowerArm; public Transform LeftLowerArm => m_LeftLowerArm;
        [SerializeField] private Transform m_RightLowerArm; public Transform RightLowerArm => m_RightLowerArm;
        [SerializeField] private Transform m_LeftHand; public Transform LeftHand => m_LeftHand;
        [SerializeField] private Transform m_RightHand; public Transform RightHand => m_RightHand;
        #endregion

        #region fingers
        [SerializeField] private Transform m_LeftThumbProximal; public Transform LeftThumbProximal => m_LeftThumbProximal;
        [SerializeField] private Transform m_LeftThumbIntermediate; public Transform LeftThumbIntermediate => m_LeftThumbIntermediate;
        [SerializeField] private Transform m_LeftThumbDistal; public Transform LeftThumbDistal => m_LeftThumbDistal;
        [SerializeField] private Transform m_LeftIndexProximal; public Transform LeftIndexProximal => m_LeftIndexProximal;
        [SerializeField] private Transform m_LeftIndexIntermediate; public Transform LeftIndexIntermediate => m_LeftIndexIntermediate;
        [SerializeField] private Transform m_LeftIndexDistal; public Transform LeftIndexDistal => m_LeftIndexDistal;
        [SerializeField] private Transform m_LeftMiddleProximal; public Transform LeftMiddleProximal => m_LeftMiddleProximal;
        [SerializeField] private Transform m_LeftMiddleIntermediate; public Transform LeftMiddleIntermediate => m_LeftMiddleIntermediate;
        [SerializeField] private Transform m_LeftMiddleDistal; public Transform LeftMiddleDistal => m_LeftMiddleDistal;
        [SerializeField] private Transform m_LeftRingProximal; public Transform LeftRingProximal => m_LeftRingProximal;
        [SerializeField] private Transform m_LeftRingIntermediate; public Transform LeftRingIntermediate => m_LeftRingIntermediate;
        [SerializeField] private Transform m_LeftRingDistal; public Transform LeftRingDistal => m_LeftRingDistal;
        [SerializeField] private Transform m_LeftLittleProximal; public Transform LeftLittleProximal => m_LeftLittleProximal;
        [SerializeField] private Transform m_LeftLittleIntermediate; public Transform LeftLittleIntermediate => m_LeftLittleIntermediate;
        [SerializeField] private Transform m_LeftLittleDistal; public Transform LeftLittleDistal => m_LeftLittleDistal;
        [SerializeField] private Transform m_RightThumbProximal; public Transform RightThumbProximal => m_RightThumbProximal;
        [SerializeField] private Transform m_RightThumbIntermediate; public Transform RightThumbIntermediate => m_RightThumbIntermediate;
        [SerializeField] private Transform m_RightThumbDistal; public Transform RightThumbDistal => m_RightThumbDistal;
        [SerializeField] private Transform m_RightIndexProximal; public Transform RightIndexProximal => m_RightIndexProximal;
        [SerializeField] private Transform m_RightIndexIntermediate; public Transform RightIndexIntermediate => m_RightIndexIntermediate;
        [SerializeField] private Transform m_RightIndexDistal; public Transform RightIndexDistal => m_RightIndexDistal;
        [SerializeField] private Transform m_RightMiddleProximal; public Transform RightMiddleProximal => m_RightMiddleProximal;
        [SerializeField] private Transform m_RightMiddleIntermediate; public Transform RightMiddleIntermediate => m_RightMiddleIntermediate;
        [SerializeField] private Transform m_RightMiddleDistal; public Transform RightMiddleDistal => m_RightMiddleDistal;
        [SerializeField] private Transform m_RightRingProximal; public Transform RightRingProximal => m_RightRingProximal;
        [SerializeField] private Transform m_RightRingIntermediate; public Transform RightRingIntermediate => m_RightRingIntermediate;
        [SerializeField] private Transform m_RightRingDistal; public Transform RightRingDistal => m_RightRingDistal;
        [SerializeField] private Transform m_RightLittleProximal; public Transform RightLittleProximal => m_RightLittleProximal;
        [SerializeField] private Transform m_RightLittleIntermediate; public Transform RightLittleIntermediate => m_RightLittleIntermediate;
        [SerializeField] private Transform m_RightLittleDistal; public Transform RightLittleDistal => m_RightLittleDistal;
        #endregion

        void Reset()
        {
            AssignBonesFromAnimator();
        }

        public struct Validation
        {
            public readonly string Message;
            public readonly bool IsError;

            public Validation(string message, bool isError)
            {
                Message = message;
                IsError = isError;
            }
        }

        IEnumerable<Validation> Required(params (string, Transform)[] props)
        {
            foreach (var prop in props)
            {
                if (prop.Item2 == null)
                {
                    var name = prop.Item1;
                    if (name.StartsWith("m_"))
                    {
                        name = name.Substring(2);
                    }
                    yield return new Validation($"{name} is Required", true);
                }
            }
        }

        static Vector3 GetForward(Transform l, Transform r)
        {
            if (l == null || r == null)
            {
                return Vector3.zero;
            }
            var lr = (r.position - l.position).normalized;
            return Vector3.Cross(lr, Vector3.up);
        }

        public Vector3 GetForward()
        {
            return GetForward(m_LeftUpperLeg, m_RightUpperLeg);
        }

        public IEnumerable<Validation> Validate()
        {
            foreach (var validation in Required(
                (nameof(m_Hips), m_Hips), (nameof(m_Spine), m_Spine), (nameof(m_Head), m_Head),
                (nameof(m_LeftUpperLeg), m_LeftUpperLeg), (nameof(m_LeftLowerLeg), m_LeftLowerLeg), (nameof(m_LeftFoot), m_LeftFoot),
                (nameof(m_RightUpperLeg), m_RightUpperLeg), (nameof(m_RightLowerLeg), m_RightLowerLeg), (nameof(m_RightFoot), m_RightFoot),
                (nameof(m_LeftUpperArm), m_LeftUpperArm), (nameof(m_LeftLowerArm), m_LeftLowerArm), (nameof(m_LeftHand), m_LeftHand),
                (nameof(m_RightUpperArm), m_RightUpperArm), (nameof(m_RightLowerArm), m_RightLowerArm), (nameof(m_RightHand), m_RightHand)
            ))
            {
                yield return validation;
            }

            // var forward = GetForward();
            // if (Vector3.Dot(Vector3.forward, forward) < 0.5f)
            // {
            //     yield return new Validation("Not facing the Z-axis positive direction", true);
            // }
        }

        /// <summary>
        /// ボーン割り当てから UnityEngine.Avatar を生成する
        /// </summary>
        /// <returns></returns>
        public Avatar CreateAvatar()
        {
            ForceTransformUniqueName.Process(transform);
            return HumanoidLoader.BuildHumanAvatarFromMap(transform, BoneMap);
        }

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Hips: return Hips;

                #region leg
                case HumanBodyBones.LeftUpperLeg: return LeftUpperLeg;
                case HumanBodyBones.RightUpperLeg: return RightUpperLeg;
                case HumanBodyBones.LeftLowerLeg: return LeftLowerLeg;
                case HumanBodyBones.RightLowerLeg: return RightLowerLeg;
                case HumanBodyBones.LeftFoot: return LeftFoot;
                case HumanBodyBones.RightFoot: return RightFoot;
                case HumanBodyBones.LeftToes: return LeftToes;
                case HumanBodyBones.RightToes: return RightToes;
                #endregion

                #region spine
                case HumanBodyBones.Spine: return Spine;
                case HumanBodyBones.Chest: return Chest;
                case HumanBodyBones.UpperChest: return UpperChest;
                case HumanBodyBones.Neck: return Neck;
                case HumanBodyBones.Head: return Head;
                case HumanBodyBones.LeftEye: return LeftEye;
                case HumanBodyBones.RightEye: return RightEye;
                case HumanBodyBones.Jaw: return Jaw;
                #endregion

                #region arm
                case HumanBodyBones.LeftShoulder: return LeftShoulder;
                case HumanBodyBones.RightShoulder: return RightShoulder;
                case HumanBodyBones.LeftUpperArm: return LeftUpperArm;
                case HumanBodyBones.RightUpperArm: return RightUpperArm;
                case HumanBodyBones.LeftLowerArm: return LeftLowerArm;
                case HumanBodyBones.RightLowerArm: return RightLowerArm;
                case HumanBodyBones.LeftHand: return LeftHand;
                case HumanBodyBones.RightHand: return RightHand;
                #endregion

                #region fingers
                case HumanBodyBones.LeftThumbProximal: return LeftThumbProximal;
                case HumanBodyBones.LeftThumbIntermediate: return LeftThumbIntermediate;
                case HumanBodyBones.LeftThumbDistal: return LeftThumbDistal;
                case HumanBodyBones.LeftIndexProximal: return LeftIndexProximal;
                case HumanBodyBones.LeftIndexIntermediate: return LeftIndexIntermediate;
                case HumanBodyBones.LeftIndexDistal: return LeftIndexDistal;
                case HumanBodyBones.LeftMiddleProximal: return LeftMiddleProximal;
                case HumanBodyBones.LeftMiddleIntermediate: return LeftMiddleIntermediate;
                case HumanBodyBones.LeftMiddleDistal: return LeftMiddleDistal;
                case HumanBodyBones.LeftRingProximal: return LeftRingProximal;
                case HumanBodyBones.LeftRingIntermediate: return LeftRingIntermediate;
                case HumanBodyBones.LeftRingDistal: return LeftRingDistal;
                case HumanBodyBones.LeftLittleProximal: return LeftLittleProximal;
                case HumanBodyBones.LeftLittleIntermediate: return LeftLittleIntermediate;
                case HumanBodyBones.LeftLittleDistal: return LeftLittleDistal;
                case HumanBodyBones.RightThumbProximal: return RightThumbProximal;
                case HumanBodyBones.RightThumbIntermediate: return RightThumbIntermediate;
                case HumanBodyBones.RightThumbDistal: return RightThumbDistal;
                case HumanBodyBones.RightIndexProximal: return RightIndexProximal;
                case HumanBodyBones.RightIndexIntermediate: return RightIndexIntermediate;
                case HumanBodyBones.RightIndexDistal: return RightIndexDistal;
                case HumanBodyBones.RightMiddleProximal: return RightMiddleProximal;
                case HumanBodyBones.RightMiddleIntermediate: return RightMiddleIntermediate;
                case HumanBodyBones.RightMiddleDistal: return RightMiddleDistal;
                case HumanBodyBones.RightRingProximal: return RightRingProximal;
                case HumanBodyBones.RightRingIntermediate: return RightRingIntermediate;
                case HumanBodyBones.RightRingDistal: return RightRingDistal;
                case HumanBodyBones.RightLittleProximal: return RightLittleProximal;
                case HumanBodyBones.RightLittleIntermediate: return RightLittleIntermediate;
                case HumanBodyBones.RightLittleDistal: return RightLittleDistal;
                    #endregion

            }

            return null;
        }

        public IEnumerable<(Transform, HumanBodyBones)> BoneMap
        {
            get
            {
                if (Hips != null) { yield return (Hips, HumanBodyBones.Hips); }

                #region leg
                if (LeftUpperLeg != null) { yield return (LeftUpperLeg, HumanBodyBones.LeftUpperLeg); }
                if (RightUpperLeg != null) { yield return (RightUpperLeg, HumanBodyBones.RightUpperLeg); }
                if (LeftLowerLeg != null) { yield return (LeftLowerLeg, HumanBodyBones.LeftLowerLeg); }
                if (RightLowerLeg != null) { yield return (RightLowerLeg, HumanBodyBones.RightLowerLeg); }
                if (LeftFoot != null) { yield return (LeftFoot, HumanBodyBones.LeftFoot); }
                if (RightFoot != null) { yield return (RightFoot, HumanBodyBones.RightFoot); }
                if (LeftToes != null) { yield return (LeftToes, HumanBodyBones.LeftToes); }
                if (RightToes != null) { yield return (RightToes, HumanBodyBones.RightToes); }
                #endregion

                #region spine
                if (Spine != null) { yield return (Spine, HumanBodyBones.Spine); }
                if (Chest != null) { yield return (Chest, HumanBodyBones.Chest); }
                if (UpperChest != null) { yield return (UpperChest, HumanBodyBones.UpperChest); }
                if (Neck != null) { yield return (Neck, HumanBodyBones.Neck); }
                if (Head != null) { yield return (Head, HumanBodyBones.Head); }
                if (LeftEye != null) { yield return (LeftEye, HumanBodyBones.LeftEye); }
                if (RightEye != null) { yield return (RightEye, HumanBodyBones.RightEye); }
                if (Jaw != null) { yield return (Jaw, HumanBodyBones.Jaw); }
                #endregion

                #region arm
                if (LeftShoulder != null) { yield return (LeftShoulder, HumanBodyBones.LeftShoulder); }
                if (RightShoulder != null) { yield return (RightShoulder, HumanBodyBones.RightShoulder); }
                if (LeftUpperArm != null) { yield return (LeftUpperArm, HumanBodyBones.LeftUpperArm); }
                if (RightUpperArm != null) { yield return (RightUpperArm, HumanBodyBones.RightUpperArm); }
                if (LeftLowerArm != null) { yield return (LeftLowerArm, HumanBodyBones.LeftLowerArm); }
                if (RightLowerArm != null) { yield return (RightLowerArm, HumanBodyBones.RightLowerArm); }
                if (LeftHand != null) { yield return (LeftHand, HumanBodyBones.LeftHand); }
                if (RightHand != null) { yield return (RightHand, HumanBodyBones.RightHand); }
                #endregion

                #region fingers
                if (LeftThumbProximal != null) { yield return (LeftThumbProximal, HumanBodyBones.LeftThumbProximal); }
                if (LeftThumbIntermediate != null) { yield return (LeftThumbIntermediate, HumanBodyBones.LeftThumbIntermediate); }
                if (LeftThumbDistal != null) { yield return (LeftThumbDistal, HumanBodyBones.LeftThumbDistal); }
                if (LeftIndexProximal != null) { yield return (LeftIndexProximal, HumanBodyBones.LeftIndexProximal); }
                if (LeftIndexIntermediate != null) { yield return (LeftIndexIntermediate, HumanBodyBones.LeftIndexIntermediate); }
                if (LeftIndexDistal != null) { yield return (LeftIndexDistal, HumanBodyBones.LeftIndexDistal); }
                if (LeftMiddleProximal != null) { yield return (LeftMiddleProximal, HumanBodyBones.LeftMiddleProximal); }
                if (LeftMiddleIntermediate != null) { yield return (LeftMiddleIntermediate, HumanBodyBones.LeftMiddleIntermediate); }
                if (LeftMiddleDistal != null) { yield return (LeftMiddleDistal, HumanBodyBones.LeftMiddleDistal); }
                if (LeftRingProximal != null) { yield return (LeftRingProximal, HumanBodyBones.LeftRingProximal); }
                if (LeftRingIntermediate != null) { yield return (LeftRingIntermediate, HumanBodyBones.LeftRingIntermediate); }
                if (LeftRingDistal != null) { yield return (LeftRingDistal, HumanBodyBones.LeftRingDistal); }
                if (LeftLittleProximal != null) { yield return (LeftLittleProximal, HumanBodyBones.LeftLittleProximal); }
                if (LeftLittleIntermediate != null) { yield return (LeftLittleIntermediate, HumanBodyBones.LeftLittleIntermediate); }
                if (LeftLittleDistal != null) { yield return (LeftLittleDistal, HumanBodyBones.LeftLittleDistal); }
                if (RightThumbProximal != null) { yield return (RightThumbProximal, HumanBodyBones.RightThumbProximal); }
                if (RightThumbIntermediate != null) { yield return (RightThumbIntermediate, HumanBodyBones.RightThumbIntermediate); }
                if (RightThumbDistal != null) { yield return (RightThumbDistal, HumanBodyBones.RightThumbDistal); }
                if (RightIndexProximal != null) { yield return (RightIndexProximal, HumanBodyBones.RightIndexProximal); }
                if (RightIndexIntermediate != null) { yield return (RightIndexIntermediate, HumanBodyBones.RightIndexIntermediate); }
                if (RightIndexDistal != null) { yield return (RightIndexDistal, HumanBodyBones.RightIndexDistal); }
                if (RightMiddleProximal != null) { yield return (RightMiddleProximal, HumanBodyBones.RightMiddleProximal); }
                if (RightMiddleIntermediate != null) { yield return (RightMiddleIntermediate, HumanBodyBones.RightMiddleIntermediate); }
                if (RightMiddleDistal != null) { yield return (RightMiddleDistal, HumanBodyBones.RightMiddleDistal); }
                if (RightRingProximal != null) { yield return (RightRingProximal, HumanBodyBones.RightRingProximal); }
                if (RightRingIntermediate != null) { yield return (RightRingIntermediate, HumanBodyBones.RightRingIntermediate); }
                if (RightRingDistal != null) { yield return (RightRingDistal, HumanBodyBones.RightRingDistal); }
                if (RightLittleProximal != null) { yield return (RightLittleProximal, HumanBodyBones.RightLittleProximal); }
                if (RightLittleIntermediate != null) { yield return (RightLittleIntermediate, HumanBodyBones.RightLittleIntermediate); }
                if (RightLittleDistal != null) { yield return (RightLittleDistal, HumanBodyBones.RightLittleDistal); }
                #endregion
            }
        }

        /// <summary>
        /// nodes からボーンを割り当てる
        /// </summary>
        /// <param name="nodes"></param>
        public void AssignBones(IEnumerable<(HumanBodyBones, Transform)> nodes)
        {
            foreach (var (key, value) in nodes)
            {
                if (key == HumanBodyBones.LastBone)
                {
                    continue;
                }
                if (value is null)
                {
                    continue;
                }

                switch (key)
                {
                    case HumanBodyBones.Hips: m_Hips = value; break;

                    #region leg
                    case HumanBodyBones.LeftUpperLeg: m_LeftUpperLeg = value; break;
                    case HumanBodyBones.RightUpperLeg: m_RightUpperLeg = value; break;
                    case HumanBodyBones.LeftLowerLeg: m_LeftLowerLeg = value; break;
                    case HumanBodyBones.RightLowerLeg: m_RightLowerLeg = value; break;
                    case HumanBodyBones.LeftFoot: m_LeftFoot = value; break;
                    case HumanBodyBones.RightFoot: m_RightFoot = value; break;
                    case HumanBodyBones.LeftToes: m_LeftToes = value; break;
                    case HumanBodyBones.RightToes: m_RightToes = value; break;
                    #endregion

                    #region spine
                    case HumanBodyBones.Spine: m_Spine = value; break;
                    case HumanBodyBones.Chest: m_Chest = value; break;
                    case HumanBodyBones.UpperChest: m_UpperChest = value; break;
                    case HumanBodyBones.Neck: m_Neck = value; break;
                    case HumanBodyBones.Head: m_Head = value; break;
                    case HumanBodyBones.LeftEye: m_LeftEye = value; break;
                    case HumanBodyBones.RightEye: m_RightEye = value; break;
                    case HumanBodyBones.Jaw: m_Jaw = value; break;
                    #endregion

                    #region arm
                    case HumanBodyBones.LeftShoulder: m_LeftShoulder = value; break;
                    case HumanBodyBones.RightShoulder: m_RightShoulder = value; break;
                    case HumanBodyBones.LeftUpperArm: m_LeftUpperArm = value; break;
                    case HumanBodyBones.RightUpperArm: m_RightUpperArm = value; break;
                    case HumanBodyBones.LeftLowerArm: m_LeftLowerArm = value; break;
                    case HumanBodyBones.RightLowerArm: m_RightLowerArm = value; break;
                    case HumanBodyBones.LeftHand: m_LeftHand = value; break;
                    case HumanBodyBones.RightHand: m_RightHand = value; break;
                    #endregion

                    #region fingers
                    case HumanBodyBones.LeftThumbProximal: m_LeftThumbProximal = value; break;
                    case HumanBodyBones.LeftThumbIntermediate: m_LeftThumbIntermediate = value; break;
                    case HumanBodyBones.LeftThumbDistal: m_LeftThumbDistal = value; break;
                    case HumanBodyBones.LeftIndexProximal: m_LeftIndexProximal = value; break;
                    case HumanBodyBones.LeftIndexIntermediate: m_LeftIndexIntermediate = value; break;
                    case HumanBodyBones.LeftIndexDistal: m_LeftIndexDistal = value; break;
                    case HumanBodyBones.LeftMiddleProximal: m_LeftMiddleProximal = value; break;
                    case HumanBodyBones.LeftMiddleIntermediate: m_LeftMiddleIntermediate = value; break;
                    case HumanBodyBones.LeftMiddleDistal: m_LeftMiddleDistal = value; break;
                    case HumanBodyBones.LeftRingProximal: m_LeftRingProximal = value; break;
                    case HumanBodyBones.LeftRingIntermediate: m_LeftRingIntermediate = value; break;
                    case HumanBodyBones.LeftRingDistal: m_LeftRingDistal = value; break;
                    case HumanBodyBones.LeftLittleProximal: m_LeftLittleProximal = value; break;
                    case HumanBodyBones.LeftLittleIntermediate: m_LeftLittleIntermediate = value; break;
                    case HumanBodyBones.LeftLittleDistal: m_LeftLittleDistal = value; break;
                    case HumanBodyBones.RightThumbProximal: m_RightThumbProximal = value; break;
                    case HumanBodyBones.RightThumbIntermediate: m_RightThumbIntermediate = value; break;
                    case HumanBodyBones.RightThumbDistal: m_RightThumbDistal = value; break;
                    case HumanBodyBones.RightIndexProximal: m_RightIndexProximal = value; break;
                    case HumanBodyBones.RightIndexIntermediate: m_RightIndexIntermediate = value; break;
                    case HumanBodyBones.RightIndexDistal: m_RightIndexDistal = value; break;
                    case HumanBodyBones.RightMiddleProximal: m_RightMiddleProximal = value; break;
                    case HumanBodyBones.RightMiddleIntermediate: m_RightMiddleIntermediate = value; break;
                    case HumanBodyBones.RightMiddleDistal: m_RightMiddleDistal = value; break;
                    case HumanBodyBones.RightRingProximal: m_RightRingProximal = value; break;
                    case HumanBodyBones.RightRingIntermediate: m_RightRingIntermediate = value; break;
                    case HumanBodyBones.RightRingDistal: m_RightRingDistal = value; break;
                    case HumanBodyBones.RightLittleProximal: m_RightLittleProximal = value; break;
                    case HumanBodyBones.RightLittleIntermediate: m_RightLittleIntermediate = value; break;
                    case HumanBodyBones.RightLittleDistal: m_RightLittleDistal = value; break;
                        #endregion
                }
            }
        }

        /// <summary>
        /// Animator から Bone を割り当てる
        /// </summary>
        /// <returns></returns>
        public bool AssignBonesFromAnimator()
        {
            if (TryGetComponent<Animator>(out var animator))
            {
                var avatar = animator.avatar;
                if (avatar == null)
                {
                    return false;
                }
                if (!avatar.isValid)
                {
                    return false;
                }
                if (!avatar.isHuman)
                {
                    return false;
                }

                var keys = CachedEnum.GetValues<HumanBodyBones>();

                AssignBones(keys.Select(x =>
                {
                    if (x == HumanBodyBones.LastBone)
                    {
                        return (HumanBodyBones.LastBone, null);
                    }
                    return ((HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), x.ToString(), true), animator.GetBoneTransform(x));
                }));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetBoneForTransform(Transform t, out HumanBodyBones bone)
        {
            foreach (var (v, k) in BoneMap)
            {
                if (v == t)
                {
                    bone = k;
                    return true;
                }
            }
            bone = default;
            return false;
        }

        public static Func<HumanBodyBones, Transform> Get_GetBoneTransform(GameObject root)
        {
            if (root.TryGetComponent<UniHumanoid.Humanoid>(out var humanoid))
            {
                return humanoid.GetBoneTransform;
            }
            else if (root.TryGetComponent<Animator>(out var animator))
            {
                // avatar
                var avatar = animator.avatar;
                if (avatar == null)
                {
                    throw new ArgumentException("no avatar");
                }
                if (!avatar.isValid)
                {
                    throw new ArgumentException("invalid avatar");
                }
                if (!avatar.isHuman)
                {
                    throw new ArgumentException("avatar is not humanoid");
                }
                return animator.GetBoneTransform;
            }
            else
            {
                throw new ArgumentException("no animator nor humanoid");
            }
        }
    }
}
