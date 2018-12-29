#pragma warning disable 0414, 0649
using System;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public enum UpdateType
    {
        None,
        Update,
        LateUpdate,
    }

    /// <summary>
    /// Headボーンローカルで目標物のYaw, Pitchを求めて目線に適用する
    /// 
    /// * VRMLookAtBoneApplyer
    /// * VRMLookAtBlendShapeApplyer
    /// 
    /// </summary>
    public class VRMLookAtHead : MonoBehaviour, IVRMComponent
    {
        public bool DrawGizmo = true;

        [SerializeField]
        public UpdateType UpdateType = UpdateType.Update;

        [SerializeField]
        public Transform Target;

        [SerializeField]
        public Transform Head;

        #region Thumbnail
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
            if (Head == null) return;
            var headPosition = Head.position + new Vector3(0, 0.05f, 0);
            t.position = headPosition + Head.localToWorldMatrix.ExtractRotation() * new Vector3(0, 0, 0.7f);
            t.LookAt(headPosition);
        }
        #endregion

        void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("animator is not found");
                return;
            }

            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head == null)
            {
                Debug.LogWarning("head is not found");
                return;
            }

            Head = head;
        }

        public void OnImported(VRMImporterContext context)
        { 
            var gltfFirstPerson = context.GLTF.extensions.VRM.firstPerson;
            switch (gltfFirstPerson.lookAtType)
            {
                case LookAtType.Bone:
                    {
                        var applyer = gameObject.AddComponent<VRMLookAtBoneApplyer>();
                        applyer.OnImported(context);
                    }
                    break;

                case LookAtType.BlendShape:
                    {
                        var applyer = gameObject.AddComponent<VRMLookAtBlendShapeApplyer>();
                        applyer.OnImported(context);
                    }
                    break;
            }
        }

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
                var yaw = Quaternion.AngleAxis(m_yaw, Vector3.up);
                var m = default(Matrix4x4);
                m.SetTRS(Vector3.zero, yaw, Vector3.one);
                return m;
            }
        }

        [SerializeField, Header("Debug")]
        float m_yaw;
        public float Yaw
        {
            get { return m_yaw; }
        }

        [SerializeField]
        float m_pitch;
        public float Pitch
        {
            get { return m_pitch; }
        }

        public event Action<float, float> YawPitchChanged;
        public void RaiseYawPitchChanged(float yaw, float pitch)
        {
            m_yaw = yaw;
            m_pitch = pitch;
            var handle = YawPitchChanged;
            if (handle != null)
            {
                handle(yaw, pitch);
            }
        }

        private void Update()
        {
            if (Head == null)
            {
                enabled = false;
                return;
            }

            if (UpdateType == UpdateType.Update)
            {
                LookWorldPosition();
            }
        }

        private void LateUpdate()
        {
            if (Head == null)
            {
                enabled = false;
                return;
            }

            if (UpdateType == UpdateType.LateUpdate)
            {
                LookWorldPosition();
            }
        }

        public void LookWorldPosition()
        {
            if (Target == null) return;
            float yaw;
            float pitch;
            LookWorldPosition(Target.position, out yaw, out pitch);
        }

        public void LookWorldPosition(Vector3 targetPosition, out float yaw, out float pitch)
        {
            var localPosition = Head.worldToLocalMatrix.MultiplyPoint(targetPosition);
            Matrix4x4.identity.CalcYawPitch(localPosition, out yaw, out pitch);
            RaiseYawPitchChanged(yaw, pitch);
        }
    }
}
