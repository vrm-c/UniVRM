using System;
using System.Collections.Generic;
using UnityEngine;

namespace SphereTriangle
{
    [Serializable]
    public class ColliderGroup
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public List<SphereCapsuleCollider> Colliders = new List<SphereCapsuleCollider>();
    }
}