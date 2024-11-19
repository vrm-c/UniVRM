using System.Collections.Generic;
using System.Linq;
using UniGLTF;
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

        /// <summary>
        /// VRM-1.0 からのバックポート。
        /// - Runtime 制御用のパラメタである
        /// - シリアライズ対象でない
        /// - World座標系
        /// </summary>
        public Vector3 ExternalForce { get; set; }

        public enum SpringBoneUpdateType
        {
            LateUpdate,
            FixedUpdate,
            Manual,
        }
        [SerializeField] public SpringBoneUpdateType m_updateType = SpringBoneUpdateType.LateUpdate;

        List<Transform> m_rootBonesNonNullUnique = new();
        List<Transform> RootBonesNonNullUnique
        {
            get
            {
                m_rootBonesNonNullUnique.Clear();
                m_rootBonesNonNullUnique.AddRange(RootBones.Where(x => x != null).Distinct());
                return m_rootBonesNonNullUnique;
            }
        }
        SpringBone.SpringBoneSystem m_system = new();

        Dictionary<Transform, int> m_rootCount = new();
        List<Validation> m_validations = new();
        public List<Validation> Validations => m_validations;
        public void OnValidate()
        {
            Validations.Clear();
            m_rootCount.Clear();
            foreach (var root in RootBones)
            {
                if (m_rootCount.TryGetValue(root, out var count))
                {
                    m_rootCount[root] = count + 1;
                }
                else
                {
                    m_rootCount.Add(root, 1);
                }
            }
            foreach (var (k, v) in m_rootCount)
            {
                if (v > 1)
                {
                    Validations.Add(Validation.Error($"Duplicate rootBone: {k} => {v}", ValidationContext.Create(k)));
                }
            }
        }

        void Awake()
        {
            Setup();
        }

        SpringBone.SceneInfo Scene => new(
            rootBones: RootBonesNonNullUnique,
            center: m_center,
            colliderGroups: ColliderGroups,
            externalForce: ExternalForce);

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
                m_system.Setup(Scene, force);
            }
        }

        public void ReinitializeRotation()
        {
            m_system.ReinitializeRotation(Scene);
        }

        public void SetModelLevel(UniGLTF.SpringBoneJobs.Blittables.BlittableModelLevel modelSettings)
        {
            UseRuntimeScalingSupport = modelSettings.SupportsScalingAtRuntime;
            ExternalForce = modelSettings.ExternalForce;
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
            if (Application.isPlaying && m_updateType != SpringBoneUpdateType.Manual)
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
