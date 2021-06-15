using System;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{
    [Serializable]
    public class VRM10ControllerLookAt : ILookAtEyeDirectionProvider
    {
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
        public LookAtType LookAtType;

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

        private Transform m_head;
        private Transform m_leftEye;
        private Transform m_rightEye;
        private ILookAtEyeDirectionApplicable _eyeDirectionApplicable;

        internal ILookAtEyeDirectionApplicable EyeDirectionApplicable => _eyeDirectionApplicable;

        public LookAtEyeDirection EyeDirection { get; private set; }

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
        private (float, float) GetLookAtYawPitch(Transform head)
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

        internal void Setup(Animator animator, Transform head)
        {
            m_head = head;
            m_leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            m_rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

            var isRuntimeAsset = true;
#if UNITY_EDITOR
            isRuntimeAsset = Application.isPlaying && !PrefabUtility.IsPartOfAnyPrefab(m_head);
#endif
            if (isRuntimeAsset && LookAtTargetType == LookAtTargetTypes.CalcYawPitchToGaze && Gaze == null)
            {
                Gaze = new GameObject().transform;
                Gaze.name = "__LOOKAT_GAZE__";
                Gaze.SetParent(m_head);
                Gaze.localPosition = Vector3.forward;
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

        public void Process()
        {
            var (yaw, pitch) = GetLookAtYawPitch(m_head);
            EyeDirection = new LookAtEyeDirection(yaw, pitch, 0, 0);
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
                if (m_leftEye != null & m_rightEye != null)
                {
                    DrawMatrix(m_leftEye.localToWorldMatrix, LOOKAT_GIZMO_SIZE);
                    DrawMatrix(m_rightEye.localToWorldMatrix, LOOKAT_GIZMO_SIZE);
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
