using System.Collections.Generic;
using System.Linq;
using UniGLTF.Extensions.VRMC_springBone;
using UniJSON;

namespace UniVRM10
{
    public static class MigrationVrmSpringBone
    {
        class SpringBoneGroupMigrator
        {
            UniGLTF.glTF _gltf;

            string _comment;
            float _dragForce;
            float[] _gravityDir;
            float _gravityPower;
            float _hitRadius;
            float _stiffness;
            int[] _colliderGroups;
            int? _center;

            List<Spring> _springs = new List<Spring>();
            public IReadOnlyList<Spring> Springs => _springs;

            public SpringBoneGroupMigrator(UniGLTF.glTF gltf, JsonNode vrm0BoneGroup)
            {
                _gltf = gltf;

                _comment = vrm0BoneGroup.GetObjectValueOrDefault("comment", "");
                _dragForce = vrm0BoneGroup["dragForce"].GetSingle();
                _gravityDir = MigrateVector3.Migrate(vrm0BoneGroup["gravityDir"]);
                _gravityPower = vrm0BoneGroup["gravityPower"].GetSingle();
                _hitRadius = vrm0BoneGroup["hitRadius"].GetSingle();
                _stiffness = vrm0BoneGroup["stiffiness"].GetSingle();
                _colliderGroups = vrm0BoneGroup["colliderGroups"].ArrayItems().Select(z => z.GetInt32()).ToArray();

                var center = vrm0BoneGroup["center"].GetInt32();
                if (center >= 0)
                {
                    _center = center;
                }

                if (vrm0BoneGroup.ContainsKey("bones"))
                {
                    foreach (var vrm0Bone in vrm0BoneGroup["bones"].ArrayItems())
                    {
                        MigrateRootBone(vrm0Bone.GetInt32());
                    }
                }
            }

            Spring CreateSpring()
            {
                var spring = new Spring
                {
                    Name = _comment,
                    ColliderGroups = _colliderGroups,
                    Joints = new List<SpringBoneJoint>(),
                    Center = _center,
                };
                _springs.Add(spring);
                return spring;
            }

            SpringBoneJoint CreateJoint(int node)
            {
                return new SpringBoneJoint
                {
                    Node = node,
                    DragForce = _dragForce,
                    GravityDir = _gravityDir,
                    GravityPower = _gravityPower,
                    HitRadius = _hitRadius,
                    Stiffness = _stiffness,
                };
            }

            void MigrateRootBone(int rootBoneIndex)
            {
                if (rootBoneIndex >= 0 && rootBoneIndex < _gltf.nodes.Count)
                {
                    // root
                    CreateJointsRecursive(_gltf.nodes[rootBoneIndex], 1);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <param name="level">children[0] のみカウントアップする。その他は0にリセットする</param>
            /// <param name="spring"></param>
            void CreateJointsRecursive(UniGLTF.glTFNode node, int level, Spring spring = null)
            {
                if (spring == null && level > 0)
                {
                    // ２番目以降の子ノードの子から新しい Spring を作る。
                    spring = CreateSpring();
                }
                if (spring != null)
                {
                    // level==0 のとき(２番目以降の兄弟ボーン)は飛ばす
                    spring.Joints.Add(CreateJoint(_gltf.nodes.IndexOf(node)));
                }

                if (node.children != null && node.children.Length > 0)
                {
                    for (int i = 0; i < node.children.Length; ++i)
                    {
                        var childIndex = node.children[i];
                        if (childIndex < 0 || childIndex >= _gltf.nodes.Count)
                        {
                            // -1 など？
                            continue;
                        }

                        if (i == 0)
                        {
                            // spring に joint を追加する
                            CreateJointsRecursive(_gltf.nodes[childIndex], level + 1, spring);
                        }
                        else
                        {
                            // 再帰
                            CreateJointsRecursive(_gltf.nodes[childIndex], 0);
                        }
                    }
                }
                else
                {

                    if (spring != null && spring.Joints.Count > 0)
                    {
                        var last = spring.Joints.Last().Node;
                        if (last.HasValue)
                        {
                            var tailJoint = AddTail7cm(last.Value);
                            spring.Joints.Add(tailJoint);
                        }
                    }
                }
            }

            // https://github.com/vrm-c/vrm-specification/pull/255
            // 1.0 では末端に7cmの遠さに joint を追加する動作をしなくなった。
            // その差異に対応して、7cmの遠さに node を追加する。
            SpringBoneJoint AddTail7cm(int lastIndex)
            {
                var last = _gltf.nodes[lastIndex];
                var name = last.name ?? "";
                var v1 = new UnityEngine.Vector3(last.translation[0], last.translation[1], last.translation[2]);
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
                var tail_index = _gltf.nodes.Count;
                _gltf.nodes.Add(tail);
                if (last.children != null && last.children.Length > 0)
                {
                    throw new System.Exception();
                }
                last.children = new[] { tail_index };

                // 1.0 では、head + tail のペアでスプリングを表し、
                // 揺れ挙動のパラメーターは head の方に入る。
                // 要するに 末端の joint では Node しか使われない。
                return new SpringBoneJoint
                {
                    Node = tail_index,
                };
            }
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
        /// <param name="vrm0"></param>
        /// <returns></returns>
        public static VRMC_springBone Migrate(UniGLTF.glTF gltf, JsonNode vrm0)
        {
            var springBone = new VRMC_springBone
            {
                SpecVersion = Vrm10Exporter.SPRINGBONE_SPEC_VERSION,
                Colliders = new List<Collider>(),
                ColliderGroups = new List<ColliderGroup>(),
                Springs = new List<Spring>(),
            };

            // NOTE: ColliderGroups をマイグレーションする.
            //       ColliderGroup は Spring から index で参照されているため、順序を入れ替えたり増減させてはいけない.
            foreach (var vrm0ColliderGroup in vrm0["colliderGroups"].ArrayItems())
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

                // NOTE: 1.0 では ColliderGroup は Collider の実体ではなく index を参照する.
                var colliderIndices = new List<int>();
                if (vrm0ColliderGroup.ContainsKey("node") && vrm0ColliderGroup.ContainsKey("colliders"))
                {
                    var nodeIndex = vrm0ColliderGroup["node"].GetInt32();
                    // NOTE: ColliderGroup に含まれる Collider をマイグレーションする.
                    foreach (var vrm0Collider in vrm0ColliderGroup["colliders"].ArrayItems())
                    {
                        if (!vrm0Collider.ContainsKey("offset")) continue;
                        if (!vrm0Collider.ContainsKey("radius")) continue;

                        colliderIndices.Add(springBone.Colliders.Count);
                        springBone.Colliders.Add(new Collider
                        {
                            Node = nodeIndex,
                            Shape = new ColliderShape
                            {
                                Sphere = new ColliderShapeSphere
                                {
                                    Offset = MigrateVector3.Migrate(vrm0Collider["offset"]),
                                    Radius = vrm0Collider["radius"].GetSingle()
                                }
                            }
                        });
                    }
                }
                var colliderGroup = new ColliderGroup()
                {
                    Colliders = colliderIndices.ToArray(),
                };
                springBone.ColliderGroups.Add(colliderGroup);
            }

            foreach (var vrm0BoneGroup in vrm0["boneGroups"].ArrayItems())
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
                var migrator = new SpringBoneGroupMigrator(gltf, vrm0BoneGroup);
                springBone.Springs.AddRange(migrator.Springs);
            }

            return springBone;
        }
    }
}
