// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_springBone
{

    public class SpringBoneJoint
    {
        public int? Node;
        
        // The radius of spring sphere
        public float? HitRadius;

        // The force to return to the initial pose
        public float? Stiffness;

        // Gravitational acceleration
        public float? GravityPower;

        // The direction of gravity. A gravity other than downward direction also works.
        public float[] GravityDir;

        // Air resistance. Deceleration force
        public float? DragForce;
    }

    public class Spring
    {
        // Name of the Spring
        public string Name;

        // Joints in this spring. Except for the first element, the parent of the element must be the previous element of the array
        public List<SpringBoneJoint> Joints;

        // Colliders that detect collision with nodes start from springRoot
        public int[] Colliders;
    }

    public class VRMC_springBone
    {
        public const string ExtensionName = "VRMC_springBone";
        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);

        // An array of springs.
        public List<Spring> Springs;
    }
}
