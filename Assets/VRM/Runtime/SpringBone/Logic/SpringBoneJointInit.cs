using System.Collections.Generic;
using UnityEngine;

namespace VRM.SpringBone
{
    /// <summary>
    /// original from
    /// http://rocketjump.skr.jp/unity3d/109/
    /// 
    /// この型のフィールドはSpringBoneのライフサイクルを通じて不変。
    /// </summary>
    struct SpringBoneJointInit
    {
        public Vector3 BoneAxis;
        public float Length;
        public Quaternion LocalRotation;

        /// <summary>
        /// しっぽの位置から回転を計算する
        /// </summary>
        public Quaternion WorldRotationFromTailPosition(Transform m_transform, Vector3 nextTail)
        {
            var rotation = (m_transform.parent != null ? m_transform.parent.rotation : Quaternion.identity) * LocalRotation;
            return Quaternion.FromToRotation(rotation * BoneAxis,
                       nextTail - m_transform.position) * rotation;
        }

        /// <summary>
        /// Verlet積分で次の位置を計算する
        /// </summary>
        public Vector3 VerletIntegration(float deltaTime, Transform center, Transform m_transform,
            SpringBoneSettings settings, SpringBoneJointState _state)
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
            return nextTail;
        }

        public void DrawGizmo(Transform center, Transform m_transform, SpringBoneSettings settings, Color color, SpringBoneJointState m_state)
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