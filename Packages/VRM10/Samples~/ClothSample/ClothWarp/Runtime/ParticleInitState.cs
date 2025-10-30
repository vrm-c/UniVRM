using System;
using SphereTriangle;
using UnityEngine;

namespace UniVRM10.ClothWarp
{
    [Serializable]
    public struct ParticleInitState
    {
        public readonly int Index;

        [SerializeField]
        public Vector3 LocalPosition;
        [SerializeField]
        public Quaternion LocalRotation;

        [SerializeField]
        public Vector3 BoneAxis;

        [SerializeField]
        public float StrandLength;

        [SerializeField]
        public float Radius;

        // 0 は移動しない固定(回転はしてもよい)
        // TODO: force / mass => accelaration
        public float Mass;

        public ParticleInitState(int index, Transform t, float radius, float mass)
        {
            Index = index;
            LocalPosition = t.localPosition;
            LocalRotation = t.localRotation;
            StrandLength = LocalPosition.magnitude;
            BoneAxis = LocalPosition.normalized;
            Radius = radius;
            Mass = mass;
        }

        public override string ToString()
        {
            return $"create Particle: {LocalPosition}, {BoneAxis}, {StrandLength}";
        }
    }
}