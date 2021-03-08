
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace UniGLTF
{
    public static class ScriptedImporterImpl
    {
        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext importerContext, Axises reverseAxis)
        {
#if VRM_DEVELOP            
            Debug.Log("OnImportAsset to " + importerContext.assetPath);
#endif

            //
            // Parse(parse glb, parser gltf json)
            //
            var parser = new GltfParser();
            parser.ParsePath(importerContext.assetPath);

            //
            // Import(create unity objects)
            //
            var context = new ImporterContext(parser, null,
                scriptedImporter.GetExternalObjectMap()
                    .Select(kv => (kv.Key.name, kv.Value))
            );
            context.InvertAxis = reverseAxis;
            context.Load();
            context.ShowMeshes();

            //
            // SubAsset
            //

            // Texture
            foreach (var info in context.TextureFactory.Textures)
            {
                if (info.IsSubAsset)
                {
                    var texture = info.Texture;
                    importerContext.AddObjectToAsset(texture.name, texture);
                }
            }

            // Material
            foreach (var info in context.MaterialFactory.Materials)
            {
                if (info.IsSubAsset)
                {
                    var material = info.Asset;
                    importerContext.AddObjectToAsset(material.name, material);
                }
            }

            // Mesh
            foreach (var mesh in context.Meshes.Select(x => x.Mesh))
            {
                // all mesh is subasset
                importerContext.AddObjectToAsset(mesh.name, mesh);
            }

            // Animation
            foreach (var clip in context.AnimationClips)
            {
                // all animation is subasset
                importerContext.AddObjectToAsset(clip.name, clip);
            }

            // Root GameObject is main object
            importerContext.AddObjectToAsset(context.Root.name, context.Root);
            importerContext.SetMainObject(context.Root);
        }
    }
}
