using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MeshUtility;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    public class RuntimeVrmConverter
    {
        public VrmLib.Model Model;

        public Dictionary<GameObject, VrmLib.Node> Nodes = new Dictionary<GameObject, VrmLib.Node>();
        public List<UnityEngine.Material> Materials = new List<UnityEngine.Material>();
        public Dictionary<UnityEngine.Mesh, VrmLib.MeshGroup> Meshes = new Dictionary<UnityEngine.Mesh, VrmLib.MeshGroup>();

        #region Export 1.0
        /// <summary>
        /// metaObject が null のときは、root から取得する
        /// </summary>
        public VrmLib.Model ToModelFrom10(GameObject root)
        {
            Model = new VrmLib.Model(VrmLib.Coordinates.Unity);

            ToGlbModel(root);

            // humanoid
            {
                var humanoid = root.GetComponent<MeshUtility.Humanoid>();
                if (humanoid is null)
                {
                    humanoid = root.AddComponent<MeshUtility.Humanoid>();
                    humanoid.AssignBonesFromAnimator();
                }

                foreach (HumanBodyBones humanBoneType in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    var transform = humanoid.GetBoneTransform(humanBoneType);
                    if (transform != null && Nodes.TryGetValue(transform.gameObject, out VrmLib.Node node))
                    {
                        node.HumanoidBone = (VrmLib.HumanoidBones)Enum.Parse(typeof(VrmLib.HumanoidBones), humanBoneType.ToString(), true);
                    }
                }
            }

            return Model;
        }

        public VrmLib.Model ToGlbModel(GameObject root)
        {
            if (Model == null)
            {
                Model = new VrmLib.Model(VrmLib.Coordinates.Unity);
            }

            // node
            {
                Model.Root.Name = root.name;
                CreateNodes(root.transform, Model.Root, Nodes);
                Model.Nodes = Nodes
                .Where(x => x.Value != Model.Root)
                            .Select(x => x.Value).ToList();
            }

            // material and textures
            var rendererComponents = root.GetComponentsInChildren<Renderer>();
            {
                foreach (var renderer in rendererComponents)
                {
                    var materials = renderer.sharedMaterials; // avoid copy
                    foreach (var material in materials)
                    {
                        if (Materials.Contains(material))
                        {
                            continue;
                        }

                        Model.Materials.Add(material);
                        Materials.Add(material);
                    }
                }
            }

            // mesh
            {
                foreach (var renderer in rendererComponents)
                {
                    if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                        if (skinnedMeshRenderer.sharedMesh != null)
                        {
                            var mesh = CreateMesh(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer, Materials);
                            var skin = CreateSkin(skinnedMeshRenderer, Nodes, root);
                            if (skin != null)
                            {
                                // blendshape only で skinning が無いやつがある
                                mesh.Skin = skin;
                                Model.Skins.Add(mesh.Skin);
                            }
                            Model.MeshGroups.Add(mesh);
                            Nodes[renderer.gameObject].MeshGroup = mesh;
                            Meshes.Add(skinnedMeshRenderer.sharedMesh, mesh);
                        }
                    }
                    else if (renderer is MeshRenderer meshRenderer)
                    {
                        var filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                        if (filter != null && filter.sharedMesh != null)
                        {
                            var mesh = CreateMesh(filter.sharedMesh, meshRenderer, Materials);
                            Model.MeshGroups.Add(mesh);
                            Nodes[renderer.gameObject].MeshGroup = mesh;
                            Meshes.Add(filter.sharedMesh, mesh);
                        }
                    }
                }
            }

            return Model;
        }
        #endregion



        private static void CreateNodes(
            Transform parentTransform,
            VrmLib.Node parentNode,
            Dictionary<GameObject, VrmLib.Node> nodes)
        {
            // parentNode.SetMatrix(parentTransform.localToWorldMatrix.ToNumericsMatrix4x4(), false);
            parentNode.LocalTranslation = parentTransform.localPosition.ToNumericsVector3();
            parentNode.LocalRotation = parentTransform.localRotation.ToNumericsQuaternion();
            parentNode.LocalScaling = parentTransform.localScale.ToNumericsVector3();
            nodes.Add(parentTransform.gameObject, parentNode);

            foreach (Transform child in parentTransform)
            {
                var childNode = new VrmLib.Node(child.gameObject.name);
                CreateNodes(child, childNode, nodes);
                parentNode.Add(childNode);
            }
        }

        private static Transform GetTransformFromRelativePath(Transform root, Queue<string> relativePath)
        {
            var name = relativePath.Dequeue();
            foreach (Transform node in root)
            {
                if (node.gameObject.name == name)
                {
                    if (relativePath.Count == 0)
                    {
                        return node;
                    }
                    else
                    {
                        return GetTransformFromRelativePath(node, relativePath);
                    }
                }
            }

            return null;
        }

        private static VrmLib.MeshGroup CreateMesh(UnityEngine.Mesh mesh, Renderer renderer, List<UnityEngine.Material> materials)
        {
            var meshGroup = new VrmLib.MeshGroup(mesh.name);
            var vrmMesh = new VrmLib.Mesh();
            vrmMesh.VertexBuffer = new VrmLib.VertexBuffer();
            vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.PositionKey, ToBufferAccessor(mesh.vertices));

            if (mesh.boneWeights.Length == mesh.vertexCount)
            {
                vrmMesh.VertexBuffer.Add(
                    VrmLib.VertexBuffer.WeightKey,
                    ToBufferAccessor(mesh.boneWeights.Select(x =>
                    new Vector4(x.weight0, x.weight1, x.weight2, x.weight3)).ToArray()
                    ));
                vrmMesh.VertexBuffer.Add(
                    VrmLib.VertexBuffer.JointKey,
                    ToBufferAccessor(mesh.boneWeights.Select(x =>
                    new VrmLib.SkinJoints((ushort)x.boneIndex0, (ushort)x.boneIndex1, (ushort)x.boneIndex2, (ushort)x.boneIndex3)).ToArray()
                    ));
            }
            if (mesh.uv.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.TexCoordKey, ToBufferAccessor(mesh.uv));
            if (mesh.normals.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.NormalKey, ToBufferAccessor(mesh.normals));
            if (mesh.colors.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.ColorKey, ToBufferAccessor(mesh.colors));
            vrmMesh.IndexBuffer = ToBufferAccessor(mesh.triangles);

            int offset = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
