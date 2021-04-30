using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniVRM10
{
    public static class MigrationVrmSpringBone
    {
        static IEnumerable<UniGLTF.glTFNode> EnumJoint(List<UniGLTF.glTFNode> nodes, UniGLTF.glTFNode node)
        {
            yield return node;

            if (node.children != null && node.children.Length > 0)
            {
                foreach (var x in EnumJoint(nodes, nodes[node.children[0]]))
                {
                    yield return x;
                }
            }
        }

        public static UniGLTF.Extensions.VRMC_springBone.VRMC_springBone Migrate(UniGLTF.glTF gltf, JsonNode sa)
        {
            var colliderNodes = new List<int>();

            var springBone = new UniGLTF.Extensions.VRMC_springBone.VRMC_springBone
            {
                ColliderGroups = new List<UniGLTF.Extensions.VRMC_springBone.ColliderGroup>(),
                Springs = new List<UniGLTF.Extensions.VRMC_springBone.Spring>(),
            };

            foreach (var x in sa["colliderGroups"].ArrayItems())
            {
                var node = x["node"].GetInt32();
                colliderNodes.Add(node);

                var colliderGroup = new UniGLTF.Extensions.VRMC_springBone.ColliderGroup()
                {
                    Colliders = new List<UniGLTF.Extensions.VRMC_springBone.Collider>(),
                };
                springBone.ColliderGroups.Add(colliderGroup);

                // {
                //   "node": 14,
                //   "colliders": [
                //     {
                //       "offset": {
                //         "x": 0.025884293,
                //         "y": -0.120000005,
                //         "z": 0
                //       },
                //       "radius": 0.05
                //     },
                //     {
                //       "offset": {
                //         "x": -0.02588429,
                //         "y": -0.120000005,
                //         "z": 0
                //       },
                //       "radius": 0.05
                //     },
                //     {
                //       "offset": {
                //         "x": 0,
                //         "y": -0.0220816135,
                //         "z": 0
                //       },
                //       "radius": 0.08
                //     }
                //   ]
                // },
                foreach (var y in x["colliders"].ArrayItems())
                {
                    colliderGroup.Colliders.Add(new UniGLTF.Extensions.VRMC_springBone.Collider
                    {
                        Node = x["node"].GetInt32(),
                        Shape = new UniGLTF.Extensions.VRMC_springBone.ColliderShape
                        {
                            Sphere = new UniGLTF.Extensions.VRMC_springBone.ColliderShapeSphere
                            {
                                Offset = MigrateVector3.Migrate(y["offset"]),
                                Radius = y["radius"].GetSingle()
                            }
                        }
                    });
                }
            }

            foreach (var x in sa["boneGroups"].ArrayItems())
            {
                // {
                //   "comment": "",
                //   "stiffiness": 2,
                //   "gravityPower": 0,
                //   "gravityDir": {
                //     "x": 0,
                //     "y": -1,
                //     "z": 0
                //   },
                //   "dragForce": 0.7,
                //   "center": -1,
                //   "hitRadius": 0.02,
                //   "bones": [
                //     97,
                //     99,
                //     101,
                //     113,
                //     114
                //   ],
                //   "colliderGroups": [
                //     3,
                //     4,
                //     5
                //   ]
                // },
                foreach (var y in x["bones"].ArrayItems())
                {
                    var comment = x.GetObjectValueOrDefault("comment", "");
                    var spring = new UniGLTF.Extensions.VRMC_springBone.Spring
                    {
                        Name = comment,
                        ColliderGroups = x["colliderGroups"].ArrayItems().Select(z => z.GetInt32()).ToArray(),
                        Joints = new List<UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint>(),
                    };

                    foreach (var z in EnumJoint(gltf.nodes, gltf.nodes[y.GetInt32()]))
                    {
                        spring.Joints.Add(new UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint
                        {
                            Node = gltf.nodes.IndexOf(z),
                            DragForce = x["dragForce"].GetSingle(),
                            GravityDir = MigrateVector3.Migrate(x["gravityDir"]),
                            GravityPower = x["gravityPower"].GetSingle(),
                            HitRadius = x["hitRadius"].GetSingle(),
                            Stiffness = x["stiffiness"].GetSingle(),
                        });
                    }

                    springBone.Springs.Add(spring);
                }
            }

            return springBone;
        }
    }
}
