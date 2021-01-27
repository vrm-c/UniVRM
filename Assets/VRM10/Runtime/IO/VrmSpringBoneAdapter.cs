using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Extensions.VRMC_springBone;
using UniGLTF.Extensions.VRMC_node_collider;
using VrmLib;

namespace UniVRM10
{
    public static class SpringBoneAdapter
    {
        public static VRMC_springBone ToGltf(this SpringBoneManager self, List<Node> nodes,
            List<glTFNode> gltfNodes)
        {
            if (self == null)
            {
                return null;
            }

            var springBone = new VRMC_springBone
            {
                Springs = new List<Spring>(),
            };

            //
            // VRMC_node_collider
            //
            foreach (var nodeCollider in self.Springs.SelectMany(x => x.Colliders))
            {
                var index = nodes.IndexOfThrow(nodeCollider.Node);
                var gltfCollider = new VRMC_node_collider
                {
                    Shapes = new List<ColliderShape>(),
                };
                foreach (var y in nodeCollider.Colliders)
                {
                    switch (y.ColliderType)
                    {
                        case VrmSpringBoneColliderTypes.Sphere:
                            {
                                var sphere = new ColliderShapeSphere
                                {
                                    Radius = y.Radius,
                                    Offset = y.Offset.ToFloat3(),
                                };
                                gltfCollider.Shapes.Add(new ColliderShape
                                {
                                    Sphere = sphere,
                                });
                                break;
                            }

                        case VrmSpringBoneColliderTypes.Capsule:
                            {
                                var capsule = new ColliderShapeCapsule
                                {
                                    Radius = y.Radius,
                                    Offset = y.Offset.ToFloat3(),
                                    Tail = y.CapsuleTail.ToFloat3(),
                                };
                                gltfCollider.Shapes.Add(new ColliderShape
                                {
                                    Capsule = capsule,
                                });
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                //
                // add to node.extensions
                //
                UniGLTF.Extensions.VRMC_node_collider.GltfSerializer.SerializeTo(ref gltfNodes[index].extensions, gltfCollider);
            }

            //
            // VRMC_springBone
            //
            foreach (var x in self.Springs)
            {
                var spring = new Spring
                {
                    Name = x.Comment,
                    Colliders = x.Colliders.Select(y => nodes.IndexOfThrow(y.Node)).ToArray(),
                    Joints = new List<SpringBoneJoint>(),
                };

                foreach (var y in x.Joints)
                {
                    spring.Joints.Add(new SpringBoneJoint
                    {
                        HitRadius = y.HitRadius,
                        DragForce = y.DragForce,
                        GravityDir = y.GravityDir.ToFloat3(),
                        GravityPower = y.GravityPower,
                        Stiffness = y.Stiffness,
                    });
                }

                springBone.Springs.Add(spring);
            }

            return springBone;
        }
    }
}
