#pragma warning disable 0414, 0649
using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    [Obsolete("Use VRMLookAtHead")]
    public class VRMLookAt : MonoBehaviour
    {
        public bool DrawGizmo = true;

        [SerializeField]
        public bool UseUpdate = true;

        [SerializeField]
        public Transform Target;

        [SerializeField]
        public OffsetOnTransform LeftEye;

        [SerializeField]
        public OffsetOnTransform RightEye;

        [SerializeField]
        public OffsetOnTransform Head;

        [SerializeField, Header("Degree Mapping")]
        public CurveMapper HorizontalOuter = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper HorizontalInner = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalDown = new CurveMapper(90.0f, 10.0f);

        [SerializeField]
        public CurveMapper VerticalUp = new CurveMapper(90.0f, 10.0f);

        public Texture2D CreateThumbnail()
        {
            var texture = new Texture2D(2048, 2048);
            {
                var go = new GameObject("ThumbCamera");
                var camera = go.AddComponent<Camera>();
                CreateThumbnail(camera, texture);
                if (Application.isPlaying) { GameObject.Destroy(go); } else { GameObject.DestroyImmediate(go); }
            }
            return texture;
        }
        void CreateThumbnail(Camera camera, Texture2D dst)
        {
            RenderTexture currentRT = RenderTexture.active;
            {
                var renderTexture = new RenderTexture(dst.width, dst.height, 24);
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                LookFace(camera.transform);
                camera.Render();
                dst.ReadPixels(new Rect(0, 0, dst.width, dst.height), 0, 0);

                RenderTexture.active = currentRT;
                camera.targetTexture = null;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(renderTexture);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(renderTexture);
                }
            }
        }

        public void LookFace(Transform t)
        {
            if (Head.Transform == null) return;
            var head = Head.Transform;
            var headPosition = head.position + new Vector3(0, 0.05f, 0);
            t.position = headPosition + Head.WorldMatrix.ExtractRotation() * new Vector3(0, 0, 0.7f);
            t.LookAt(headPosition);
        }

        public void CopyTo(GameObject _dst, Dictionary<Transform, Transform> map)
        {
            var dst = _dst.AddComponent<VRMLookAt>();
            dst.Target = Target;
            dst.Head = OffsetOnTransform.Create(map[Head.Transform]);
            dst.RightEye = OffsetOnTransform.Create(map[RightEye.Transform]);
            dst.LeftEye = OffsetOnTransform.Create(map[LeftEye.Transform]);

            dst.HorizontalOuter = HorizontalOuter;
            dst.HorizontalInner = HorizontalInner;
            dst.VerticalDown = VerticalDown;
            dst.VerticalUp = VerticalUp;
        }

        private void Reset()
        {
            Target = Camera.main.transform;

            GetBones();
        }

        private void OnValidate()
        {
            HorizontalInner.OnValidate();
            HorizontalOuter.OnValidate();
            VerticalUp.OnValidate();
            VerticalDown.OnValidate();
        }

        public void GetBones()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                LeftEye = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.LeftEye));
                RightEye = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.RightEye));
                Head = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.Head));
            }
        }

        private void Awake()
        {
            Head.Setup();
            LeftEye.Setup();
            RightEye.Setup();
        }

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

        const float SIZE = 0.5f;

        private void OnDrawGizmos()
        {
            if (!DrawGizmo) return;

            if (LeftEye.Transform != null & RightEye.Transform != null)
            {
                DrawMatrix(LeftEye.WorldMatrix, SIZE);
                DrawMatrix(RightEye.WorldMatrix, SIZE);

            }
            else
            {
                DrawMatrix(Head.WorldMatrix, SIZE);
            }
        }
        #endregion

        static Matrix4x4 LookAtMatrixFromWorld(Vector3 from, Vector3 target)
        {
            var pos = new Vector4(from.x, from.y, from.z, 1);
            return LookAtMatrix(UnityExtensions.Matrix4x4FromColumns(Vector3.right, Vector3.up, Vector3.forward, pos), target);
        }

        static Matrix4x4 LookAtMatrix(Vector3 up_vector, Vector3 localPosition)
        {
            var z_axis = localPosition.normalized;
            var x_axis = Vector3.Cross(up_vector, z_axis).normalized;
            var y_axis = Vector3.Cross(z_axis, x_axis).normalized;
            return UnityExtensions.Matrix4x4FromColumns(x_axis, y_axis, z_axis, new Vector4(0, 0, 0, 1));
        }

        static Matrix4x4 LookAtMatrix(Matrix4x4 m, Vector3 target)
        {
            return LookAtMatrix(Vector3.up, m.inverse.MultiplyPoint(target));
        }

        public Matrix4x4 YawMatrix
        {
            get
            {
                var yaw = Quaternion.AngleAxis(Yaw, Head.OffsetRotation.GetColumn(1));
                var m = default(Matrix4x4);
                m.SetTRS(Vector3.zero, yaw, Vector3.one);
                return m;
            }
        }

        [SerializeField, Header("Debug")]
        public float Yaw;
        public float Pitch;
        private void LateUpdate()
        {
            if (!UseUpdate) return;
            if (Target == null) return;

            LookWorldPosition(Target.position);
        }

        public void LookWorldPosition(Vector3 targetPosition)
        {
            var localPosition = Head.InitialWorldMatrix.inverse.MultiplyPoint(targetPosition);
            Head.OffsetRotation.CalcYawPitch(localPosition, out Yaw, out Pitch);

            ApplyRotations(Yaw, Pitch);
        }

        void ApplyRotations(float yaw, float pitch)
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
            if (LeftEye.Transform != null && RightEye.Transform != null)
            {
                // 目に値を適用する
                LeftEye.Transform.rotation = LeftEye.InitialWorldMatrix.ExtractRotation() * Head.OffsetRotation.YawPitchRotation(leftYaw, pitch);
                RightEye.Transform.rotation = RightEye.InitialWorldMatrix.ExtractRotation() * Head.OffsetRotation.YawPitchRotation(rightYaw, pitch);
            }
            else if (Head.Transform != null)
            {
                // 頭に値を適用する
                Head.Transform.rotation = Head.InitialWorldMatrix.ExtractRotation() * Head.OffsetRotation.YawPitchRotation(yaw, pitch);
            }
        }
    }
}
