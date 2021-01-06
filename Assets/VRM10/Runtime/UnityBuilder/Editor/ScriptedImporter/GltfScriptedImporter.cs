using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using VrmLib;

namespace UniVRM10
{

    [ScriptedImporter(1, "glb")]
    public class GltfScriptedImporter : ScriptedImporter, IExternalUnityObject
    {
        const string TextureDirName = "Textures";
        const string MaterialDirName = "Materials";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            Debug.Log("OnImportAsset to " + ctx.assetPath);

            try
            {
                // Create model
                VrmLib.Model model = CreateGlbModel(ctx.assetPath);
                Debug.Log($"ModelLoader.Load: {model}");

                // Build Unity Model
                var assets = EditorUnityBuilder.ToUnityAsset(model, assetPath, this);

                // Texture
                var externalTextures = this.GetExternalUnityObjects<UnityEngine.Texture2D>();
                foreach (var texture in assets.Textures)
                {
                    if (texture == null)
                        continue;

                    if (externalTextures.ContainsValue(texture))
                    {
                    }
                    else
                    {
                        ctx.AddObjectToAsset(texture.name, texture);
                    }
                }

                // Material
                var externalMaterials = this.GetExternalUnityObjects<UnityEngine.Material>();
                foreach (var material in assets.Materials)
                {
                    if (material == null)
                        continue;

                    if (externalMaterials.ContainsValue(material))
                    {

                    }
                    else
                    {
                        ctx.AddObjectToAsset(material.name, material);
                    }
                }

                // Mesh
                foreach (var mesh in assets.Meshes)
                {
                    ctx.AddObjectToAsset(mesh.name, mesh);
                }

                // Root
                ctx.AddObjectToAsset(assets.Root.name, assets.Root);
                ctx.SetMainObject(assets.Root);

            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private Model CreateGlbModel(string path)
        {
            var bytes = File.ReadAllBytes(path);
            if (!VrmLib.Glb.TryParse(bytes, out VrmLib.Glb glb, out Exception ex))
            {
                throw ex;
            }

            VrmLib.Model model = null;
            VrmLib.IVrmStorage storage;
            storage = new Vrm10Storage(glb.Json.Bytes, glb.Binary.Bytes);
            model = VrmLib.ModelLoader.Load(storage, Path.GetFileNameWithoutExtension(path));
            model.ConvertCoordinate(VrmLib.Coordinates.Unity);

            return model;
        }

        public void ExtractTextures()
        {
            this.ExtractTextures(TextureDirName, (path) => { return CreateGlbModel(path); });
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractMaterials()
        {
            this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat");
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractMaterialsAndTextures()
        {
            this.ExtractTextures(TextureDirName, (path) => { return CreateGlbModel(path); }, () => { this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat"); });
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


