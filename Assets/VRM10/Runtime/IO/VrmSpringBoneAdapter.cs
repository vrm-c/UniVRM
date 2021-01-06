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
        public static SpringSetting ToGltf(this SpringBone self, List<Node> nodes)
        {
            var setting = new SpringSetting
            {
                DragForce = self.DragForce,
                GravityPower = self.GravityPower,
                Stiffness = self.Stiffness,
                GravityDir = self.GravityDir.ToFloat3(),
            };
            return setting;
        }

        public static VRMC_springBone ToGltf(this SpringBoneManager self, List<Node> nodes,
            List<glTFNode> gltfNodes)
        {
            if (self == null)
            {
                return null;
            }

            var springBone = new VRMC_springBone();

            //
            // VRMC_node_collider
            //
            foreach (var x in self.Colliders)
            {
                var index = nodes.IndexOfThrow(x.Node);
                var collider = new VRMC_node_collider();
                foreach (var y in x.Colliders)
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
                                collider.Shapes.Add(new ColliderShape
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
                                collider.Shapes.Add(new ColliderShape
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
                UniGLTF.Extensions.VRMC_node_collider.GltfSerializer.SerializeTo(ref gltfNodes[index].extensions, collider);
            }

            //
            // VRMC_springBone
            //
            foreach (var x in self.Springs)
            {
                var settingIndex = springBone.Settings.Count;
                springBone.Settings.Add(x.ToGltf(nodes));
                foreach (var bone in x.Bones)
                {
                    var spring = new Spring
                    {
                        Name = x.Comment,
                        HitRadius = x.HitRadius,
                        SpringRoot = nodes.IndexOfThrow(bone),
                        Setting = settingIndex,
                        Colliders = x.Colliders.Select(y => nodes.IndexOfThrow(y.Node)).ToArray(),
                    };
                    springBone.Springs.Add(spring);
                }
            }

            return springBone;
        }
    }
}
