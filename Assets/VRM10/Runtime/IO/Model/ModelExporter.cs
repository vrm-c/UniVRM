using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UniGLTF;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// UnityEngine.GameObject hierarchy => GLTF.Nodes, GLTF.Meshes, GLTF.Skins
    /// </summary>
    public class ModelExporter
    {
        public VrmLib.Model Model;

        public Dictionary<GameObject, VrmLib.Node> Nodes = new Dictionary<GameObject, VrmLib.Node>();
        public List<UnityEngine.Material> Materials = new List<UnityEngine.Material>();
        public Dictionary<UnityEngine.Mesh, VrmLib.MeshGroup> Meshes = new Dictionary<UnityEngine.Mesh, VrmLib.MeshGroup>();

        /// <summary>
        /// GameObject to VrmLib.Model
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public VrmLib.Model Export(INativeArrayManager arrayManager, GameObject root)
        {
            Model = new VrmLib.Model(VrmLib.Coordinates.Unity);

            _Export(arrayManager, root);

            // humanoid
            {
                var humanoid = root.GetComponentOrNull<UniHumanoid.Humanoid>();
                if (humanoid == null)
                {
                    humanoid = root.AddComponent<UniHumanoid.Humanoid>();
                    humanoid.AssignBonesFromAnimator();
                }

                foreach (HumanBodyBones humanBoneType in CachedEnum.GetValues<HumanBodyBones>())
                {
                    var transform = humanoid.GetBoneTransform(humanBoneType);
                    if (transform != null && Nodes.TryGetValue(transform.gameObject, out VrmLib.Node node))
                    {
                        switch (humanBoneType)
                        {
                            // https://github.com/vrm-c/vrm-specification/issues/380
                            case HumanBodyBones.LeftThumbProximal: node.HumanoidBone = VrmLib.HumanoidBones.leftThumbMetacarpal; break;
                            case HumanBodyBones.LeftThumbIntermediate: node.HumanoidBone = VrmLib.HumanoidBones.leftThumbProximal; break;
                            case HumanBodyBones.RightThumbProximal: node.HumanoidBone = VrmLib.HumanoidBones.rightThumbMetacarpal; break;
                            case HumanBodyBones.RightThumbIntermediate: node.HumanoidBone = VrmLib.HumanoidBones.rightThumbProximal; break;
                            default: node.HumanoidBone = (VrmLib.HumanoidBones)Enum.Parse(typeof(VrmLib.HumanoidBones), humanBoneType.ToString(), true); break;
                        }
                    }
                }
            }

            return Model;
        }

        /// <summary>
        /// 頂点と面が存在する Mesh のみをエクスポート可能とする
        /// </summary>
        static bool MeshCanExport(UnityEngine.Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogWarning("mesh is null");
                return false;
            }
            if (mesh.vertexCount == 0)
            {
                Debug.LogWarning($"{mesh}: no vertices");
                return false;
            }
            if (mesh.triangles == null)
            {
                Debug.LogWarning($"{mesh}: no triangles");
                return false;
            }
            if (mesh.triangles.Length == 0)
            {
                Debug.LogWarning($"{mesh}: no triangles");
                return false;
            }
            return true;
        }

        VrmLib.Model _Export(INativeArrayManager arrayManager, GameObject root)
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
                        if (material == null)
                        {
                            continue;
                        }
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
                        if (MeshCanExport(skinnedMeshRenderer.sharedMesh))
                        {
                            var mesh = CreateMesh(arrayManager, skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer, Materials);
                            var skin = CreateSkin(arrayManager, skinnedMeshRenderer, Nodes, root);
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
                        if (meshRenderer.gameObject.TryGetComponent<MeshFilter>(out var filter) && MeshCanExport(filter.sharedMesh))
                        {
                            var mesh = CreateMesh(arrayManager, filter.sharedMesh, meshRenderer, Materials);
                            Model.MeshGroups.Add(mesh);
                            Nodes[renderer.gameObject].MeshGroup = mesh;
                            if (!Meshes.ContainsKey(filter.sharedMesh))
                            {
                                Meshes.Add(filter.sharedMesh, mesh);
                            }
                        }
                    }
                }
            }

            return Model;
        }

        private static void CreateNodes(
            Transform parentTransform,
            VrmLib.Node parentNode,
            Dictionary<GameObject, VrmLib.Node> nodes)
        {
            // parentNode.SetMatrix(parentTransform.localToWorldMatrix.ToNumericsMatrix4x4(), false);
            parentNode.LocalTranslation = parentTransform.localPosition;
            parentNode.LocalRotation = parentTransform.localRotation;
            parentNode.LocalScaling = parentTransform.localScale;
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

        private static VrmLib.MeshGroup CreateMesh(INativeArrayManager arrayManager, UnityEngine.Mesh mesh, Renderer renderer, List<UnityEngine.Material> materials)
        {
            var meshGroup = new VrmLib.MeshGroup(mesh.name);
            var vrmMesh = new VrmLib.Mesh();
            vrmMesh.VertexBuffer = new VrmLib.VertexBuffer();
            vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.PositionKey, ToBufferAccessor(arrayManager, mesh.vertices));

            if (mesh.boneWeights.Length == mesh.vertexCount)
            {
                vrmMesh.VertexBuffer.Add(
                    VrmLib.VertexBuffer.WeightKey,
                    ToBufferAccessor(arrayManager, mesh.boneWeights.Select(x =>
                    new Vector4(x.weight0, x.weight1, x.weight2, x.weight3)).ToArray()
                    ));
                vrmMesh.VertexBuffer.Add(
                    VrmLib.VertexBuffer.JointKey,
                    ToBufferAccessor(arrayManager, mesh.boneWeights.Select(x =>
                    new SkinJoints((ushort)x.boneIndex0, (ushort)x.boneIndex1, (ushort)x.boneIndex2, (ushort)x.boneIndex3)).ToArray()
                    ));
            }
            if (mesh.uv.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.TexCoordKey, ToBufferAccessor(arrayManager, mesh.uv));
            if (mesh.normals.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.NormalKey, ToBufferAccessor(arrayManager, mesh.normals));
            if (mesh.colors.Length == mesh.vertexCount) vrmMesh.VertexBuffer.Add(VrmLib.VertexBuffer.ColorKey, ToBufferAccessor(arrayManager, mesh.colors));
            vrmMesh.IndexBuffer = ToBufferAccessor(arrayManager, mesh.triangles);

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
                    morphTarget.VertexBuffer.Add(VrmLib.VertexBuffer.PositionKey, ToBufferAccessor(arrayManager, blendShapeVertices));
                    vrmMesh.MorphTargets.Add(morphTarget);
                }
            }

            meshGroup.Meshes.Add(vrmMesh);
            return meshGroup;
        }

        private static VrmLib.Skin CreateSkin(INativeArrayManager arrayManager,
            SkinnedMeshRenderer skinnedMeshRenderer,
            Dictionary<GameObject, VrmLib.Node> nodes,
            GameObject root)
        {
            if (skinnedMeshRenderer.bones == null || skinnedMeshRenderer.bones.Length == 0)
            {
                return null;
            }

            var skin = new VrmLib.Skin();
            skin.InverseMatrices = ToBufferAccessor(arrayManager, skinnedMeshRenderer.sharedMesh.bindposes);
            if (skinnedMeshRenderer.rootBone != null)
            {
                skin.Root = nodes[skinnedMeshRenderer.rootBone.gameObject];
            }

            skin.Joints = skinnedMeshRenderer.bones.Select(x =>
            {
                if (x != null)
                {
                    return nodes[x.gameObject];
                }
                else
                {
                    return null;
                }
            }).ToList();
            return skin;
        }

        private static BufferAccessor ToBufferAccessor(INativeArrayManager arrayManager, SkinJoints[] values)
        {
            return ToBufferAccessor(arrayManager, values, AccessorValueType.UNSIGNED_SHORT, AccessorVectorType.VEC4);
        }

        private static BufferAccessor ToBufferAccessor(INativeArrayManager arrayManager, Color[] colors)
        {
            return ToBufferAccessor(arrayManager, colors, AccessorValueType.FLOAT, AccessorVectorType.VEC4);
        }

        private static BufferAccessor ToBufferAccessor(INativeArrayManager arrayManager, Vector4[] vectors)
        {
            return ToBufferAccessor(arrayManager, vectors, AccessorValueType.FLOAT, AccessorVectorType.VEC4);
        }

        private static BufferAccessor ToBufferAccessor(INativeArrayManager arrayManager, Vector3[] vectors)
        {
            return ToBufferAccessor(arrayManager, vectors, AccessorValueType.FLOAT, AccessorVectorType.VEC3);
        }

        private static BufferAccessor ToBufferAccessor(INativeArrayManager arrayManager, Vector2[] vectors)
        {
            return ToBufferAccessor(arrayManager, vectors, AccessorValueType.FLOAT, AccessorVectorType.VEC2);
        }

        private static BufferAccessor ToBufferAccessor(INativeArrayManager arrayManager, int[] scalars)
        {
            return ToBufferAccessor(arrayManager, scalars, AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR);
        }

        private static BufferAccessor ToBufferAccessor(INativeArrayManager arrayManager, Matrix4x4[] matrixes)
        {
            return ToBufferAccessor(arrayManager, matrixes, AccessorValueType.FLOAT, AccessorVectorType.MAT4);
        }

        private static BufferAccessor ToBufferAccessor<T>(INativeArrayManager arrayManager, T[] value, AccessorValueType valueType, AccessorVectorType vectorType) where T : struct
        {
            return new BufferAccessor(arrayManager,
                arrayManager.CreateNativeArray(value).Reinterpret<byte>(Marshal.SizeOf<T>()),
                valueType,
                vectorType,
                value.Length
                );
        }
    }
}
