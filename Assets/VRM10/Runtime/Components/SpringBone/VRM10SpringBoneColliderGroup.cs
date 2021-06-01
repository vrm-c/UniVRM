using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public class VRM10SpringBoneColliderGroup : MonoBehaviour
    {
        [SerializeField]
        public string Name;

        public string GUIName(int i) => $"{i:00}:{Name}";

        [SerializeField]
        public List<VRM10SpringBoneCollider> Colliders = new List<VRM10SpringBoneCollider>();
    }
}