using System;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{
    [Serializable]
    public class VRM10ObjectLookAt : ILookAtEyeDirectionProvider
    {
        public enum LookAtTargetTypes
        {
            CalcYawPitchToGaze,
            SetYawPitch,
        }

        [SerializeField]
        public Vector3 OffsetFromHead = new Vector3(0, 0.06f, 0);

        [SerializeField]
        public LookAtType LookAtType;

        [SerializeField]
        public CurveMapper HorizontalOuter = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper HorizontalInner = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalDown = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalUp = new CurveMapper(90.0f, 10.0f);

        private Transform m_head;
        private Transform m_leftEye;
        private Transform m_rightEye;
        private ILookAtEyeDirectionApplicable _eyeDirectionApplicable;

        internal ILookAtEyeDirectionApplicable EyeDirectionApplicable => _eyeDirectionApplicable;

        public LookAtEyeDirection EyeDirection { get; private set; }

        #region LookAtTargetTypes.CalcYawPitchToGaze
        // 座標計算用のempty
        Transform m_lookAtOrigin;
        public Transform GetLookAtOrigin(Transform head)
        {
            if (!Application.isPlaying)
            {
                return null;
            }
            if (m_lookAtOrigin == null)
            {
                m_lookAtOrigin = new GameObject("_lookat_origin_").transform;
                m_lookAtOrigin.SetParent(head);
            }
            return m_lookAtOrigin;
        }

        /// <summary>
        /// Headローカルの注視点からYaw, Pitch角を計算する
        /// </summary>
        (float, float) CalcLookAtYawPitch(Vector3 targetWorldPosition, Transform head)
        {
            GetLookAtOrigin(head).localPosition = OffsetFromHead;

            var localPosition = m_lookAtOrigin.worldToLocalMatrix.MultiplyPoint(targetWorldPosition);
            float yaw, pitch;
            Matrix4x4.identity.CalcYawPitch(localPosition, out yaw, out pitch);
            return (yaw, pitch);
        }
        #endregion

        #region LookAtTargetTypes.SetYawPitch
        float m_yaw;
        float m_pitch;

        /// <summary>
        /// LookAtTargetTypes.SetYawPitch時の視線の角度を指定する
        /// </summary>
        /// <param name="yaw">Headボーンのforwardに対するyaw角(度)</param>
        /// <param name="pitch">Headボーンのforwardに対するpitch角(度)</param>
        public void SetLookAtYawPitch(float yaw, float pitch)
        {
            m_yaw = yaw;
            m_pitch = pitch;
        }
        #endregion

        /// <summary>
        /// LookAtTargetType に応じた yaw, pitch を得る
        /// </summary>
        /// <returns>Headボーンのforwardに対するyaw角(度), pitch角(度)</returns>
        public (float, float) GetLookAtYawPitch(Transform head, LookAtTargetTypes lookAtTargetType, Transform gaze)
        {
            switch (lookAtTargetType)
            {
                case LookAtTargetTypes.CalcYawPitchToGaze:
                    // Gaze(Transform)のワールド位置に対して計算する
                    return CalcLookAtYawPitch(gaze.position, head);

                case LookAtTargetTypes.SetYawPitch:
                    // 事前にSetYawPitchした値を使う
                    return (m_yaw, m_pitch);
            }

            throw new NotImplementedException();
        }

        internal void Setup(Animator animator, Transform head, LookAtTargetTypes lookAtTargetType, Transform gaze)
        {
            m_head = head;
            m_leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            m_rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

            var isRuntimeAsset = true;
#if UNITY_EDITOR
            isRuntimeAsset = Application.isPlaying && !PrefabUtility.IsPartOfAnyPrefab(m_head);
#endif
            if (isRuntimeAsset && lookAtTargetType == LookAtTargetTypes.CalcYawPitchToGaze && gaze == null)
            {
                gaze = new GameObject().transform;
                gaze.name = "__LOOKAT_GAZE__";
                gaze.SetParent(m_head);
                gaze.localPosition = Vector3.forward;
            }
            switch (LookAtType)
            {
                case LookAtType.bone:
                    _eyeDirectionApplicable = new LookAtEyeDirectionApplicableToBone(m_leftEye, m_rightEye, HorizontalOuter, HorizontalInner, VerticalDown, VerticalUp);
                    break;
                case LookAtType.expression:
                    _eyeDirectionApplicable = new LookAtEyeDirectionApplicableToExpression(HorizontalOuter, HorizontalInner, VerticalDown, VerticalUp);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Process(LookAtTargetTypes lookAtTargetType, Transform gaze)
        {
            var (yaw, pitch) = GetLookAtYawPitch(m_head, lookAtTargetType, gaze);
            EyeDirection = new LookAtEyeDirection(yaw, pitch, 0, 0);
        }
    }
}
