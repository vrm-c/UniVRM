// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_springBone_limit
{

    public class ConeLimit
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The angle of the cone limit in radians. If the angle is set to π or greater, the angle will be interpreted as π by the implementation. When the angle is set to π, the cone shape becomes a sphere.
        public float? Angle;

        // The rotation from the default orientation of the cone limit. The rotation is represented as a quaternion (x, y, z, w), where w is the scalar.
        public float[] Rotation;
    }

    public class HingeLimit
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The angle of the hinge limit in radians. If the angle is set to π or greater, the angle will be interpreted as π by the implementation. When the angle is set to π, the hinge shape becomes a full disc.
        public float? Angle;

        // The rotation from the default orientation of the hinge limit. The rotation is represented as a quaternion (x, y, z, w), where w is the scalar.
        public float[] Rotation;
    }

    public class SphericalLimit
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The phi angle of the spherical limit in radians. If the phi angle is set to π or greater, the angle will be interpreted as π by the implementation.
        public float? Pitch;

        // The theta angle of the spherical limit in radians. If the theta angle is set to π/2 or greater, the angle will be interpreted as π/2 by the implementation.
        public float? Yaw;

        // The rotation from the default orientation of the spherical limit. The rotation is represented as a quaternion (x, y, z, w), where w is the scalar.
        public float[] Rotation;
    }

    public class Limit
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // A limit that restricts the orientation of the spring in a cone shape.
        public ConeLimit Cone;

        // A limit that restricts the orientation of the spring in a hinge shape.
        public HingeLimit Hinge;

        // A limit that restricts the orientation of the spring in a spherical coordinate shape.
        public SphericalLimit Spherical;
    }

    public class VRMC_springBone_limit
    {
        public const string ExtensionName = "VRMC_springBone_limit";

        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specification version of VRMC_springBone_limit.
        public string SpecVersion;

        // Describes a limit apply to the spring. Either cone, hinge, or spherical must be present.
        public Limit Limit;
    }
}
