using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// VRMC_node_collider
    /// </summary>
    [DisallowMultipleComponent]

    [AddComponentMenu("VRM10/VRM10SpringBoneColliderGroup")]
    public class VRM10SpringBoneColliderGroup : MonoBehaviour
    {
        [SerializeField]
        List<VRM10SpringBoneCollider> m_colliders = new List<VRM10SpringBoneCollider>();

        public IEnumerable<VRM10SpringBoneCollider> Colliders
        {
            get
            {
                return m_colliders.Where(x => x != null);
            }
            set
            {
                m_colliders = value.ToList();
                OnValidate();
            }
        }

        public void AddCollider(VRM10SpringBoneCollider collider)
        {
            if (collider == null)
            {
                Debug.LogWarning("null collider");
                return;
            }

            if (m_colliders == null)
            {
                m_colliders = new List<VRM10SpringBoneCollider>();
            }
            m_colliders.Add(collider);
        }

        void OnValidate()
        {
            if (m_colliders.Any(x => x == null))
            {
                Debug.LogWarning($"{this} remove null");
                m_colliders = m_colliders.Where(x => x != null).ToList();
            }
        }
    }
}
