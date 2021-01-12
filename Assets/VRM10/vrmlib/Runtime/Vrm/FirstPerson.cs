using System.Collections.Generic;
using System.Numerics;

namespace VrmLib
{
    public enum FirstPersonMeshType
    {
        Auto, // Create headlessModel
        Both, // Default layer
        ThirdPersonOnly,
        FirstPersonOnly,
    }

    public class FirstPersonMeshAnnotation
    {
        public Node Node;

        public readonly FirstPersonMeshType FirstPersonFlag;

        public FirstPersonMeshAnnotation(Node node, FirstPersonMeshType flag)
        {
            Node = node;
            FirstPersonFlag = flag;
        }
    }

    public class FirstPerson
    {
        public readonly List<FirstPersonMeshAnnotation> Annotations = new List<FirstPersonMeshAnnotation>();
    }
}
