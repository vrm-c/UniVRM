using System;
using System.Collections.Generic;
using UnityEngine;
using VrmLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{
    [Serializable]
    public class VRM10ControllerLookAt
    {
        public enum LookAtTypes
        {
            // Gaze control by bone (leftEye, rightEye)
            Bone,
            // Gaze control by blend shape (lookUp, lookDown, lookLeft, lookRight)
            Expression,
        }

        public enum LookAtTargetTypes
        {
            CalcYawPitchToGaze,
            SetYawPitch,
        }

        [SerializeField]
        public bool DrawGizmo = true;

        [SerializeField]
        public Vector3 OffsetFromHead = new Vector3(0, 0.06f, 0);

        [SerializeField]
        public LookAtTypes LookAtType;

        [SerializeField]
        public CurveMapper HorizontalOuter = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper HorizontalInner = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalDown = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalUp = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public LookAtTargetTypes LookAtTargetType;

        OffsetOnTransform m_leftEye;
        OffsetOnTransform m_rightEye;
        ExpressionKey m_lookRightKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookRight);
        ExpressionKey m_lookLeftKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookLeft);
        ExpressionKey m_lookUpKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookUp);
        ExpressionKey m_lookDownKey = ExpressionKey.CreateFromPreset(ExpressionPreset.LookDown);

        #region LookAtTargetTypes.CalcYawPitchToGaze
        /// <summay>
        /// LookAtTargetTypes.CalcYawPitchToGaze時の注視点
        /// </summary>
        [SerializeField]
        public Transform Gaze;

        // 座標計算用のempty
        Transform m_lookAtOrigin;
        Transform GetLookAtOrigin(Transform head)
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
        public (float, float) GetLookAtYawPitch(Transform head)
        {
            switch (LookAtTargetType)
            {
                case LookAtTargetTypes.CalcYawPitchToGaze:
                    // Gaze(Transform)のワールド位置に対して計算する
                    return CalcLookAtYawPitch(Gaze.position, head);

                case LookAtTargetTypes.SetYawPitch:
                    // 事前にSetYawPitchした値を使う
                    return (m_yaw, m_pitch);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// LeftEyeボーンとRightEyeボーンに回転を適用する
        /// </summary>
        void LookAtBone(float yaw, float pitch)
        {
            // horizontal
            float leftYaw, rightYaw;
            if (yaw < 0)
            {
                leftYaw = -HorizontalOuter.Map(-yaw);
                rightYaw = -HorizontalInner.Map(-yaw);
            }
            else
            {
                rightYaw = HorizontalOuter.Map(yaw);
                leftYaw = HorizontalInner.Map(yaw);
            }

            // vertical
            if (pitch < 0)
            {
                pitch = -VerticalDown.Map(-pitch);
            }
            else
            {
                pitch = VerticalUp.Map(pitch);
            }

            // Apply
            if (m_leftEye.Transform != null && m_rightEye.Transform != null)
            {
                // 目に値を適用する
                m_leftEye.Transform.rotation = m_leftEye.InitialWorldMatrix.ExtractRotation() * Matrix4x4.identity.YawPitchRotation(leftYaw, pitch);
                m_rightEye.Transform.rotation = m_rightEye.InitialWorldMatrix.ExtractRotation() * Matrix4x4.identity.YawPitchRotation(rightYaw, pitch);
            }
        }

        public delegate void SetExpressionWeights(IEnumerable<KeyValuePair<ExpressionKey, float>> weights);

        /// <summary>
        /// Expression による LookAt の Weight を計算する
        /// </summary>
        private IEnumerable<KeyValuePair<ExpressionKey, float>> GetLookAtExpressionEnumerable(float yaw, float pitch)
        {
            if (yaw < 0)
            {
                // Left
                yield return new KeyValuePair<ExpressionKey, float>(m_lookRightKey, 0);
                yield return new KeyValuePair<ExpressionKey, float>(m_lookLeftKey, Mathf.Clamp(HorizontalOuter.Map(Mathf.Abs(yaw)), 0, 1.0f));
            }
            else
            {
                // Right
                yield return new KeyValuePair<ExpressionKey, float>(m_lookRightKey, Mathf.Clamp(HorizontalOuter.Map(Mathf.Abs(yaw)), 0, 1.0f));
                yield return new KeyValuePair<ExpressionKey, float>(m_lookLeftKey, 0);
            }

            if (pitch < 0)
            {
                // Down
                yield return new KeyValuePair<ExpressionKey, float>(m_lookUpKey, 0);
                yield return new KeyValuePair<ExpressionKey, float>(m_lookDownKey, Mathf.Clamp(VerticalDown.Map(Mathf.Abs(pitch)), 0, 1.0f));
            }
            else
            {
                // Up
                yield return new KeyValuePair<ExpressionKey, float>(m_lookUpKey, Mathf.Clamp(VerticalUp.Map(Mathf.Abs(pitch)), 0, 1.0f));
                yield return new KeyValuePair<ExpressionKey, float>(m_lookDownKey, 0);
            }
        }

        internal void Setup(Animator animator, Transform head)
        {
            m_leftEye = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.LeftEye));
            m_rightEye = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.RightEye));
            if (Gaze == null)
            {
                Gaze = new GameObject().transform;
                Gaze.name = "__LOOKAT_GAZE__";
                Gaze.SetParent(head);
                Gaze.localPosition = Vector3.forward;
            }
        }

        internal void Process(Transform head, SetExpressionWeights setExpressionWeights)
        {
            var (yaw, pitch) = GetLookAtYawPitch(head);

            switch (LookAtType)
            {
                case LookAtTypes.Bone:
                    LookAtBone(yaw, pitch);
                    break;

                case LookAtTypes.Expression:
                    setExpressionWeights(GetLookAtExpressionEnumerable(yaw, pitch));
                    break;
            }
        }

