using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF
{
    public static class NodeImporter
    {
        public static GameObject ImportNode(VGltf.Types.Node node)
        {
            var nodeName = node.Name;
            if (!string.IsNullOrEmpty(nodeName) && nodeName.Contains("/"))
            {
                Debug.LogWarningFormat("node {0} contains /. replace _", node.Name);
                nodeName = nodeName.Replace("/", "_");
            }
            var go = new GameObject(nodeName);

            var useTRS = false;

            //
            // transform
            //
            if (node.Translation != null && node.Translation.Length >= 3)
            {
                go.transform.localPosition = new Vector3(
                    node.Translation[0],
                    node.Translation[1],
                    node.Translation[2]);

                useTRS = true;
            }

            if (node.Rotation != null && node.Rotation.Length >= 4)
            {
                go.transform.localRotation = new Quaternion(
                    node.Rotation[0],
                    node.Rotation[1],
                    node.Rotation[2],
                    node.Rotation[3]);

                useTRS = true;
            }

            if (node.Scale != null && node.Scale.Length >= 3)
            {
                go.transform.localScale = new Vector3(
                    node.Scale[0],
                    node.Scale[1],
                    node.Scale[2]);

                useTRS = true;
            }

            if (!useTRS && node.Matrix != null && node.Matrix.Length >= 16)
            {
                var m = UnityExtensions.MatrixFromArray(node.Matrix);
                go.transform.localRotation = m.ExtractRotation();
                go.transform.localPosition = m.ExtractPosition();
                go.transform.localScale = m.ExtractScale();
            }

            return go;
        }

        public class TransformWithSkin
        {
            public Transform Transform;
            public GameObject GameObject { get { return Transform.gameObject; } }
            public int? SkinIndex;
        }

        public static TransformWithSkin BuildHierarchy(ImporterContext context, int i)
        {
            var go = context.Nodes[i].gameObject;
            if (string.IsNullOrEmpty(go.name))
            {
                go.name = string.Format("node{0:000}", i);
            }

            var nodeWithSkin = new TransformWithSkin
            {
                Transform = go.transform,
            };

            //
            // build hierachy
            //
            var node = context.GLTF2.Nodes[i];
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    context.Nodes[child].transform.SetParent(context.Nodes[i].transform,
                        false // node has local transform
                        );
                }
            }

            //
            // attach mesh
            //
            if (node.Mesh != null)
            {
                var mesh = context.Meshes[node.Mesh.Value];

                if (mesh.Mesh.blendShapeCount == 0 && node.Skin == null)
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

                    if (node.Skin != null)
                    {
                        nodeWithSkin.SkinIndex = node.Skin.Value;
                    }

                    renderer.sharedMesh = mesh.Mesh;
                    renderer.sharedMaterials = mesh.Materials;
                    // invisible in loading
                    renderer.enabled = false;
                    mesh.Renderers.Add(renderer);
                }
            }

            return nodeWithSkin;
        }

        //
        // fix node's coordinate. z-back to z-forward
        //
        public static void FixCoordinate(ImporterContext context, List<TransformWithSkin> nodes)
        {
            var globalTransformMap = nodes.ToDictionary(x => x.Transform, x => new PosRot
            {
                Position = x.Transform.position,
                Rotation = x.Transform.rotation,
            });

            foreach (var i in context.GLTF2.RootNodesIndices)
            {
                // fix nodes coordinate
                // reverse Z in global
                var t = nodes[i].Transform;
                //t.SetParent(root.transform, false);

                foreach (var transform in t.Traverse())
                {
                    var g = globalTransformMap[transform];
                    transform.position = g.Position.ReverseZ();
                    transform.rotation = g.Rotation.ReverseZ();
                }
            }
        }

        public static void SetupSkinning(ImporterContext context, List<TransformWithSkin> nodes, int i)
        {
            var x = nodes[i];
            var skinnedMeshRenderer = x.Transform.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                var mesh = skinnedMeshRenderer.sharedMesh;
                if (x.SkinIndex.HasValue)
                {
                    if (mesh == null) throw new Exception();
                    if (skinnedMeshRenderer == null) throw new Exception();

                    if (x.SkinIndex.Value < context.GLTF2.Skins.Count)
                    {
                        // calculate internal values(boudingBox etc...) when sharedMesh assinged ?
                        skinnedMeshRenderer.sharedMesh = null;

                        var skin = context.GLTF2.Skins[x.SkinIndex.Value];
                        var joints = skin.Joints.Select(y => nodes[y].Transform).ToArray();
                        if (joints.Any())
                        {
                            // have bones
                            skinnedMeshRenderer.bones = joints;

                            if (skin.InverseBindMatrices != null)
                            {
                                var bindMat = context.Store.GetOrLoadTypedBufferByAccessorIndex(skin.InverseBindMatrices.Value);
                                var bindPoses = bindMat.GetEntity<Matrix4x4>().GetEnumerable()
                                    .Select(y => y.ReverseZ())
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
                                var calculatedBindPoses = joints.Select(y => y.worldToLocalMatrix * meshCoords.localToWorldMatrix).ToArray();
                                mesh.bindposes = calculatedBindPoses;
                            }
                        }
                        else
                        {
                            // BlendShape only ?
                        }

                        skinnedMeshRenderer.sharedMesh = mesh;
                        if (skin.Skeleton != null && skin.Skeleton.Value >= 0 && skin.Skeleton.Value < nodes.Count)
                        {
                            skinnedMeshRenderer.rootBone = nodes[skin.Skeleton.Value].Transform;
                        }
                    }
                }
            }
        }
    }
}
