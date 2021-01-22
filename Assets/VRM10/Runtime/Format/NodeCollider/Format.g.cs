// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace UniGLTF.Extensions.VRMC_node_collider
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
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        public ColliderShapeSphere Sphere;

        public ColliderShapeCapsule Capsule;
    }

    public class VRMC_node_collider
    {
        public const string ExtensionName = "VRMC_node_collider";
        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);

        // Dictionary object with extension-specific objects.
        public glTFExtension Extensions;

        // Application-specific data.
        public glTFExtension Extras;

        public List<ColliderShape> Shapes;
    }
}
