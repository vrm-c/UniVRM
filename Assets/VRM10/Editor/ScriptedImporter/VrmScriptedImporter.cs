using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{

    [ScriptedImporter(1, "vrm")]
    public class VrmScriptedImporter : ScriptedImporter
    {
        const string TextureDirName = "Textures";
        const string MaterialDirName = "Materials";
        const string MetaDirName = "MetaObjects";
        const string ExpressionDirName = "Expressions";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            Debug.Log("OnImportAsset to " + ctx.assetPath);

            var parser = new UniGLTF.GltfParser();
            parser.ParsePath(ctx.assetPath);

            // try
            // {
            //     // Create Vrm Model
            //     VrmLib.Model model = VrmLoader.CreateVrmModel(parser);
            //     if (model == null)
            //     {
            //         // maybe VRM-0.X
            //         return;
            //     }
            //     Debug.Log($"VrmLoader.CreateVrmModel: {model}");

            //     // Build Unity Model
            //     var assets = EditorUnityBuilder.ToUnityAsset(model, assetPath, this);
            //     ComponentBuilder.Build10(model, assets);

            //     // Texture
            //     var externalTextures = this.GetExternalUnityObjects<UnityEngine.Texture2D>();
            //     foreach (var texture in assets.Textures)
            //     {
            //         if (texture == null)
            //             continue;

            //         if (externalTextures.ContainsValue(texture))
            //         {
            //         }
            //         else
            //         {
            //             ctx.AddObjectToAsset(texture.name, texture);
            //         }
            //     }

            //     // Material
            //     var externalMaterials = this.GetExternalUnityObjects<UnityEngine.Material>();
            //     foreach (var material in assets.Materials)
            //     {
            //         if (material == null)
            //             continue;

            //         if (externalMaterials.ContainsValue(material))
            //         {

            //         }
            //         else
            //         {
            //             ctx.AddObjectToAsset(material.name, material);
            //         }
            //     }

            //     // Mesh
            //     foreach (var mesh in assets.Meshes)
            //     {
            //         ctx.AddObjectToAsset(mesh.name, mesh);
            //     }

            //     //// ScriptableObject
            //     // avatar
            //     ctx.AddObjectToAsset("avatar", assets.HumanoidAvatar);

            //     // meta
            //     {
            //         var external = this.GetExternalUnityObjects<UniVRM10.VRM10MetaObject>().FirstOrDefault();
            //         if (external.Value != null)
            //         {
            //             var controller = assets.Root.GetComponent<VRM10Controller>();
            //             if (controller != null)
            //             {
            //                 controller.Meta = external.Value;
            //             }
            //         }
            //         else
            //         {
            //             var meta = assets.ScriptableObjects
            //                 .FirstOrDefault(x => x.GetType() == typeof(UniVRM10.VRM10MetaObject)) as UniVRM10.VRM10MetaObject;
            //             if (meta != null)
            //             {
            //                 meta.name = "meta";
            //                 ctx.AddObjectToAsset(meta.name, meta);
            //             }
            //         }
            //     }

            //     // expression
            //     {
            //         var external = this.GetExternalUnityObjects<UniVRM10.VRM10Expression>();
            //         if (external.Any())
            //         {
            //         }
            //         else
            //         {
            //             var expression = assets.ScriptableObjects
            //                 .Where(x => x.GetType() == typeof(UniVRM10.VRM10Expression))
            //                 .Select(x => x as UniVRM10.VRM10Expression);
            //             foreach (var clip in expression)
            //             {
            //                 clip.name = clip.ExpressionName;
            //                 ctx.AddObjectToAsset(clip.ExpressionName, clip);
            //             }
            //         }
            //     }
            //     {
            //         var external = this.GetExternalUnityObjects<UniVRM10.VRM10ExpressionAvatar>().FirstOrDefault();
            //         if (external.Value != null)
            //         {
            //             var controller = assets.Root.GetComponent<VRM10Controller>();
            //             if (controller != null)
            //             {
            //                 controller.Expression.ExpressionAvatar = external.Value;
            //             }
            //         }
            //         else
            //         {
            //             var expressionAvatar = assets.ScriptableObjects
            //                 .FirstOrDefault(x => x.GetType() == typeof(UniVRM10.VRM10ExpressionAvatar)) as UniVRM10.VRM10ExpressionAvatar;
            //             if (expressionAvatar != null)
            //             {
            //                 expressionAvatar.name = "expressionAvatar";
            //                 ctx.AddObjectToAsset(expressionAvatar.name, expressionAvatar);
            //             }
            //         }
            //     }

            //     // Root
            //     ctx.AddObjectToAsset(assets.Root.name, assets.Root);
            //     ctx.SetMainObject(assets.Root);

            // }
            // catch (System.Exception ex)
            // {
            //     Debug.LogError(ex);
            // }
        }

        public void ExtractTextures()
        {
            this.ExtractTextures(TextureDirName, (path) =>
            {
                var parser = new UniGLTF.GltfParser();
                parser.ParsePath(path);
                return VrmLoader.CreateVrmModel(parser);
            });
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractMaterials()
        {
            this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat");
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractMaterialsAndTextures()
        {
            this.ExtractTextures(TextureDirName, (path) =>
            {
                var parser = new UniGLTF.GltfParser();
                parser.ParsePath(path);
                return VrmLoader.CreateVrmModel(parser);
            }, () =>
            {
                this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat");
            });
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractMeta()
        {
            this.ExtractAssets<UniVRM10.VRM10MetaObject>(MetaDirName, ".asset");
            var metaObject = this.GetExternalUnityObjects<UniVRM10.VRM10MetaObject>().FirstOrDefault();
            var metaObjectPath = AssetDatabase.GetAssetPath(metaObject.Value);
            if (!string.IsNullOrEmpty(metaObjectPath))
            {
                EditorUtility.SetDirty(metaObject.Value);
                AssetDatabase.WriteImportSettingsIfDirty(metaObjectPath);
            }
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void ExtractExpressions()
        {
            this.ExtractAssets<UniVRM10.VRM10ExpressionAvatar>(ExpressionDirName, ".asset");
            this.ExtractAssets<UniVRM10.VRM10Expression>(ExpressionDirName, ".asset");

            var expressionAvatar = this.GetExternalUnityObjects<UniVRM10.VRM10ExpressionAvatar>().FirstOrDefault();
            var expressions = this.GetExternalUnityObjects<UniVRM10.VRM10Expression>();

            expressionAvatar.Value.Clips = expressions.Select(x => x.Value).ToList();
            var avatarPath = AssetDatabase.GetAssetPath(expressionAvatar.Value);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                EditorUtility.SetDirty(expressionAvatar.Value);
                AssetDatabase.WriteImportSettingsIfDirty(avatarPath);
            }

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