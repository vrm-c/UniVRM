using System;
using UnityEngine;
using System.Linq;
using MeshUtility;
using VrmLib;

namespace UniVRM10
{
    public static class EditorUnityBuilder
    {
        // static public ModelAsset ToUnityAsset(VrmLib.Model model, string assetPath, IExternalUnityObject scriptedImporter)
        // {
        //     var modelAsset = new ModelAsset();
        //     CreateTextureAsset(model, modelAsset, scriptedImporter);
        //     CreateMaterialAsset(model, modelAsset, scriptedImporter);
        //     CreateMeshAsset(model, modelAsset);

        //     // node
        //     RuntimeUnityBuilder.CreateNodes(model.Root, null, modelAsset.Map.Nodes);
        //     modelAsset.Root = modelAsset.Map.Nodes[model.Root];

        //     // renderer
        //     var map = modelAsset.Map;
        //     foreach (var (node, go) in map.Nodes)
        //     {
        //         if (node.MeshGroup is null)
        //         {
        //             continue;
        //         }

        //         if (node.MeshGroup.Meshes.Count > 1)
        //         {
        //             throw new NotImplementedException("invalid isolated vertexbuffer");
        //         }

        //         var renderer = RuntimeUnityBuilder.CreateRenderer(node, go, map);
        //         map.Renderers.Add(node, renderer);
        //         modelAsset.Renderers.Add(renderer);
        //     }

        //     if (model.Vrm != null)
        //     {
        //         // humanoid
        //         var humanoid = modelAsset.Root.AddComponent<MeshUtility.Humanoid>();
        //         humanoid.AssignBones(modelAsset.Map.Nodes.Select(x => (x.Key.HumanoidBone.GetValueOrDefault().ToUnity(), x.Value.transform)));
        //         modelAsset.HumanoidAvatar = humanoid.CreateAvatar();
        //         modelAsset.HumanoidAvatar.name = "VRM";

        //         var animator = modelAsset.Root.AddComponent<Animator>();
        //         animator.avatar = modelAsset.HumanoidAvatar;
        //     }

        //     return modelAsset;
        // }

        // static private void CreateTextureAsset(VrmLib.Model model, ModelAsset modelAsset, IExternalUnityObject scriptedImporter)
        // {
        //     var externalObjects = scriptedImporter.GetExternalUnityObjects<Texture2D>();

        //     // textures
        //     for (int i = 0; i < model.Textures.Count; ++i)
        //     {
        //         if (model.Textures[i] is VrmLib.ImageTexture imageTexture)
        //         {
        //             if (string.IsNullOrEmpty(model.Textures[i].Name))
        //             {
        //                 model.Textures[i].Name = string.Format("{0}_img{1}", model.Root.Name, i);
        //             }
        //             if (externalObjects.ContainsKey(model.Textures[i].Name))
        //             {
        //                 modelAsset.Map.Textures.Add(imageTexture, externalObjects[model.Textures[i].Name]);
        //                 modelAsset.Textures.Add(externalObjects[model.Textures[i].Name]);
        //             }
        //             else
        //             {
        //                 var name = !string.IsNullOrEmpty(imageTexture.Name)
        //                     ? imageTexture.Name
        //                     : string.Format("{0}_img{1}", model.Root.Name, i);

        //                 var texture = RuntimeUnityBuilder.CreateTexture(imageTexture);
        //                 texture.name = name;

        //                 modelAsset.Map.Textures.Add(imageTexture, texture);
        //                 modelAsset.Textures.Add(texture);
        //             }
        //         }
        //         else
        //         {
        //             Debug.LogWarning($"{i} not ImageTexture");
        //         }
        //     }
        // }


        // static private void CreateMaterialAsset(VrmLib.Model model, ModelAsset modelAsset, IExternalUnityObject scriptedImporter)
        // {
        //     var externalObjects = scriptedImporter.GetExternalUnityObjects<UnityEngine.Material>();

        //     foreach (var src in model.Materials)
        //     {
        //         if (externalObjects.ContainsKey(src.Name))
        //         {
        //             modelAsset.Map.Materials.Add(src, externalObjects[src.Name]);
        //             modelAsset.Materials.Add(externalObjects[src.Name]);
        //         }
        //         else
        //         {
        //             // TODO: material has VertexColor
        //             var material = RuntimeUnityMaterialBuilder.CreateMaterialAsset(src, hasVertexColor: false, modelAsset.Map.Textures);
        //             material.name = src.Name;
        //             modelAsset.Map.Materials.Add(src, material);
        //             modelAsset.Materials.Add(material);
        //         }
        //     }
        // }

        // static private void CreateMeshAsset(VrmLib.Model model, ModelAsset modelAsset)
        // {
        //     for (int i = 0; i < model.MeshGroups.Count; ++i)
        //     {
        //         var src = model.MeshGroups[i];
        //         if (src.Meshes.Count == 1)
        //         {
        //             // submesh 方式
        //             var mesh = new UnityEngine.Mesh();
        //             mesh.name = src.Name;
        //             mesh.LoadMesh(src.Meshes[0], src.Skin);
        //             modelAsset.Map.Meshes.Add(src, mesh);
        //             modelAsset.Meshes.Add(mesh);
        //         }
        //         else
        //         {
        //             // 頂点バッファの連結が必用
        //             throw new NotImplementedException();
        //         }
        //     }
        // }
    }
}
