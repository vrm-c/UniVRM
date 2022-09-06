using System;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public sealed class Vrm10RuntimeLookAt : ILookAtEyeDirectionProvider
    {
        VRM10ObjectLookAt m_lookat;

        private Transform m_head;
        private Transform m_leftEye;
        private Transform m_rightEye;
        private ILookAtEyeDirectionApplicable _eyeDirectionApplicable;

        internal ILookAtEyeDirectionApplicable EyeDirectionApplicable => _eyeDirectionApplicable;

        public LookAtEyeDirection EyeDirection { get; private set; }

        #region LookAtTargetTypes.CalcYawPitchToGaze
        // 座標計算用のempty
        Transform m_lookAtSpace;
        public Transform GetLookAtOrigin(Transform head)
        {
            if (!Application.isPlaying)
            {
                return null;
            }
            if (m_lookAtSpace == null)
            {
                m_lookAtSpace = new GameObject("_lookat_origin_").transform;
                m_lookAtSpace.SetParent(head);
            }
            return m_lookAtSpace;
        }

        /// <summary>
        /// Headローカルの注視点からYaw, Pitch角を計算する
        /// </summary>
        (float, float) CalcLookAtYawPitch(Vector3 targetWorldPosition, Transform head)
        {
            var lookAtSpace = GetLookAtOrigin(head);
            lookAtSpace.localPosition = m_lookat.OffsetFromHead;
            var localPosition = lookAtSpace.worldToLocalMatrix.MultiplyPoint(targetWorldPosition);
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
        public (float, float) GetLookAtYawPitch(Transform head, VRM10ObjectLookAt.LookAtTargetTypes lookAtTargetType, Transform gaze)
        {
            switch (lookAtTargetType)
            {
                case VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze:
                    // Gaze(Transform)のワールド位置に対して計算する
                    return CalcLookAtYawPitch(gaze.position, head);

                case VRM10ObjectLookAt.LookAtTargetTypes.SetYawPitch:
                    // 事前にSetYawPitchした値を使う
                    return (m_yaw, m_pitch);
            }

            throw new NotImplementedException();
        }

        internal Vrm10RuntimeLookAt(VRM10ObjectLookAt lookat, UniHumanoid.Humanoid humanoid, Transform head, VRM10ObjectLookAt.LookAtTargetTypes lookAtTargetType, Transform gaze)
        {
            // 初期姿勢で初期化する！
            GetLookAtOrigin(head);

            m_lookat = lookat;

            m_head = head;
            m_leftEye = humanoid.GetBoneTransform(HumanBodyBones.LeftEye);
            m_rightEye = humanoid.GetBoneTransform(HumanBodyBones.RightEye);

            var isRuntimeAsset = true;
#if UNITY_EDITOR
            isRuntimeAsset = Application.isPlaying && !PrefabUtility.IsPartOfAnyPrefab(m_head);
#endif
            if (isRuntimeAsset && lookAtTargetType == VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze && gaze == null)
            {
                gaze = new GameObject().transform;
                gaze.name = "__LOOKAT_GAZE__";
                gaze.SetParent(m_head);
                gaze.localPosition = Vector3.forward;
            }

            // bone が無いときのエラー防止。マイグレーション失敗？
            if (m_lookat.LookAtType == LookAtType.bone && m_leftEye != null && m_rightEye != null)
            {
                _eyeDirectionApplicable = new LookAtEyeDirectionApplicableToBone(m_leftEye, m_rightEye, m_lookat.HorizontalOuter, m_lookat.HorizontalInner, m_lookat.VerticalDown, m_lookat.VerticalUp);
            }
            else
            {
                _eyeDirectionApplicable = new LookAtEyeDirectionApplicableToExpression(m_lookat.HorizontalOuter, m_lookat.HorizontalInner, m_lookat.VerticalDown, m_lookat.VerticalUp);
            }
        }

        internal void Process(VRM10ObjectLookAt.LookAtTargetTypes lookAtTargetType, Transform gaze)
        {
            var (yaw, pitch) = GetLookAtYawPitch(m_head, lookAtTargetType, gaze);
            EyeDirection = new LookAtEyeDirection(yaw, pitch, 0, 0);
        }
    }
}
