using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM.SpringBone
{
    /// <summary>
    /// original from
    /// http://rocketjump.skr.jp/unity3d/109/
    /// 
    /// この型のフィールドはSpringBoneのライフサイクルを通じて不変。
    /// </summary>
    readonly struct SpringBoneJointInit
    {
        public readonly Vector3 BoneAxis;
        public readonly float Length;
        public readonly Quaternion LocalRotation;

        public SpringBoneJointInit(
        Vector3 boneAxis,
        float length,
        Quaternion localRotation) => (BoneAxis, Length, LocalRotation) = (boneAxis, length, localRotation);

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
        public Vector3 VerletIntegration(float deltaTime, Quaternion parentRotation,
            SpringBoneSettings settings, SpringBoneJointState state, float scalingFactor, Vector3 externalForce)
        {
            var nextTail = state.CurrentTail
                           + (state.CurrentTail - state.PrevTail) * (1.0f - settings.DragForce) // 前フレームの移動を継続する(減衰もあるよ)
                           + parentRotation * LocalRotation * BoneAxis * settings.StiffnessForce * deltaTime * scalingFactor // 親の回転による子ボーンの移動目標
                           + (settings.GravityDir * settings.GravityPower + externalForce) * deltaTime * scalingFactor; // 外力による移動量
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