using System;
using System.Collections.Generic;
using UnityEngine;
using UniVRM10;

namespace SphereTriangle
{
    [Serializable]
    public class ColliderGroup
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public List<VRM10SpringBoneCollider> Colliders = new();
    }
}