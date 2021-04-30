using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// VRMC_node_collider
    /// </summary>
    [AddComponentMenu("VRM10/VRM10SpringBoneColliderGroup")]
    public class VRM10SpringBoneColliderGroup : MonoBehaviour
    {
        [SerializeField]
        public List<VRM10SpringBoneCollider> Colliders = new List<VRM10SpringBoneCollider>();
    }
}
