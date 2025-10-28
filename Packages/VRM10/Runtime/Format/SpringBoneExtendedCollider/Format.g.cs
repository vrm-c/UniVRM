// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_springBone_extended_collider
{

    public class ExtendedColliderShapeSphere
    {
        // The offset of the sphere from the origin in the local space.
        public float[] Offset;

        // The radius of the sphere.
        public float? Radius;

        // If true, the collider prevents spring bones from going outside of the sphere instead.
        public bool? Inside;
    }

    public class ExtendedColliderShapeCapsule
    {
        // The offset of the capsule head from the origin in the local space.
        public float[] Offset;

        // The radius of the capsule.
        public float? Radius;

        // The offset of the capsule tail from the origin in the local space.
        public float[] Tail;

        // If true, the collider prevents spring bones from going outside of the capsule instead.
        public bool? Inside;
    }

    public class ExtendedColliderShapePlane
    {
        // The offset of the plane from the origin in the local space.
        public float[] Offset;

        // The normal of the plane in the local space. Must be normalized.
        public float[] Normal;
    }

    public class ExtendedColliderShape
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        public ExtendedColliderShapeSphere Sphere;

        public ExtendedColliderShapeCapsule Capsule;

        public ExtendedColliderShapePlane Plane;
    }

    public class VRMC_springBone_extended_collider
    {
        public const string ExtensionName = "VRMC_springBone_extended_collider";

        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specification version of VRMC_springBone_extended_collider.
        public string SpecVersion;

        // The shape of the collider.
        public ExtendedColliderShape Shape;
    }
}
