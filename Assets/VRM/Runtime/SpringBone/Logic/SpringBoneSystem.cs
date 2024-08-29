using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRM.SpringBone
{
    /// <summary>
    /// 同じ設定のスプリングをまとめて処理する。
    /// 
    /// root o-o-o-x tail
    /// 
    /// [vrm0] tail は 7cm 遠にダミーの joint があるようにふるまう。
    /// 
    /// </summary>
    class SpringBoneSystem
    {
        Dictionary<Transform, Quaternion> m_initialLocalRotationMap;
        List<SpringBoneJoint> m_verlet = new();
        List<SphereCollider> m_colliders = new();

        public void SetLocalRotationsIdentity()
        {
            foreach (var verlet in m_verlet) verlet.Head.localRotation = Quaternion.identity;
        }

        public void Setup(SceneInfo scene, bool force)
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

            foreach (var go in scene.RootBones)
            {
                if (go != null)
                {
                    foreach (var x in go.transform.GetComponentsInChildren<Transform>(true)) m_initialLocalRotationMap[x] = x.localRotation;

                    SetupRecursive(scene.m_center, go);
                }
            }
        }

        private static IEnumerable<Transform> GetChildren(Transform parent)
        {
            for (var i = 0; i < parent.childCount; ++i) yield return parent.GetChild(i);
        }

        private void SetupRecursive(Transform center, Transform parent)
        {
            Vector3 localPosition = default;
            Vector3 scale = default;
            if (parent.childCount == 0)
            {
                // 子ノードが無い。7cm 固定
                var delta = parent.position - parent.parent.position;
                var childPosition = parent.position + delta.normalized * 0.07f * parent.UniformedLossyScale();
                localPosition = parent.worldToLocalMatrix.MultiplyPoint(childPosition); // cancel scale
                scale = parent.lossyScale;
            }
            else
            {
                var firstChild = GetChildren(parent).First();
                localPosition = firstChild.localPosition;
                scale = firstChild.lossyScale;
            }
            m_verlet.Add(new SpringBone.SpringBoneJoint(center, parent,
                    new Vector3(
                        localPosition.x * scale.x,
                        localPosition.y * scale.y,
                        localPosition.z * scale.z
                    )))
                ;

            foreach (Transform child in parent) SetupRecursive(center, child);
        }


        public void UpdateProcess(float deltaTime,
            SceneInfo scene,
            SpringBoneSettings settings
            )
        {
            if (m_verlet == null || m_verlet.Count == 0)
            {
                if (scene.RootBones == null) return;
                Setup(scene, false);
            }

            m_colliders.Clear();
            if (scene.ColliderGroups != null)
            {
                foreach (var group in scene.ColliderGroups)
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

            var stiffness = settings.m_stiffnessForce * deltaTime;
            var external = settings.m_gravityDir * (settings.m_gravityPower * deltaTime);

            foreach (var verlet in m_verlet)
            {
                verlet.SetRadius(settings.m_hitRadius);
                verlet.Update(scene.m_center,
                    stiffness,
                    settings.m_dragForce,
                    external,
                    m_colliders
                );
            }
        }

        public void PlayingGizmo(Transform m_center, Color m_gizmoColor)
        {
            foreach (var verlet in m_verlet)
            {
                verlet.DrawGizmo(m_center, m_gizmoColor);
            }
        }

        public void EditorGizmo(Transform head, float m_hitRadius)
        {
            Vector3 childPosition;
            Vector3 scale;
            if (head.childCount == 0)
            {
                // 子ノードが無い。7cm 固定
                var delta = head.position - head.parent.position;
                childPosition = head.position + delta.normalized * 0.07f * head.UniformedLossyScale();
                scale = head.lossyScale;
            }
            else
            {
                var firstChild = GetChildren(head).First();
                childPosition = firstChild.position;
                scale = firstChild.lossyScale;
            }

            Gizmos.DrawLine(head.position, childPosition);
            Gizmos.DrawWireSphere(childPosition, m_hitRadius * scale.x);

            foreach (Transform child in head) EditorGizmo(child, m_hitRadius);
        }
    }
}