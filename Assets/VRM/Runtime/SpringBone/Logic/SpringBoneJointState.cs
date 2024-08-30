using System.Collections.Generic;
using UnityEngine;

namespace VRM.SpringBone
{
    struct JointInit
    {
        public Vector3 BoneAxis;
        public float Length;
        public Quaternion LocalRotation;

        public Quaternion ApplyRotation(Transform m_transform, Vector3 nextTail)
        {
            var rotation = (m_transform.parent != null ? m_transform.parent.rotation : Quaternion.identity) * LocalRotation;
            return Quaternion.FromToRotation(rotation * BoneAxis,
                       nextTail - m_transform.position) * rotation;
        }
    };

    struct JointState
    {
        public Vector3 CurrentTail;
        public Vector3 PrevTail;

        public static JointState Init(Transform center, Transform transform, Vector3 localChildPosition)
        {
            var worldChildPosition = transform.TransformPoint(localChildPosition);
            var tail = center != null
                    ? center.InverseTransformPoint(worldChildPosition)
                    : worldChildPosition;
            return new JointState
            {
                CurrentTail = tail,
                PrevTail = tail,
            };
        }

        public static JointState Make(Transform center, Vector3 currentTail, Vector3 nextTail)
        {
            return new JointState
            {
                PrevTail = center != null
                    ? center.InverseTransformPoint(currentTail)
                    : currentTail,
                CurrentTail = center != null
                    ? center.InverseTransformPoint(nextTail)
                    : nextTail,
            };
        }

        public JointState ToWorld(Transform center)
        {
            return new JointState
            {
                CurrentTail = center != null
                    ? center.TransformPoint(CurrentTail)
                    : CurrentTail,
                PrevTail = center != null
                    ? center.TransformPoint(PrevTail)
                    : PrevTail,
            };
        }
    };

    /// <summary>
    /// original from
    /// http://rocketjump.skr.jp/unity3d/109/
    /// 
    /// Joint 状態(初期・フレーム)を管理する
    /// </summary>
    class SpringBoneJointState
    {
        // 不変
        JointInit m_init;
        // フレーム状態
        JointState m_state;

        public SpringBoneJointState(Transform center, Transform transform, Vector3 localChildPosition)
        {
            m_state = JointState.Init(center, transform, localChildPosition);
            m_init = new JointInit
            {
                LocalRotation = transform.localRotation,
                BoneAxis = localChildPosition.normalized,
                Length = localChildPosition.magnitude,
            };
        }

        public void Update(float deltaTime, Transform center, Transform m_transform,
            SpringBoneSettings settings,
            List<SphereCollider> colliders, bool calcScale = false)
        {
            var state = m_state.ToWorld(center);

            // verlet積分で次の位置を計算
            var scaleFactor = calcScale ? m_transform.UniformedLossyScale() : 1.0f;
            var nextTail = state.CurrentTail
                           + (state.CurrentTail - state.PrevTail) * (1.0f - settings.DragForce) // 前フレームの移動を継続する(減衰もあるよ)
                           + (m_transform.parent != null ? m_transform.parent.rotation : Quaternion.identity) * m_init.LocalRotation * m_init.BoneAxis * settings.StiffnessForce * deltaTime * scaleFactor // 親の回転による子ボーンの移動目標
                           + settings.GravityDir * (settings.GravityPower * deltaTime) * scaleFactor; // 外力による移動量

            // 長さをboneLengthに強制
            var position = m_transform.position;
            nextTail = position + (nextTail - position).normalized * m_init.Length;

            // Collisionで移動
            nextTail = Collision(m_transform, settings, colliders, nextTail);

            m_state = JointState.Make(center, currentTail: state.CurrentTail, nextTail: nextTail);

            //回転を適用
            m_transform.rotation = m_init.ApplyRotation(m_transform, nextTail);
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
                    nextTail = m_transform.position + (posFromCollider - m_transform.position).normalized * m_init.Length;
                }
            }
            return nextTail;
        }

        public void DrawGizmo(Transform center, Transform m_transform, SpringBoneSettings settings, Color color)
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
    }
}