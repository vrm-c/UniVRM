using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;

namespace VRM
{
    /// <summary>
    /// The base algorithm is http://rocketjump.skr.jp/unity3d/109/ of @ricopin416
    /// DefaultExecutionOrder(11000) means calculate springbone after FinalIK( VRIK )
    /// </summary>
    [DefaultExecutionOrder(11000)]
    // [RequireComponent(typeof(VCIObject))]
    public sealed class VRMSpringBone : MonoBehaviour
    {
        [SerializeField]
        public string m_comment;

        [SerializeField] [Header("Gizmo")] private bool m_drawGizmo = default;

        [SerializeField] private Color m_gizmoColor = Color.yellow;

        [SerializeField]
        [Range(0, 4)]
        [Header("Settings")]
        public float m_stiffnessForce = 1.0f;

        [SerializeField] [Range(0, 2)] public float m_gravityPower;

        [SerializeField] public Vector3 m_gravityDir = new Vector3(0, -1.0f, 0);

        [SerializeField] [Range(0, 1)] public float m_dragForce = 0.4f;

        [SerializeField] public Transform m_center;

        [SerializeField] public List<Transform> RootBones = new List<Transform>();
        Dictionary<Transform, Quaternion> m_initialLocalRotationMap;

        [SerializeField]
        [Range(0, 0.5f)]
        [Header("Collider")]
        public float m_hitRadius = 0.02f;

        [SerializeField]
        public VRMSpringBoneColliderGroup[] ColliderGroups;

        public enum SpringBoneUpdateType
        {
            LateUpdate,
            FixedUpdate,
        }
        [SerializeField]
        public SpringBoneUpdateType m_updateType = SpringBoneUpdateType.LateUpdate;

        /// <summary>
        /// original from
        /// http://rocketjump.skr.jp/unity3d/109/
        /// </summary>
        private class VRMSpringBoneLogic
        {
            Transform m_transform;
            public Transform Head => m_transform;

            private Vector3 m_boneAxis;
            private Vector3 m_currentTail;

            private readonly float m_length;
            private Vector3 m_localDir;
            private Vector3 m_prevTail;

            public VRMSpringBoneLogic(Transform center, Transform transform, Vector3 localChildPosition)
            {
                m_transform = transform;
                var worldChildPosition = m_transform.TransformPoint(localChildPosition);
                m_currentTail = center != null
                        ? center.InverseTransformPoint(worldChildPosition)
                        : worldChildPosition;
                m_prevTail = m_currentTail;
                LocalRotation = transform.localRotation;
                m_boneAxis = localChildPosition.normalized;
                m_length = localChildPosition.magnitude;
            }

            public Vector3 Tail => m_transform.localToWorldMatrix.MultiplyPoint(m_boneAxis * m_length);

            private Quaternion LocalRotation { get; }

            public float Radius { get; set; }

            private Quaternion ParentRotation =>
                m_transform.parent != null
                    ? m_transform.parent.rotation
                    : Quaternion.identity;

            public void Update(Transform center,
                float stiffnessForce, float dragForce, Vector3 external,
                List<SphereCollider> colliders)
            {
                var currentTail = center != null
                        ? center.TransformPoint(m_currentTail)
                        : m_currentTail;
                var prevTail = center != null
                        ? center.TransformPoint(m_prevTail)
                        : m_prevTail;

                // verlet積分で次の位置を計算
                var nextTail = currentTail
                               + (currentTail - prevTail) * (1.0f - dragForce) // 前フレームの移動を継続する(減衰もあるよ)
                               + ParentRotation * LocalRotation * m_boneAxis * stiffnessForce // 親の回転による子ボーンの移動目標
                               + external; // 外力による移動量

                // 長さをboneLengthに強制
                var position = m_transform.position;
                nextTail = position + (nextTail - position).normalized * m_length;

                // Collisionで移動
                nextTail = Collision(colliders, nextTail);

                m_prevTail = center != null
                        ? center.InverseTransformPoint(currentTail)
                        : currentTail;

                m_currentTail = center != null
                        ? center.InverseTransformPoint(nextTail)
                        : nextTail;

                //回転を適用
                m_transform.rotation = ApplyRotation(nextTail);
            }

            protected virtual Quaternion ApplyRotation(Vector3 nextTail)
            {
                var rotation = ParentRotation * LocalRotation;
                return Quaternion.FromToRotation(rotation * m_boneAxis,
                           nextTail - m_transform.position) * rotation;
            }

            protected virtual Vector3 Collision(List<SphereCollider> colliders, Vector3 nextTail)
            {
                foreach (var collider in colliders)
                {
                    var r = Radius + collider.Radius;
                    if (Vector3.SqrMagnitude(nextTail - collider.Position) <= (r * r))
                    {
                        // ヒット。Colliderの半径方向に押し出す
                        var normal = (nextTail - collider.Position).normalized;
                        var posFromCollider = collider.Position + normal * (Radius + collider.Radius);
                        // 長さをboneLengthに強制
                        nextTail = m_transform.position + (posFromCollider - m_transform.position).normalized * m_length;
                    }
                }
                return nextTail;
            }

