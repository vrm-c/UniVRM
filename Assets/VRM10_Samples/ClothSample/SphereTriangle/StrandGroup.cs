using System;
using System.Collections.Generic;
using UnityEngine;

namespace SphereTriangle
{
    public enum StrandConnectionType
    {
        Cloth,
        ClothLoop,
        Strand,
    }

    [System.Flags]
    public enum CollisionGroupMask : uint
    {
        None = 0,
        Group01 = 0x00000001,
        Group02 = 0x00000002,
        Group03 = 0x00000004,
        Group04 = 0x00000008,
        Group05 = 0x00000010,
        Group06 = 0x00000020,
        Group07 = 0x00000040,
        Group08 = 0x00000080,
        Group09 = 0x00000100,
        Group10 = 0x00000200,
        Group11 = 0x00000400,
        Group12 = 0x00000800,
        Group13 = 0x00001000,
        Group14 = 0x00002000,
        Group15 = 0x00004000,
        Group16 = 0x00008000,
        Group17 = 0x00010000,
        Group18 = 0x00020000,
        Group19 = 0x00040000,
        Group20 = 0x00080000,
        Group21 = 0x00100000,
        Group22 = 0x00200000,
        Group23 = 0x00400000,
        Group24 = 0x00800000,
        Group25 = 0x01000000,
        Group26 = 0x02000000,
        Group27 = 0x04000000,
        Group28 = 0x08000000,
        Group29 = 0x10000000,
        Group30 = 0x20000000,
        Group31 = 0x40000000,
        Group32 = 0x80000000,
        All = uint.MaxValue,
    }

    [Serializable]
    public class StrandGroup
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public CollisionGroupMask CollisionMask;

        [SerializeField]
        public StrandConnectionType Connection;

        [SerializeField]
        public List<Transform> Roots = new List<Transform>();

        [SerializeField]
        [Range(0.001f, 0.5f)]
        public float DefaultStrandRaius = 0.05f;

    }
}