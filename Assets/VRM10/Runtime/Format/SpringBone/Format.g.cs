// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_springBone
{

    public class ColliderShapeSphere
    {
        // The sphere center. vector3
        public float[] Offset;

        // The sphere radius
        public float? Radius;
    }

    public class ColliderShapeCapsule
    {
        // The capsule head. vector3
        public float[] Offset;

        // The capsule radius
        public float? Radius;

        // The capsule tail. vector3
        public float[] Tail;
    }

    public class ColliderShape
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        public ColliderShapeSphere Sphere;

        public ColliderShapeCapsule Capsule;
    }

    public class Collider
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The node index.
        public int? Node;

        public ColliderShape Shape;
    }

    public class ColliderGroup
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Name of the ColliderGroup
        public string Name;

        // An array of colliders.
        public int[] Colliders;
    }

    public class SpringBoneJoint
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The node index.
        public int? Node;

        // The radius of spring sphere.
        public float? HitRadius;

        // The force to return to the initial pose.
        public float? Stiffness;

        // Gravitational acceleration.
        public float? GravityPower;

        // The direction of gravity. A gravity other than downward direction also works.
        public float[] GravityDir;

        // Air resistance. Deceleration force.
        public float? DragForce;
    }

    public class Spring
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Name of the Spring
        public string Name;

        // Joints of the spring. Except for the first element, a previous joint of the array must be an ancestor of the joint.
        public List<SpringBoneJoint> Joints;

        // Indices of ColliderGroups that detect collision with this spring.
        public int[] ColliderGroups;

        // An index of node which is used as a root of center space.
        public int? Center;
    }

    public class VRMC_springBone
    {
        public const string ExtensionName = "VRMC_springBone";

        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specification version of VRMC_springBone
        public string SpecVersion;

        // An array of colliders.
        public List<Collider> Colliders;

        // An array of colliderGroups.
        public List<ColliderGroup> ColliderGroups;

        // An array of springs.
        public List<Spring> Springs;
    }
}
