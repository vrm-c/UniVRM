using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] public string m_comment;
        [SerializeField] private Color m_gizmoColor = Color.yellow;
        [SerializeField] public float m_stiffnessForce = 1.0f;
        [SerializeField] public float m_gravityPower;
        [SerializeField] public Vector3 m_gravityDir = new Vector3(0, -1.0f, 0);
        [SerializeField][Range(0, 1)] public float m_dragForce = 0.4f;
        [SerializeField] public Transform m_center;
        [SerializeField] public List<Transform> RootBones = new List<Transform>();
        [SerializeField] public float m_hitRadius = 0.02f;
        [SerializeField] public VRMSpringBoneColliderGroup[] ColliderGroups;

        public enum SpringBoneUpdateType
        {
            LateUpdate,
            FixedUpdate,
            Manual,
        }
        [SerializeField] public SpringBoneUpdateType m_updateType = SpringBoneUpdateType.LateUpdate;

        SpringBone.SpringBoneSystem m_system = new();

        void Awake()
        {
            Setup();
        }

        SpringBone.SceneInfo Scene => new SpringBone.SceneInfo { RootBones = RootBones, Center = m_center };
        SpringBone.SpringBoneSettings Settings => new SpringBone.SpringBoneSettings
        {
            StiffnessForce = m_stiffnessForce,
            GravityDir = m_gravityDir,
            GravityPower = m_gravityPower,
            HitRadius = m_hitRadius,
            DragForce = m_dragForce,
        };

        [ContextMenu("Reset bones")]
        public void Setup(bool force = false)
        {
            if (RootBones != null)
            {
                m_system.Setup(Scene, force);
            }
        }

        void LateUpdate()
        {
            if (m_updateType == SpringBoneUpdateType.LateUpdate)
            {
                m_system.UpdateProcess(Time.deltaTime, Scene, Settings);
            }
        }

        void FixedUpdate()
        {
            if (m_updateType == SpringBoneUpdateType.FixedUpdate)
            {
                m_system.UpdateProcess(Time.fixedDeltaTime, Scene, Settings);
            }
        }

        public void ManualUpdate(float deltaTime)
        {
            if (m_updateType != SpringBoneUpdateType.Manual)
            {
                throw new System.ArgumentException("require SpringBoneUpdateType.Manual");
            }
            m_system.UpdateProcess(deltaTime, Scene, Settings);
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                m_system.PlayingGizmo(m_center, Settings, m_gizmoColor);
            }
            else
            {
                // Editor
                Gizmos.color = m_gizmoColor;
                foreach (var root in RootBones)
                {
                    if (root != null)
                    {
                        m_system.EditorGizmo(root.transform, m_hitRadius);
                    }
                }
            }
        }
    }
}
