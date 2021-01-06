using System;
using System.Collections.Generic;
using System.Linq;
using MeshUtility;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{
    /// <summary>
    /// VrmLib.Model から UnityPrefab を構築する
    /// </summary>
    public static class RuntimeUnityBuilder
    {
        /// <summary>
        /// モデル(Transform + Renderer)を構築する。
        /// <summary>
        public static ModelAsset ToUnityAsset(VrmLib.Model model, bool showMesh = true)
        {
            var modelAsset = new ModelAsset();

            // texture
            for (int i = 0; i < model.Textures.Count; ++i)
            {
                var src = model.Textures[i];
                var name = !string.IsNullOrEmpty(src.Name)
                    ? src.Name
                    : string.Format("{0}_img{1}", model.Root.Name, i);
                if (src is VrmLib.ImageTexture imageTexture)
                {
                    var texture = CreateTexture(imageTexture);
                    texture.name = name;
                    modelAsset.Map.Textures.Add(src, texture);
                    modelAsset.Textures.Add(texture);
                }
                else
                {
                    Debug.LogWarning($"{name} not ImageTexture");
                }
            }

            // material
            foreach (var src in model.Materials)
            {
                // TODO: material has VertexColor
                var material = RuntimeUnityMaterialBuilder.CreateMaterialAsset(src, hasVertexColor: false, modelAsset.Map.Textures);
                material.name = src.Name;
                modelAsset.Map.Materials.Add(src, material);
                modelAsset.Materials.Add(material);
            }

            // mesh
            for (int i = 0; i < model.MeshGroups.Count; ++i)
            {
                var src = model.MeshGroups[i];
                if (src.Meshes.Count == 1)
                {
                    // submesh 方式
                    var mesh = new UnityEngine.Mesh();
                    mesh.name = src.Name;
                    mesh.LoadMesh(src.Meshes[0], src.Skin);
                    modelAsset.Map.Meshes.Add(src, mesh);
                    modelAsset.Meshes.Add(mesh);
                }
                else
                {
                    // 頂点バッファの連結が必用
                    throw new NotImplementedException();
                }
            }

            // node: recursive
            CreateNodes(model.Root, null, modelAsset.Map.Nodes);
            modelAsset.Root = modelAsset.Map.Nodes[model.Root];

            // renderer
            var map = modelAsset.Map;
            foreach (var (node, go) in map.Nodes)
            {
                if (node.MeshGroup is null)
                {
                    continue;
                }

                if (node.MeshGroup.Meshes.Count > 1)
                {
                    throw new NotImplementedException("invalid isolated vertexbuffer");
                }

                var renderer = CreateRenderer(node, go, map);
                if (!showMesh)
                {
                    renderer.enabled = false;
                }
                map.Renderers.Add(node, renderer);
                modelAsset.Renderers.Add(renderer);
            }

            var humanoid = modelAsset.Root.AddComponent<MeshUtility.Humanoid>();
            humanoid.AssignBones(map.Nodes.Select(x => (x.Key.HumanoidBone.GetValueOrDefault().ToUnity(), x.Value.transform)));
            modelAsset.HumanoidAvatar = humanoid.CreateAvatar();
            modelAsset.HumanoidAvatar.name = "VRM";

            var animator = modelAsset.Root.AddComponent<Animator>();
            animator.avatar = modelAsset.HumanoidAvatar;

            return modelAsset;
        }

        public static HumanBodyBones ToUnity(this VrmLib.HumanoidBones bone)
        {
            if (bone == VrmLib.HumanoidBones.unknown)
            {
                return HumanBodyBones.LastBone;
            }
            return VrmLib.EnumUtil.Cast<HumanBodyBones>(bone);
        }

        private static RenderTextureReadWrite GetRenderTextureReadWrite(VrmLib.Texture.ColorSpaceTypes type)
        {
            return (type == VrmLib.Texture.ColorSpaceTypes.Linear) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;
        }

        /// <summary>
        /// 画像のバイト列からテクスチャを作成する
        /// <summary>
        public static Texture2D CreateTexture(VrmLib.ImageTexture imageTexture)
        {
            Texture2D dstTexture = null;
            UnityEngine.Material convertMaterial = null;
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, imageTexture.ColorSpace == VrmLib.Texture.ColorSpaceTypes.Linear);
            texture.LoadImage(imageTexture.Image.Bytes.ToArray());

            // Convert Texture Gltf to Unity
            if (imageTexture.TextureType == VrmLib.Texture.TextureTypes.NormalMap)
            {
                convertMaterial = TextureConvertMaterial.GetNormalMapConvertGltfToUnity();
                dstTexture = UnityTextureUtil.CopyTexture(
                    texture,
                    GetRenderTextureReadWrite(imageTexture.ColorSpace),
                    convertMaterial);
            }
            else if (imageTexture.TextureType == VrmLib.Texture.TextureTypes.MetallicRoughness)
            {
                var metallicRoughnessImage = imageTexture as VrmLib.MetallicRoughnessImageTexture;
                convertMaterial = TextureConvertMaterial.GetMetallicRoughnessGltfToUnity(metallicRoughnessImage.RoughnessFactor);
                dstTexture = UnityTextureUtil.CopyTexture(
                    texture,
                    GetRenderTextureReadWrite(imageTexture.ColorSpace),
                    convertMaterial);
            }
            else if (imageTexture.TextureType == VrmLib.Texture.TextureTypes.Occlusion)
            {
                convertMaterial = TextureConvertMaterial.GetOcclusionGltfToUnity();
                dstTexture = UnityTextureUtil.CopyTexture(
                    texture,
                    GetRenderTextureReadWrite(imageTexture.ColorSpace),
                    convertMaterial);
            }

            if (dstTexture != null)
            {
                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
                texture = dstTexture;
            }

            if (convertMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(convertMaterial);
            }

            return texture;
        }

        /// <summary>
        /// ヒエラルキーを再帰的に構築する
        /// <summary>
        public static void CreateNodes(VrmLib.Node node, GameObject parent, Dictionary<VrmLib.Node, GameObject> nodes)
        {
            GameObject go = new GameObject(node.Name);
            go.transform.SetPositionAndRotation(node.Translation.ToUnityVector3(), node.Rotation.ToUnityQuaternion());
            nodes.Add(node, go);
            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
            }

            if (node.Children.Count > 0)
            {
                for (int n = 0; n < node.Children.Count; n++)
                {
                    CreateNodes(node.Children[n], go, nodes);
                }
            }
        }

        /// <summary>
        /// MeshFilter + MeshRenderer もしくは SkinnedMeshRenderer を構築する
        /// </summary>
        public static Renderer CreateRenderer(VrmLib.Node node, GameObject go, ModelMap map)
        {
            var mesh = node.MeshGroup.Meshes[0];

            Renderer renderer = null;
            var hasBlendShape = mesh.MorphTargets.Any();
            if (node.MeshGroup.Skin != null || hasBlendShape)
            {
                var skinnedMeshRenderer = go.AddComponent<SkinnedMeshRenderer>();
                renderer = skinnedMeshRenderer;
                skinnedMeshRenderer.sharedMesh = map.Meshes[node.MeshGroup];
                if (node.MeshGroup.Skin != null)
                {
                    skinnedMeshRenderer.bones = node.MeshGroup.Skin.Joints.Select(x => map.Nodes[x].transform).ToArray();
                    if (node.MeshGroup.Skin.Root != null)
                    {
                        skinnedMeshRenderer.rootBone = map.Nodes[node.MeshGroup.Skin.Root].transform;
                    }
                }
            }
            else
            {
                var meshFilter = go.AddComponent<MeshFilter>();
                renderer = go.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = map.Meshes[node.MeshGroup];
            }
            var materials = mesh.Submeshes.Select(x => map.Materials[x.Material]).ToArray();
            renderer.sharedMaterials = materials;

            return renderer;
        }
    }
}
