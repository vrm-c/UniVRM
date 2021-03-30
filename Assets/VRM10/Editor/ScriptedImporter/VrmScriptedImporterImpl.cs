using System.Linq;
using UnityEngine;
using UniGLTF;
using System.IO;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    public static class VrmScriptedImporterImpl
    {
        /// <summary>
        /// VRM1 で　パースし、失敗したら Migration してから VRM1 でパースする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="migrateToVrm1"></param>
        /// <returns></returns>
        static GltfParser Parse(string path, bool migrateToVrm1)
        {
            //
            // Parse(parse glb, parser gltf json)
            //
            var parser = new GltfParser();
            parser.ParsePath(path);
            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                return parser;
            }

            if (migrateToVrm1)
            {
                // try migrateion
                var migrated = MigrationVrm.Migrate(File.ReadAllBytes(path));
                parser = new GltfParser();
                parser.Parse(path, migrated);
                return parser;
            }

            return null;
        }

        public static void Import(ScriptedImporter scriptedImporter, AssetImportContext context, bool migrateToVrm1)
        {
#if VRM_DEVELOP            
            Debug.Log("OnImportAsset to " + scriptedImporter.assetPath);
#endif

            var parser = Parse(scriptedImporter.assetPath, migrateToVrm1);
            if (parser == null)
            {
                // fail to parse vrm1
                return;
            }

            //
            // Import(create unity objects)
            //
            var externalObjectMap = scriptedImporter.GetExternalObjectMap().Select(kv => (kv.Value.name, kv.Value)).ToArray();

            using (var loader = new RuntimeUnityBuilder(parser, externalObjectMap))
            {
                // settings TextureImporters
                foreach (var textureInfo in Vrm10MToonMaterialImporter.EnumerateAllTexturesDistinct(parser))
                {
                    TextureImporterConfigurator.Configure(textureInfo, loader.TextureFactory.ExternalMap);
                }

                loader.Load();
                loader.ShowMeshes();

                loader.TransferOwnership(o =>
                {
                    context.AddObjectToAsset(o.name, o);
                    if (o is GameObject)
                    {
                        // Root GameObject is main object
                        context.SetMainObject(loader.Root);
                    }

                    return true;
                });
            }
        }

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


        // public void ExtractTextures()
        // {
        //     this.ExtractTextures(TextureDirName, (path) =>
        //     {
        //         var parser = new UniGLTF.GltfParser();
        //         parser.ParsePath(path);
        //         return VrmLoader.CreateVrmModel(parser);
        //     });
        //     AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public void ExtractMaterials()
        // {
        //     this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat");
        //     AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public void ExtractMaterialsAndTextures()
        // {
        //     this.ExtractTextures(TextureDirName, (path) =>
        //     {
        //         var parser = new UniGLTF.GltfParser();
        //         parser.ParsePath(path);
        //         return VrmLoader.CreateVrmModel(parser);
        //     }, () =>
        //     {
        //         this.ExtractAssets<UnityEngine.Material>(MaterialDirName, ".mat");
        //     });
        //     AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public void ExtractMeta()
        // {
        //     this.ExtractAssets<UniVRM10.VRM10MetaObject>(MetaDirName, ".asset");
        //     var metaObject = this.GetExternalUnityObjects<UniVRM10.VRM10MetaObject>().FirstOrDefault();
        //     var metaObjectPath = AssetDatabase.GetAssetPath(metaObject.Value);
        //     if (!string.IsNullOrEmpty(metaObjectPath))
        //     {
        //         EditorUtility.SetDirty(metaObject.Value);
        //         AssetDatabase.WriteImportSettingsIfDirty(metaObjectPath);
        //     }
        //     AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public void ExtractExpressions()
        // {
        //     this.ExtractAssets<UniVRM10.VRM10ExpressionAvatar>(ExpressionDirName, ".asset");
        //     this.ExtractAssets<UniVRM10.VRM10Expression>(ExpressionDirName, ".asset");

        //     var expressionAvatar = this.GetExternalUnityObjects<UniVRM10.VRM10ExpressionAvatar>().FirstOrDefault();
        //     var expressions = this.GetExternalUnityObjects<UniVRM10.VRM10Expression>();

        //     expressionAvatar.Value.Clips = expressions.Select(x => x.Value).ToList();
        //     var avatarPath = AssetDatabase.GetAssetPath(expressionAvatar.Value);
        //     if (!string.IsNullOrEmpty(avatarPath))
        //     {
        //         EditorUtility.SetDirty(expressionAvatar.Value);
        //         AssetDatabase.WriteImportSettingsIfDirty(avatarPath);
        //     }

        //     AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public Dictionary<string, T> GetExternalUnityObjects<T>() where T : UnityEngine.Object
        // {
        //     return this.GetExternalObjectMap().Where(x => x.Key.type == typeof(T)).ToDictionary(x => x.Key.name, x => (T)x.Value);
        // }

        // public void SetExternalUnityObject<T>(UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object
        // {
        //     this.AddRemap(sourceAssetIdentifier, obj);
        //     AssetDatabase.WriteImportSettingsIfDirty(this.assetPath);
        //     AssetDatabase.ImportAsset(this.assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public static void ClearExternalObjects<T>(this ScriptedImporter importer) where T : UnityEngine.Object
        // {
        //     foreach (var extarnalObject in importer.GetExternalObjectMap().Where(x => x.Key.type == typeof(T)))
        //     {
        //         importer.RemoveRemap(extarnalObject.Key);
        //     }

        //     AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        //     AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // public static void ClearExtarnalObjects(this ScriptedImporter importer)
        // {
        //     foreach (var extarnalObject in importer.GetExternalObjectMap())
        //     {
        //         importer.RemoveRemap(extarnalObject.Key);
        //     }

        //     AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        //     AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        // }

        // private static T GetSubAsset<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        // {
        //     return importer.GetSubAssets<T>(assetPath)
        //         .FirstOrDefault();
        // }

        // public static IEnumerable<T> GetSubAssets<T>(this ScriptedImporter importer, string assetPath) where T : UnityEngine.Object
        // {
        //     return AssetDatabase
        //         .LoadAllAssetsAtPath(assetPath)
        //         .Where(x => AssetDatabase.IsSubAsset(x))
        //         .Where(x => x is T)
        //         .Select(x => x as T);
        // }

        // private static void ExtractFromAsset(UnityEngine.Object subAsset, string destinationPath, bool isForceUpdate)
        // {
        //     string assetPath = AssetDatabase.GetAssetPath(subAsset);

        //     var clone = UnityEngine.Object.Instantiate(subAsset);
        //     AssetDatabase.CreateAsset(clone, destinationPath);

        //     var assetImporter = AssetImporter.GetAtPath(assetPath);
        //     assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(subAsset), clone);

        //     if (isForceUpdate)
        //     {
        //         AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        //         AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        //     }
        // }

        // public static void ExtractAssets<T>(this ScriptedImporter importer, string dirName, string extension) where T : UnityEngine.Object
        // {
        //     if (string.IsNullOrEmpty(importer.assetPath))
        //         return;

        //     var subAssets = importer.GetSubAssets<T>(importer.assetPath);

        //     var path = string.Format("{0}/{1}.{2}",
        //         Path.GetDirectoryName(importer.assetPath),
        //         Path.GetFileNameWithoutExtension(importer.assetPath),
        //         dirName
        //         );

        //     var info = importer.SafeCreateDirectory(path);

        //     foreach (var asset in subAssets)
        //     {
        //         ExtractFromAsset(asset, string.Format("{0}/{1}{2}", path, asset.name, extension), false);
        //     }
        // }


        // public static void ExtractTextures(this ScriptedImporter importer, string dirName, Func<string, VrmLib.Model> CreateModel, Action onComplited = null)
        // {
        //     if (string.IsNullOrEmpty(importer.assetPath))
        //         return;

        //     var subAssets = importer.GetSubAssets<UnityEngine.Texture2D>(importer.assetPath);

        //     var path = string.Format("{0}/{1}.{2}",
        //         Path.GetDirectoryName(importer.assetPath),
        //         Path.GetFileNameWithoutExtension(importer.assetPath),
        //         dirName
        //         );

        //     importer.SafeCreateDirectory(path);

        //     Dictionary<VrmLib.ImageTexture, string> targetPaths = new Dictionary<VrmLib.ImageTexture, string>();

        //     // Reload Model
        //     var model = CreateModel(importer.assetPath);
        //     var mimeTypeReg = new System.Text.RegularExpressions.Regex("image/(?<mime>.*)$");
        //     int count = 0;
        //     foreach (var texture in model.Textures)
        //     {
        //         var imageTexture = texture as VrmLib.ImageTexture;
        //         if (imageTexture == null) continue;

        //         var mimeType = mimeTypeReg.Match(imageTexture.Image.MimeType);
        //         var assetName = !string.IsNullOrEmpty(imageTexture.Name) ? imageTexture.Name : string.Format("{0}_img{1}", model.Root.Name, count);
        //         var targetPath = string.Format("{0}/{1}.{2}",
        //             path,
        //             assetName,
        //             mimeType.Groups["mime"].Value);
        //         imageTexture.Name = assetName;

        //         if (imageTexture.TextureType == VrmLib.Texture.TextureTypes.MetallicRoughness
        //             || imageTexture.TextureType == VrmLib.Texture.TextureTypes.Occlusion)
        //         {
        //             var subAssetTexture = subAssets.Where(x => x.name == imageTexture.Name).FirstOrDefault();
        //             File.WriteAllBytes(targetPath, subAssetTexture.EncodeToPNG());
        //         }
        //         else
        //         {
        //             File.WriteAllBytes(targetPath, imageTexture.Image.Bytes.ToArray());
        //         }

        //         AssetDatabase.ImportAsset(targetPath);
        //         targetPaths.Add(imageTexture, targetPath);

        //         count++;
        //     }

        //     EditorApplication.delayCall += () =>
        //     {
        //         foreach (var targetPath in targetPaths)
        //         {
        //             var imageTexture = targetPath.Key;
        //             var targetTextureImporter = AssetImporter.GetAtPath(targetPath.Value) as TextureImporter;
        //             targetTextureImporter.sRGBTexture = (imageTexture.ColorSpace == VrmLib.Texture.ColorSpaceTypes.Srgb);
        //             if (imageTexture.TextureType == VrmLib.Texture.TextureTypes.NormalMap)
        //             {
        //                 targetTextureImporter.textureType = TextureImporterType.NormalMap;
        //             }
        //             targetTextureImporter.SaveAndReimport();

        //             var externalObject = AssetDatabase.LoadAssetAtPath(targetPath.Value, typeof(UnityEngine.Texture2D));
        //             importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(UnityEngine.Texture2D), imageTexture.Name), externalObject);
        //         }

        //         //AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        //         AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);

        //         if (onComplited != null)
        //         {
        //             onComplited();
        //         }
        //     };
        // }
    }
}
