using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    public static class NodeImporter
    {
        public static GameObject ImportNode(glTFNode node, int nodeIndex)
        {
            var nodeName = node.name;
            if (!string.IsNullOrEmpty(nodeName) && nodeName.Contains("/"))
            {
                Debug.LogWarningFormat("node {0} contains /. replace _", node.name);
                nodeName = nodeName.Replace("/", "_");
            }
            if (string.IsNullOrEmpty(nodeName))
            {
                nodeName = string.Format("nodeIndex_{0}", nodeIndex);
            }
            var go = new GameObject(nodeName);

            //
            // transform
            //
            if (node.translation != null && node.translation.Length > 0)
            {
                go.transform.localPosition = new Vector3(
                    node.translation[0],
                    node.translation[1],
                    node.translation[2]);
            }
            if (node.rotation != null && node.rotation.Length > 0)
            {
                go.transform.localRotation = new Quaternion(
                    node.rotation[0],
                    node.rotation[1],
                    node.rotation[2],
                    node.rotation[3]);
            }
            if (node.scale != null && node.scale.Length > 0)
            {
                go.transform.localScale = new Vector3(
                    node.scale[0],
                    node.scale[1],
                    node.scale[2]);
            }
            if (node.matrix != null && node.matrix.Length > 0)
            {
                var m = UnityExtensions.MatrixFromArray(node.matrix);
                (go.transform.localPosition, go.transform.localRotation, go.transform.localScale) = m.Extract();
            }
            return go;
        }

        public class TransformWithSkin
        {
            public Transform Transform;
            public GameObject GameObject { get { return Transform.gameObject; } }
            public int? SkinIndex;
        }

        public static TransformWithSkin BuildHierarchy(glTF gltf, int i, List<Transform> nodes, List<MeshWithMaterials> meshes)
        {
            var go = nodes[i].gameObject;
            if (string.IsNullOrEmpty(go.name))
            {
                go.name = string.Format("node{0:000}", i);
            }

            var nodeWithSkin = new TransformWithSkin
            {
                Transform = go.transform,
            };

            //
            // build hierarchy
            //
            var node = gltf.nodes[i];
            if (node.children != null)
            {
                foreach (var child in node.children)
                {
                    nodes[child].transform.SetParent(nodes[i].transform,
                        false // node has local transform
                        );
                }
            }

            //
            // attach mesh
            //
            if (node.mesh != -1)
            {
                var mesh = meshes[node.mesh];
                if (mesh.Mesh.blendShapeCount == 0 && node.skin == -1)
                {
                    // without blendshape and bone skinning
                    var filter = go.AddComponent<MeshFilter>();
                    filter.sharedMesh = mesh.Mesh;
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterials = mesh.Materials;
                    // invisible in loading
                    renderer.enabled = false;
                    mesh.Renderers.Add(renderer);
                }
                else
                {
                    var renderer = go.AddComponent<SkinnedMeshRenderer>();

                    if (node.skin != -1)
                    {
                        nodeWithSkin.SkinIndex = node.skin;
                    }

                    renderer.sharedMesh = mesh.Mesh;
                    renderer.sharedMaterials = mesh.Materials;
                    // invisible in loading
                    renderer.enabled = false;

                    if (mesh.ShouldSetRendererNodeAsBone)
                    {
                        renderer.bones = new[] { renderer.transform };

                        //
                        // calc default matrices
                        // https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
                        //
                        var meshCoords = renderer.transform;
                        var calculatedBindPoses = renderer.bones.Select(bone => bone.worldToLocalMatrix * meshCoords.localToWorldMatrix).ToArray();
                        mesh.Mesh.bindposes = calculatedBindPoses;
                    }

                    mesh.Renderers.Add(renderer);
                }
            }

            return nodeWithSkin;
        }

        //
        // fix node's coordinate. z-back to z-forward
        //
        public static void FixCoordinate(glTF gltf, List<TransformWithSkin> nodes, IAxisInverter inverter)
        {
            if (gltf.rootnodes == null)
            {
                return;
            }
            var globalTransformMap = nodes.ToDictionary(x => x.Transform, x => new PosRot
            {
                Position = x.Transform.position,
                Rotation = x.Transform.rotation,
            });
            foreach (var x in gltf.rootnodes)
            {
                // fix nodes coordinate
                // reverse Z in global
                var t = nodes[x].Transform;
                //t.SetParent(root.transform, false);

                foreach (var transform in t.Traverse())
                {
                    var g = globalTransformMap[transform];
                    transform.position = inverter.InvertVector3(g.Position);
                    transform.rotation = inverter.InvertQuaternion(g.Rotation);
                }
            }
        }

        public static void SetupSkinning(GltfData data, List<TransformWithSkin> nodes, int i, IAxisInverter inverter)
        {
            var x = nodes[i];
            if (x.Transform.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                var mesh = skinnedMeshRenderer.sharedMesh;
                if (x.SkinIndex.HasValue)
                {
                    if (mesh == null) throw new Exception();
                    if (skinnedMeshRenderer == null) throw new Exception();

                    if (x.SkinIndex.Value < data.GLTF.skins.Count)
                    {
                        // calculate internal values(boundingBox etc...) when sharedMesh assigned ?
                        skinnedMeshRenderer.sharedMesh = null;

                        var skin = data.GLTF.skins[x.SkinIndex.Value];
                        var joints = skin.joints.Select(y =>
                        {
                            if (y >= 0 && y < nodes.Count)
                            {
                                return nodes[y].Transform;
                            }
                            else
                            {
                                return null;
                            }
                        }).ToArray();
                        if (joints.Any())
                        {
                            // have bones
                            skinnedMeshRenderer.bones = joints;

                            if (skin.inverseBindMatrices != -1)
                            {
                                var bindPoses = data.GetArrayFromAccessor<Matrix4x4>(skin.inverseBindMatrices)
                                    .Select(inverter.InvertMat4)
                                    .ToArray()
                                    ;
                                mesh.bindposes = bindPoses;
                            }
                            else
                            {
                                //
                                // calc default matrices
                                // https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
                                //
                                var meshCoords = skinnedMeshRenderer.transform; // ?
                                var calculatedBindPoses = joints.Select(y =>
                                {
                                    if (y != null)
                                    {
                                        return y.worldToLocalMatrix * meshCoords.localToWorldMatrix;
                                    }
                                    else
                                    {
                                        return Matrix4x4.identity * meshCoords.localToWorldMatrix;
                                    }
                                }).ToArray();
                                mesh.bindposes = calculatedBindPoses;
                            }
                        }
                        else
                        {
                            // BlendShape only ?
                        }

                        skinnedMeshRenderer.sharedMesh = mesh;
                        if (skin.skeleton >= 0 && skin.skeleton < nodes.Count)
                        {
                            skinnedMeshRenderer.rootBone = nodes[skin.skeleton].Transform;
                        }
                    }
                }
            }
        }
    }
}
