using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public class SpringBoneProcessor
    {
        List<SpringBoneLogic> m_logics = new List<SpringBoneLogic>();
        Dictionary<Transform, Quaternion> m_initialLocalRotationMap;


        public void ResetSpringBone()
        {
            foreach (var verlet in m_logics)
            {
                verlet.Head.localRotation = Quaternion.identity;
            }
        }

        public void SetupRecursive(Transform parent, Transform center)
        {
            if (parent.childCount == 0)
            {
                // 末端に追加のスプリングを付加する
                var delta = parent.position - parent.parent.position;
                var childPosition = parent.position + delta.normalized * 0.07f;
                m_logics.Add(new SpringBoneLogic(center, parent, parent.worldToLocalMatrix.MultiplyPoint(childPosition)));
            }
            else
            {
                // 最初の子ボーンを尻尾としてスプリングを付加する
                var firstChild = parent.GetChild(0);
                var localPosition = firstChild.localPosition;
                var scale = firstChild.lossyScale;
                m_logics.Add(new SpringBoneLogic(center, parent,
                    new Vector3(
                        localPosition.x * scale.x,
                        localPosition.y * scale.y,
                        localPosition.z * scale.z
                        )))
                    ;
            }

            foreach (Transform child in parent)
            {
                SetupRecursive(child, center);
            }
        }

        void Setup(List<Transform> RootBones, Transform m_center, bool force = false)
        {
            if (force || m_initialLocalRotationMap == null)
            {
                m_initialLocalRotationMap = new Dictionary<Transform, Quaternion>();
            }
            else
            {
                // restore initial rotation
                foreach (var kv in m_initialLocalRotationMap)
                {
                    kv.Key.localRotation = kv.Value;
                }
                m_initialLocalRotationMap.Clear();
            }
            m_logics.Clear();

            foreach (var transform in RootBones)
            {
                if (transform != null)
                {
                    foreach (var x in transform.Traverse())
                    {
                        // backup initial rotation
                        m_initialLocalRotationMap[x] = x.localRotation;
                    }

                    SetupRecursive(transform, m_center);
                }
            }
        }

        public void Update(List<Transform> RootBones, List<SpringBoneLogic.InternalCollider> m_colliderList,
            float stiffness, float m_dragForce, Vector3 external,
             float m_hitRadius, Transform m_center)
        {
            if (m_logics == null || m_logics.Count == 0)
            {
                Setup(RootBones, m_center);
            }

            foreach (var verlet in m_logics)
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

        public void DrawGizmos(Transform m_center, float m_hitRadius, Color m_gizmoColor)
        {
            foreach (var verlet in m_logics)
            {
                verlet.DrawGizmo(m_center, m_hitRadius, m_gizmoColor);
            }
        }
    }
}
