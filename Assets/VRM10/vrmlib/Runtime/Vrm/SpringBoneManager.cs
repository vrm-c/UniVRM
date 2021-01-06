using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public enum VrmSpringBoneColliderTypes
    {
        Sphere,
        Capsule,
    }

    public struct VrmSpringBoneCollider
    {
        public readonly VrmSpringBoneColliderTypes ColliderType;
        public readonly Vector3 Offset;
        public readonly float Radius;
        public readonly Vector3 CapsuleTail;

        VrmSpringBoneCollider(VrmSpringBoneColliderTypes type, Vector3 offset, float radius, Vector3 tail)
        {
            ColliderType = type;
            Offset = offset;
            Radius = radius;
            CapsuleTail = tail;
        }

        public static VrmSpringBoneCollider CreateSphere(Vector3 offset, float radius)
        {
            return new VrmSpringBoneCollider(VrmSpringBoneColliderTypes.Sphere, offset, radius, Vector3.Zero);
        }

        public static VrmSpringBoneCollider CreateCapsule(Vector3 offset, float radius, Vector3 tail)
        {
            return new VrmSpringBoneCollider(VrmSpringBoneColliderTypes.Capsule, offset, radius, tail);
        }
    }

    public class SpringBoneColliderGroup
    {
        public readonly Node Node;

        public readonly List<VrmSpringBoneCollider> Colliders;

        public SpringBoneColliderGroup(Node node, IEnumerable<VrmSpringBoneCollider> colliders)
        {
            Node = node;
            Colliders = colliders.ToList();
        }
    }

    public class SpringBone
    {
        public const string ExtensionName = "VRMC_springBone";
        public readonly List<Node> Bones = new List<Node>();
        public Node Origin;

        public readonly List<SpringBoneColliderGroup> Colliders = new List<SpringBoneColliderGroup>();

        public string Comment = "";

        public float DragForce;

        public Vector3 GravityDir;

        public float GravityPower;

        public float HitRadius;

        public float Stiffness;
    }

    public class SpringBoneManager
    {
        public readonly List<SpringBone> Springs = new List<SpringBone>();
        public readonly List<SpringBoneColliderGroup> Colliders = new List<SpringBoneColliderGroup>();
    }
}