#if UNITY_2019
                var subMesh = mesh.GetSubMesh(i);
                try
                {
                    vrmMesh.Submeshes.Add(new VrmLib.Submesh(offset, subMesh.indexCount, materials.IndexOf(renderer.sharedMaterials[i])));
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                offset += subMesh.indexCount;
#else
                var triangles = mesh.GetTriangles(i);
                try
                {
                    vrmMesh.Submeshes.Add(new VrmLib.Submesh(offset, triangles.Length, materials.IndexOf(renderer.sharedMaterials[i])));
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                offset += triangles.Length;
#endif
            }

            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                var blendShapeVertices = mesh.vertices;
                var usePosition = blendShapeVertices != null && blendShapeVertices.Length > 0;

                var blendShapeNormals = mesh.normals;
                var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length;
                // var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length && !exportOnlyBlendShapePosition;

                var blendShapeTangents = mesh.tangents.Select(y => (Vector3)y).ToArray();
                //var useTangent = usePosition && blendShapeTangents != null && blendShapeTangents.Length == blendShapeVertices.Length;
                // var useTangent = false;

                var frameCount = mesh.GetBlendShapeFrameCount(i);
                mesh.GetBlendShapeFrameVertices(i, frameCount - 1, blendShapeVertices, blendShapeNormals, null);

                if (usePosition)
                {
                    var morphTarget = new VrmLib.MorphTarget(mesh.GetBlendShapeName(i));
                    morphTarget.VertexBuffer = new VrmLib.VertexBuffer();
                    morphTarget.VertexBuffer.Add(VrmLib.VertexBuffer.PositionKey, ToBufferAccessor(blendShapeVertices));
                    vrmMesh.MorphTargets.Add(morphTarget);
                }
            }

            meshGroup.Meshes.Add(vrmMesh);
            return meshGroup;
        }

        private static VrmLib.Skin CreateSkin(
            SkinnedMeshRenderer skinnedMeshRenderer,
            Dictionary<GameObject, VrmLib.Node> nodes,
            GameObject root)
        {
            if (skinnedMeshRenderer.bones == null || skinnedMeshRenderer.bones.Length == 0)
            {
                return null;
            }

            var skin = new VrmLib.Skin();
            skin.InverseMatrices = ToBufferAccessor(skinnedMeshRenderer.sharedMesh.bindposes);
            if (skinnedMeshRenderer.rootBone != null)
            {
                skin.Root = nodes[skinnedMeshRenderer.rootBone.gameObject];
            }

            skin.Joints = skinnedMeshRenderer.bones.Select(x => nodes[x.gameObject]).ToList();
            return skin;
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(VrmLib.SkinJoints[] values)
        {
            return ToBufferAccessor(values, VrmLib.AccessorValueType.UNSIGNED_SHORT, VrmLib.AccessorVectorType.VEC4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Color[] colors)
        {
            return ToBufferAccessor(colors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Vector4[] vectors)
        {
            return ToBufferAccessor(vectors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Vector3[] vectors)
        {
            return ToBufferAccessor(vectors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC3);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Vector2[] vectors)
        {
            return ToBufferAccessor(vectors, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.VEC2);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(int[] scalars)
        {
            return ToBufferAccessor(scalars, VrmLib.AccessorValueType.UNSIGNED_INT, VrmLib.AccessorVectorType.SCALAR);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor(Matrix4x4[] matrixes)
        {
            return ToBufferAccessor(matrixes, VrmLib.AccessorValueType.FLOAT, VrmLib.AccessorVectorType.MAT4);
        }

        private static VrmLib.BufferAccessor ToBufferAccessor<T>(T[] value, VrmLib.AccessorValueType valueType, VrmLib.AccessorVectorType vectorType) where T : struct
        {
            var span = VrmLib.SpanLike.CopyFrom(value);
            return new VrmLib.BufferAccessor(
                span.Bytes,
                valueType,
                vectorType,
                value.Length
                );
        }
    }
}
