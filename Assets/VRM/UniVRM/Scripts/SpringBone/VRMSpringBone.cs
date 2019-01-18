using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;

namespace VRM
{
    /// <summary>
    /// The base algorithm is http://rocketjump.skr.jp/unity3d/109/ of @ricopin416
    /// DefaultExecutionOrder(11000) means calclate springbone after FinaiIK( VRIK )
    /// </summary>
    #if UNITY_5_5_OR_NEWER
    [DefaultExecutionOrder(11000)]
    #endif
    public class VRMSpringBone : MonoBehaviour
    {
        [SerializeField]
        public string m_comment;

        [SerializeField, Header("Gizmo")]
        bool m_drawGizmo;

        [SerializeField]
        Color m_gizmoColor = Color.yellow;

        [SerializeField, Range(0, 4), Header("Settings")]
        public float m_stiffnessForce = 1.0f;

        [SerializeField, Range(0, 2)]
        public float m_gravityPower = 0;

        [SerializeField]
        public Vector3 m_gravityDir = new Vector3(0, -1.0f, 0);

        [SerializeField, Range(0, 1)]
        public float m_dragForce = 0.4f;

        [SerializeField]
        public Transform m_center;

        [SerializeField]
        public List<Transform> RootBones = new List<Transform>();
        Dictionary<Transform, Quaternion> m_initialLocalRotationMap;

        [SerializeField, Range(0, 0.5f), Header("Collider")]
        public float m_hitRadius = 0.02f;

        [SerializeField]
        public VRMSpringBoneColliderGroup[] ColliderGroups;

        /// <summary>
        /// 
        /// original from
        /// 
        /// http://rocketjump.skr.jp/unity3d/109/
        /// 
        /// </summary>
        public class VRMSpringBoneLogic
        {
            Transform m_transform;
            public Transform Head
            {
                get { return m_transform; }
            }

            public Vector3 Tail
            {
                get { return m_transform.localToWorldMatrix.MultiplyPoint(m_boneAxis * m_length); }
            }

            float m_length;
            Vector3 m_currentTail;
            Vector3 m_prevTail;
            Vector3 m_localDir;
            Quaternion m_localRotation;
            public Quaternion LocalRotation
            {
                get { return m_localRotation; }
            }
            public Vector3 m_boneAxis;

            public float Radius { get; set; }

            public VRMSpringBoneLogic(Transform center, Transform transform, Vector3 localChildPosition)
            {
                m_transform = transform;
                var worldChildPosition = m_transform.TransformPoint(localChildPosition);
                m_currentTail = center!= null
                    ? center.InverseTransformPoint(worldChildPosition)
                    : worldChildPosition
                    ;
                m_prevTail = m_currentTail;
                m_localRotation = transform.localRotation;
                m_boneAxis = localChildPosition.normalized;
                m_length = localChildPosition.magnitude;
            }

            Quaternion ParentRotation
            {
                get
                {
                    return m_transform.parent != null
                        ? m_transform.parent.rotation
                        : Quaternion.identity
                        ;
                }
            }

            public void Update(Transform center,
                float stiffnessForce, float dragForce, Vector3 external,
                List<SphereCollider> colliders)
            {
                var currentTail = center!=null
                    ? center.TransformPoint(m_currentTail)
                    : m_currentTail
                    ;
                var prevTail = center!=null
                    ? center.TransformPoint(m_prevTail)
                    : m_prevTail
                    ;

                // verlet積分で次の位置を計算
                var nextTail = currentTail
                    + (currentTail - prevTail) * (1.0f - dragForce) // 前フレームの移動を継続する(減衰もあるよ)
                    + ParentRotation * m_localRotation * m_boneAxis * stiffnessForce // 親の回転による子ボーンの移動目標
                    + external // 外力による移動量
                    ;

                // 長さをboneLengthに強制
                nextTail = m_transform.position + (nextTail - m_transform.position).normalized * m_length;

                // Collisionで移動
                nextTail = Collision(colliders, nextTail);

                m_prevTail = center!=null
                    ? center.InverseTransformPoint(currentTail)
                    : currentTail
                    ;
                m_currentTail = center!=null
                    ? center.InverseTransformPoint(nextTail)
                    : nextTail
                    ;

                //回転を適用
                Head.rotation = ApplyRotation(nextTail);
            }

            protected virtual Quaternion ApplyRotation(Vector3 nextTail)
            {
                var rotation = ParentRotation * m_localRotation;
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
                var currentTail = center!=null
                    ? center.TransformPoint(m_currentTail)
                    : m_currentTail
                    ;
                var prevTail = center != null
                    ? center.TransformPoint(m_prevTail)
                    : m_prevTail
                    ;

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
        public void Setup(bool force=false)
        {
            if (RootBones != null)
            {
                if (force || m_initialLocalRotationMap == null)
                {
                    m_initialLocalRotationMap = new Dictionary<Transform, Quaternion>();
                }
                else
                {
                    foreach(var kv in m_initialLocalRotationMap)
                    {
                        kv.Key.localRotation = kv.Value;
                    }
                    m_initialLocalRotationMap.Clear();
                }
                m_verlet.Clear();

                foreach (var go in RootBones)
                {
                    if (go != null)
                    {
                        foreach(var x in go.transform.Traverse())
                        {
                            m_initialLocalRotationMap[x] = x.localRotation;
                        }

                        SetupRecursive(m_center, go);
                    }
                }
            }
        }

        static IEnumerable<Transform> GetChildren(Transform parent)
        {
            for(int i=0; i<parent.childCount; ++i)
            {
                yield return parent.GetChild(i);
            }
        }

        void SetupRecursive(Transform center, Transform parent)
        {
            if (parent.childCount == 0)
            {
                var delta = parent.position - parent.parent.position;
                var childPosition = parent.position + delta.normalized * 0.07f;
                m_verlet.Add(new VRMSpringBoneLogic(center, parent, parent.worldToLocalMatrix.MultiplyPoint(childPosition)));
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

            foreach (Transform child in parent)
            {
                SetupRecursive(center, child);
            }
        }

        public struct SphereCollider
        {
            public Vector3 Position;
            public float Radius;
        }

        List<SphereCollider> m_colliderList = new List<SphereCollider>();
        void LateUpdate()
        {
            if (m_verlet == null || m_verlet.Count == 0)
            {
                if (RootBones == null)
                {
                    return;
                }

                Setup();
            }

            m_colliderList.Clear();
            if (ColliderGroups != null)
            {
                foreach (var group in ColliderGroups)
                {
                    if (group != null)
                    {
                        foreach (var collider in group.Colliders)
                        {
                            m_colliderList.Add(new SphereCollider
                            {
                                Position = group.transform.TransformPoint(collider.Offset),
                                Radius = collider.Radius,
                            });
                        }
                    }
                }
            }

            var stiffness = m_stiffnessForce * Time.deltaTime;
            var external = m_gravityDir * (m_gravityPower * Time.deltaTime);

            foreach (var verlet in m_verlet)
            {
                verlet.Radius = m_hitRadius;
                verlet.Update(m_center,
                    stiffness,
                    m_dragForce,
                    external,
                    m_colliderList
                    );
            }
        }

        private void OnDrawGizmos()
        {
            if (m_drawGizmo)
            {
                foreach (var verlet in m_verlet)
                {
                    verlet.DrawGizmo(m_center, m_hitRadius, m_gizmoColor);
                }
            }
        }
    }
}
