using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniVRM10
{
    public static class MigrationVrmSpringBone
    {
        static void CreateJointsRecursive(UniGLTF.Extensions.VRMC_springBone.Spring spring, List<UniGLTF.glTFNode> nodes, UniGLTF.glTFNode node, Func<int, UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint> createJoint)
        {
            spring.Joints.Add(createJoint(nodes.IndexOf(node)));

            if (node.children != null && node.children.Length > 0)
            {
                // 先頭の子ノードを追加する
                CreateJointsRecursive(spring, nodes, nodes[node.children[0]], createJoint);
                // foreach (var x in TraverseFirstChild(nodes, nodes[node.children[0]]))
                // {
                //     yield return x;
                // }
            }
            else
            {
                // https://github.com/vrm-c/vrm-specification/pull/255
                // 1.0 では末端に7cmの遠さに joint を追加する動作をしなくなった。
                // その差異に対応して、7cmの遠さに node を追加する。

                // var tail = AddTail7cm(nodes, nodes[spring.Joints.Last().Node])
            }
        }

        static int AddTail7cm(List<UniGLTF.glTFNode> nodes, UniGLTF.glTFNode last)
        {
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
            var tail_index = nodes.Count;
            nodes.Add(tail);
            if (last.children != null && last.children.Length > 0)
            {
                throw new System.Exception();
            }
            last.children = new[] { tail_index };
            return tail_index;
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
                if (x.ContainsKey("bones"))
                {
                    var comment = x.GetObjectValueOrDefault("comment", "");
                    var dragForce = x["dragForce"].GetSingle();
                    var gravityDir = MigrateVector3.Migrate(x["gravityDir"]);
                    var gravityPower = x["gravityPower"].GetSingle();
                    var hitRadius = x["hitRadius"].GetSingle();
                    var stiffiness = x["stiffiness"].GetSingle();
                    foreach (var y in x["bones"].ArrayItems())
                    {
                        var rootBone = gltf.nodes[y.GetInt32()];

                        var spring = new UniGLTF.Extensions.VRMC_springBone.Spring
                        {
                            Name = comment,
                            ColliderGroups = x["colliderGroups"].ArrayItems().Select(z => z.GetInt32()).ToArray(),
                            Joints = new List<UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint>(),
                        };
                        springBone.Springs.Add(spring);

                        CreateJointsRecursive(spring, gltf.nodes, rootBone, (int node) =>
                        {
                            return new UniGLTF.Extensions.VRMC_springBone.SpringBoneJoint
                            {
                                Node = node,
                                DragForce = dragForce,
                                GravityDir = gravityDir,
                                GravityPower = gravityPower,
                                HitRadius = hitRadius,
                                Stiffness = stiffiness,
                            };
                        });
                    }
                }
            }

            return springBone;
        }
    }
}
