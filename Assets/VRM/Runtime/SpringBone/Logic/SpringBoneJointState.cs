using System.Collections.Generic;
using UnityEngine;

namespace VRM.SpringBone
{
    /// <summary>
    /// original from
    /// http://rocketjump.skr.jp/unity3d/109/
    /// 
    /// Joint 状態(初期・フレーム)を管理する
    /// </summary>
    struct SpringBoneJointInit
    {
        public Vector3 BoneAxis;
        public float Length;
        public Quaternion LocalRotation;

        public Quaternion CalcRotation(Transform m_transform, Vector3 nextTail)
        {
            var rotation = (m_transform.parent != null ? m_transform.parent.rotation : Quaternion.identity) * LocalRotation;
            return Quaternion.FromToRotation(rotation * BoneAxis,
                       nextTail - m_transform.position) * rotation;
        }

        public JointState Update(float deltaTime, Transform center, Transform m_transform,
            SpringBoneSettings settings,
            List<SphereCollider> colliders, JointState _state)
        {
            var state = _state.ToWorld(center);

            // verlet積分で次の位置を計算
            var nextTail = state.CurrentTail
                           + (state.CurrentTail - state.PrevTail) * (1.0f - settings.DragForce) // 前フレームの移動を継続する(減衰もあるよ)
                           + (m_transform.parent != null ? m_transform.parent.rotation : Quaternion.identity) * LocalRotation * BoneAxis * settings.StiffnessForce * deltaTime // 親の回転による子ボーンの移動目標
                           + settings.GravityDir * (settings.GravityPower * deltaTime); // 外力による移動量

            // 長さをboneLengthに強制
            var position = m_transform.position;
            nextTail = position + (nextTail - position).normalized * Length;

            // Collisionで移動
            nextTail = Collision(m_transform, settings, colliders, nextTail);

            return JointState.Make(center, currentTail: state.CurrentTail, nextTail: nextTail);
        }

        Vector3 Collision(Transform m_transform, SpringBoneSettings settings, List<SphereCollider> colliders, Vector3 nextTail)
        {
            foreach (var collider in colliders)
            {
                var m_radius = settings.HitRadius * m_transform.UniformedLossyScale();
                var r = m_radius + collider.Radius;
                if (Vector3.SqrMagnitude(nextTail - collider.Position) <= (r * r))
                {
                    // ヒット。Colliderの半径方向に押し出す
                    var normal = (nextTail - collider.Position).normalized;
                    var posFromCollider = collider.Position + normal * (m_radius + collider.Radius);
                    // 長さをboneLengthに強制
                    nextTail = m_transform.position + (posFromCollider - m_transform.position).normalized * Length;
                }
            }
            return nextTail;
        }

        public void DrawGizmo(Transform center, Transform m_transform, SpringBoneSettings settings, Color color, JointState m_state)
        {
            var state = m_state.ToWorld(center);
            var m_radius = settings.HitRadius * m_transform.UniformedLossyScale();

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(state.CurrentTail, state.PrevTail);
            Gizmos.DrawWireSphere(state.PrevTail, m_radius);

            Gizmos.color = color;
            Gizmos.DrawLine(state.CurrentTail, m_transform.position);
            Gizmos.DrawWireSphere(state.CurrentTail, m_radius);
        }

    };
}