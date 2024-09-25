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

        /// <summary>
        /// - アプリケーション開発者用のパラメタである
        /// - Runtime 制御用のパラメタである
        /// - シリアライズ対象でない
        /// - true にすることで、モデルをスケーリングしたときも SpringBone の見た目上の動き(角速度)がなるべく保たれるようになる
        /// - Non-Uniform scaling 下における動作は保証しない        
        /// </summary>
        public bool UseRuntimeScalingSupport { get; set; }

#if VRM0X_SPRING_UPDATE_SELF
        public enum SpringBoneUpdateType
        {
            LateUpdate,
            FixedUpdate,
            Manual,
        }
        [SerializeField] public SpringBoneUpdateType m_updateType = SpringBoneUpdateType.LateUpdate;
#endif

        SpringBone.SpringBoneSystem m_springSystem;
        SpringBone.SpringBoneSystem SpringSystem
        {
            get
            {
                if (m_springSystem == null)
                {
                    m_springSystem = new();
                }
                return m_springSystem;
            }
        }

        void Awake()
        {
            Setup();
        }

        SpringBone.SceneInfo Scene => new(
            rootBones: RootBones,
            center: m_center,
            colliderGroups: ColliderGroups);
        SpringBone.SpringBoneSettings Settings => new
        (
            stiffnessForce: m_stiffnessForce,
            dragForce: m_dragForce,
            gravityDir: m_gravityDir,
            gravityPower: m_gravityPower,
            hitRadius: m_hitRadius,
            useRuntimeScalingSupport: UseRuntimeScalingSupport);

        [ContextMenu("Reset bones")]
        public void Setup(bool force = false)
        {
            if (RootBones != null)
            {
                SpringSystem.Setup(Scene, force);
            }
        }

#if VRM0X_SPRING_UPDATE_SELF
        void LateUpdate()
        {
            if (m_updateType == SpringBoneUpdateType.LateUpdate)
            {
                SpringSystem.UpdateProcess(Time.deltaTime, Scene, Settings);
            }
        }

        void FixedUpdate()
        {
            if (m_updateType == SpringBoneUpdateType.FixedUpdate)
            {
                SpringSystem.UpdateProcess(Time.fixedDeltaTime, Scene, Settings);
            }
        }
#endif

        public void ManualUpdate(float deltaTime)
        {
            SpringSystem.UpdateProcess(deltaTime, Scene, Settings);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = m_gizmoColor;
            foreach (var root in RootBones)
            {
                if (root != null)
                {
                    SpringSystem.EditorGizmo(root.transform, m_hitRadius);
                }
            }
        }
    }
}
