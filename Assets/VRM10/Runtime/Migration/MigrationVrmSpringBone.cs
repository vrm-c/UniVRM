using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniVRM10
{
    public static class MigrationVrmSpringBone
    {
        static IEnumerable<UniGLTF.glTFNode> TraverseFirstChild(List<UniGLTF.glTFNode> nodes, UniGLTF.glTFNode node)
        {
            yield return node;

            if (node.children != null && node.children.Length > 0)
            {
                foreach (var x in TraverseFirstChild(nodes, nodes[node.children[0]]))
                {
                    yield return x;
                }
            }
        }

        static void AddTail7cm(UniGLTF.glTF gltf, UniGLTF.glTFNode[] joints)
        {
            if (joints.Length < 2)
            {
                return;
            }
            var last = joints.Last();
            var name = last.name ?? "";
            var v1 = new UnityEngine.Vector3(last.translation[0], last.translation[1], last.translation[2]);
            // var last2 = joints[joints.Length - 2];
            // var v2 = new UnityEngine.Vector3(last2.translation[0], last2.translation[1], last2.translation[2]);
            var delta = v1.normalized * 0.07f; // 7cm
            var tail = new UniGLTF.glTFNode
            {
                name = name + "_end",
                translation = new float[] {
                    delta.x,
                    delta.y,
                    delta.z
                },
            };
            var tail_index = gltf.nodes.Count;
            gltf.nodes.Add(tail);
            if (last.children != null && last.children.Length > 0)
            {
                throw new System.Exception();
            }
            last.children = new[] { tail_index };
        }

        /// <summary>
        /// {
        ///   "colliderGroups": [
        ///   ],    
        ///   "boneGroups": [
        ///   ],
        /// }
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="sa"></param>
        /// <returns></returns>
        public static UniGLTF.Extensions.VRMC_springBone.VRMC_springBone Migrate(UniGLTF.glTF gltf, JsonNode sa)
        {
            var springBone = new UniGLTF.Extensions.VRMC_springBone.VRMC_springBone
            {
                Colliders = new List<UniGLTF.Extensions.VRMC_springBone.Collider>(),
                ColliderGroups = new List<UniGLTF.Extensions.VRMC_springBone.ColliderGroup>(),
                Springs = new List<UniGLTF.Extensions.VRMC_springBone.Spring>(),
            };

            foreach (var x in sa["colliderGroups"].ArrayItems())
            {
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
                var colliders = new List<int>();
                foreach (var y in x["colliders"].ArrayItems())
                {
                    colliders.Add(springBone.Colliders.Count);
                    springBone.Colliders.Add(new UniGLTF.Extensions.VRMC_springBone.Collider
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
                var colliderGroup = new UniGLTF.Extensions.VRMC_springBone.ColliderGroup()
                {
                    Colliders = colliders.ToArray(),
                };
                springBone.ColliderGroups.Add(colliderGroup);
            }

            // https://github.com/vrm-c/vrm-specification/pull/255
            // 1.0 では末端に7cmの遠さに joint を追加する動作をしなくなった。
            // その差異に対応して、7cmの遠さに node を追加する。
            foreach (var x in sa["boneGroups"].ArrayItems())
            {
                foreach (var y in x["bones"].ArrayItems())
                {
                    var joints = TraverseFirstChild(gltf.nodes, gltf.nodes[y.GetInt32()]).ToArray();
                    AddTail7cm(gltf, joints);
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

                    foreach (var z in TraverseFirstChild(gltf.nodes, gltf.nodes[y.GetInt32()]))
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
