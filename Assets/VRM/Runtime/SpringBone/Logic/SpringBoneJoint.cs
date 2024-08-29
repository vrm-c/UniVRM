using System.Collections.Generic;
using UnityEngine;

namespace VRM.SpringBone
{
    /// <summary>
    /// original from
    /// http://rocketjump.skr.jp/unity3d/109/
    /// </summary>
    class SpringBoneJoint
    {
        Transform m_transform;
        public Transform Head => m_transform;

        private Vector3 m_boneAxis;
        private Vector3 m_currentTail;

        private readonly float m_length;
        private Vector3 m_localDir;
        private Vector3 m_prevTail;

        public SpringBoneJoint(Transform center, Transform transform, Vector3 localChildPosition)
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

        float m_radius;
        public void SetRadius(float radius)
        {
            m_radius = radius * m_transform.UniformedLossyScale();
        }

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

        protected virtual Vector3 Collision(List<SpringBone.SphereCollider> colliders, Vector3 nextTail)
        {
            foreach (var collider in colliders)
            {
                var r = m_radius + collider.Radius;
                if (Vector3.SqrMagnitude(nextTail - collider.Position) <= (r * r))
                {
                    // ヒット。Colliderの半径方向に押し出す
                    var normal = (nextTail - collider.Position).normalized;
                    var posFromCollider = collider.Position + normal * (m_radius + collider.Radius);
                    // 長さをboneLengthに強制
                    nextTail = m_transform.position + (posFromCollider - m_transform.position).normalized * m_length;
                }
            }
            return nextTail;
        }

        public void DrawGizmo(Transform center, Color color)
        {
            var currentTail = center != null
                    ? center.TransformPoint(m_currentTail)
                    : m_currentTail;
            var prevTail = center != null
                    ? center.TransformPoint(m_prevTail)
                    : m_prevTail;

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(currentTail, prevTail);
            Gizmos.DrawWireSphere(prevTail, m_radius);

            Gizmos.color = color;
            Gizmos.DrawLine(currentTail, m_transform.position);
            Gizmos.DrawWireSphere(currentTail, m_radius);
        }
    }

}