            public void DrawGizmo(Transform center, float radius, Color color)
            {
                var currentTail = center != null
                        ? center.TransformPoint(m_currentTail)
                        : m_currentTail;
                var prevTail = center != null
                        ? center.TransformPoint(m_prevTail)
                        : m_prevTail;

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(currentTail, prevTail);
                Gizmos.DrawWireSphere(prevTail, radius);

                Gizmos.color = color;
                Gizmos.DrawLine(currentTail, m_transform.position);
                Gizmos.DrawWireSphere(currentTail, radius);
            }
        }
        List<VRMSpringBoneLogic> m_verlet = new List<VRMSpringBoneLogic>();

        void Awake()
        {
            Setup();
        }

        [ContextMenu("Reset bones")]
        public void Setup(bool force = false)
        {
            if (RootBones != null)
            {
                if (force || m_initialLocalRotationMap == null)
                {
                    m_initialLocalRotationMap = new Dictionary<Transform, Quaternion>();
                }
                else
                {
                    foreach (var kv in m_initialLocalRotationMap) kv.Key.localRotation = kv.Value;
                    m_initialLocalRotationMap.Clear();
                }
                m_verlet.Clear();

                foreach (var go in RootBones)
                {
                    if (go != null)
                    {
                        foreach (var x in go.transform.GetComponentsInChildren<Transform>(true)) m_initialLocalRotationMap[x] = x.localRotation;

                        SetupRecursive(m_center, go);
                    }
                }
            }
        }

        public void SetLocalRotationsIdentity()
        {
            foreach (var verlet in m_verlet) verlet.Head.localRotation = Quaternion.identity;
        }

        private static IEnumerable<Transform> GetChildren(Transform parent)
        {
            for (var i = 0; i < parent.childCount; ++i) yield return parent.GetChild(i);
        }

        private void SetupRecursive(Transform center, Transform parent)
        {
            if (parent.childCount == 0)
            {
                var delta = parent.position - parent.parent.position;
                var childPosition = parent.position + delta.normalized * 0.07f;
                m_verlet.Add(new VRMSpringBoneLogic(center, parent,
                    parent.worldToLocalMatrix.MultiplyPoint(childPosition)));
            }
            else
            {
                var firstChild = GetChildren(parent).First();
                var localPosition = firstChild.localPosition;
                var scale = firstChild.lossyScale;
                m_verlet.Add(new VRMSpringBoneLogic(center, parent,
                        new Vector3(
                            localPosition.x * scale.x,
                            localPosition.y * scale.y,
                            localPosition.z * scale.z
                        )))
                    ;
            }

            foreach (Transform child in parent) SetupRecursive(center, child);
        }

        void LateUpdate()
        {
            if (m_updateType == SpringBoneUpdateType.LateUpdate)
            {
                UpdateProcess(Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            if (m_updateType == SpringBoneUpdateType.FixedUpdate)
            {
                UpdateProcess(Time.fixedDeltaTime);
            }
        }

        public struct SphereCollider
        {
            // public Transform Transform;
            public readonly Vector3 Position;
            public readonly float Radius;

            public SphereCollider(Transform transform, VRMSpringBoneColliderGroup.SphereCollider collider)
            {
                Position = transform.TransformPoint(collider.Offset);
                var ls = transform.lossyScale;
                var scale = Mathf.Max(Mathf.Max(ls.x, ls.y), ls.z);
                Radius = scale * collider.Radius;
            }
        }

        private List<SphereCollider> m_colliders = new List<SphereCollider>();
        private void UpdateProcess(float deltaTime)
        {
            if (m_verlet == null || m_verlet.Count == 0)
            {
                if (RootBones == null) return;

                Setup();
            }

            m_colliders.Clear();
            if (ColliderGroups != null)
            {
                foreach (var group in ColliderGroups)
                {
                    if (group != null)
                    {
                        foreach (var collider in group.Colliders)
                        {
                            m_colliders.Add(new SphereCollider(group.transform, collider));
                        }
                    }
                }
            }

            var stiffness = m_stiffnessForce * deltaTime;
            var external = m_gravityDir * (m_gravityPower * deltaTime);

            foreach (var verlet in m_verlet)
            {
                verlet.Radius = m_hitRadius;
                verlet.Update(m_center,
                    stiffness,
                    m_dragForce,
                    external,
                    m_colliders
                );
            }
        }

        private void OnDrawGizmos()
        {
            if (!m_drawGizmo) return;

            foreach (var verlet in m_verlet)
                verlet.DrawGizmo(m_center, m_hitRadius, m_gizmoColor);
        }
    }
}