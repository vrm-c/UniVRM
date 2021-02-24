using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace UniGLTF
{
    [ScriptedImporter(1, "glb")]
    public class GltfScriptedImporter : ScriptedImporter
    {
        const string TextureDirName = "Textures";
        const string MaterialDirName = "Materials";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            Debug.Log("OnImportAsset to " + ctx.assetPath);

            try
            {
                // Parse
                var parser = new GltfParser();
                parser.ParsePath(ctx.assetPath);

                // Build Unity Model
                var context = new ImporterContext(parser, GetExternalObjectMap()
                .Select(kv => new KeyValuePair<string, UnityEngine.Object>(kv.Key.name, kv.Value)));
                context.Load();
                context.ShowMeshes();

                // Texture
                var externalTextures = this.GetExternalUnityObjects<UnityEngine.Texture2D>();
                foreach (var info in context.TextureFactory.Textures)
                {
                    if (!info.UseExternal)
                    {
                        var texture = info.Texture;
                        ctx.AddObjectToAsset(texture.name, texture);
                    }
                }

                // Material
                var externalMaterials = this.GetExternalUnityObjects<UnityEngine.Material>();
                foreach (var material in context.MaterialFactory.Materials)
                {
                    if (material == null)
                    {
                        throw new Exception();
                    }

                    if (!externalMaterials.ContainsValue(material))
                    {
                        ctx.AddObjectToAsset(material.name, material);
                    }
                }

                // Mesh
                foreach (var mesh in context.Meshes.Select(x => x.Mesh))
                {
                    ctx.AddObjectToAsset(mesh.name, mesh);
                }

                // Root
                ctx.AddObjectToAsset(context.Root.name, context.Root);
                ctx.SetMainObject(context.Root);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void ExtractTextures()
        {
            // extract textures to files
            this.ExtractTextures(TextureDirName);
            // reimport
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractMaterials()
        {
            this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat");
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractMaterialsAndTextures()
        {
            this.ExtractTextures(TextureDirName, () => { this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat"); });
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public Dictionary<string, T> GetExternalUnityObjects<T>() where T : UnityEngine.Object
        {
            return this.GetExternalObjectMap().Where(x => x.Key.type == typeof(T)).ToDictionary(x => x.Key.name, x => (T)x.Value);
        }

        public void SetExternalUnityObject<T>(UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        {
            this.AddRemap(sourceAssetIdentifier, obj);
            AssetDatabase.WriteImportSettingsIfDirty(this.assetPath);
            AssetDatabase.ImportAsset(this.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