#if UNITY_EDITOR
        #region Gizmo
        static void DrawMatrix(Matrix4x4 m, float size)
        {
            Gizmos.matrix = m;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.right * size);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.up * size);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * size);
        }

        const float LOOKAT_GIZMO_SIZE = 0.5f;

        private void OnDrawGizmos()
        {
            if (DrawGizmo)
            {
                if (m_leftEye.Transform != null & m_rightEye.Transform != null)
                {
                    DrawMatrix(m_leftEye.WorldMatrix, LOOKAT_GIZMO_SIZE);
                    DrawMatrix(m_rightEye.WorldMatrix, LOOKAT_GIZMO_SIZE);
                }
            }
        }
        #endregion

        const float RADIUS = 0.5f;

        public void OnSceneGUILookAt(Transform head)
        {
            if (head == null) return;
            if (!DrawGizmo) return;

            if (Gaze != null)
            {
                {
                    EditorGUI.BeginChangeCheck();
                    var newTargetPosition = Handles.PositionHandle(Gaze.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(Gaze, "Change Look At Target Position");
                        Gaze.position = newTargetPosition;
                    }
                }

                Handles.color = new Color(1, 1, 1, 0.6f);
                Handles.DrawDottedLine(GetLookAtOrigin(head).position, Gaze.position, 4.0f);
            }

            var (yaw, pitch) = GetLookAtYawPitch(head);
            var lookAtOriginMatrix = GetLookAtOrigin(head).localToWorldMatrix;
            Handles.matrix = lookAtOriginMatrix;
            var p = OffsetFromHead;
            Handles.Label(Vector3.zero,
            $"FromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]\nYaw: {yaw:0.}degree\nPitch: {pitch:0.}degree");

            Handles.color = new Color(0, 1, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    Matrix4x4.identity.GetColumn(1),
                    Matrix4x4.identity.GetColumn(2),
                    yaw,
                    RADIUS);


            var yawQ = Quaternion.AngleAxis(yaw, Vector3.up);
            var yawMatrix = default(Matrix4x4);
            yawMatrix.SetTRS(Vector3.zero, yawQ, Vector3.one);

            Handles.matrix = lookAtOriginMatrix * yawMatrix;
            Handles.color = new Color(1, 0, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    Matrix4x4.identity.GetColumn(0),
                    Matrix4x4.identity.GetColumn(2),
                    -pitch,
                    RADIUS);
        }

#endif
    }
}
