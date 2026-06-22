// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;


namespace UniGLTF.Extensions.VRMC_vrm_expressions_node_transform
{

    public class NodeTransformBind
    {
        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // The node index.
        public int? Node;

        // The node's unit quaternion rotation in the order (x, y, z, w), where w is the scalar.
        public float[] Rotation;

        // The node's non-uniform scale, given as the scaling factors along the x, y, and z axes.
        public float[] Scale;

        // The node's translation along the x, y, and z axes.
        public float[] Translation;
    }

    public class VRMC_vrm_expressions_node_transform
    {
        public const string ExtensionName = "VRMC_vrm_expressions_node_transform";

        // Dictionary object with extension-specific objects.
        public object Extensions;

        // Application-specific data.
        public object Extras;

        // Specify a node transform
        public List<NodeTransformBind> NodeTransformBinds;
    }
}
