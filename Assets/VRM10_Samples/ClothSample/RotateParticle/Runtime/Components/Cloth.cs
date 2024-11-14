using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/Cloth")]
    [DisallowMultipleComponent]
    public class Cloth : MonoBehaviour
    {
        [SerializeField]
        public bool Loop = false;

        [SerializeField]
        public List<Warp> Warps = new();
    }
}