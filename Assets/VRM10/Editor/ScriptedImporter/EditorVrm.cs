using UniGLTF;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    public static class EditorVrm
    {
        static ExpressionKey CreateKey(UniGLTF.Extensions.VRMC_vrm.Expression expression)
        {
            if (expression.Preset == UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
            {
                return ExpressionKey.CreateCustom(expression.Name);
            }
            else
            {
                return ExpressionKey.CreateFromPreset(expression.Preset);
            }
        }

        public static void OnGUI(ScriptedImporter importer, GltfParser parser, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm)
        {
            var hasExternal = importer.GetExternalObjectMap().Any(x => x.Value is VRM10MetaObject || x.Value is VRM10ExpressionAvatar || x.Value is VRM10Expression);
            using (new TmpGuiEnable(!hasExternal))
            {
                if (GUILayout.Button("Extract Meta And Expressions ..."))
                {
                    Extract(importer, parser);
                }
            }

            // meta
            importer.DrawRemapGUI<VRM10MetaObject>(new SubAssetKey[] { VRM10MetaObject.SubAssetKey });

            // expressions
            importer.DrawRemapGUI<VRM10Expression>(vrm.Expressions.Select(x => CreateKey(x).SubAssetKey));

            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects<VRM10MetaObject>();
                importer.ClearExternalObjects<VRM10Expression>();
            }
        }

        /// <summary>
        /// $"{assetPath without extension}.{folderName}"
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        static string GetAndCreateFolder(string assetPath, string suffix)
        {
            var path = $"{Path.GetDirectoryName(assetPath)}/{Path.GetFileNameWithoutExtension(assetPath)}{suffix}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// SubAssetを外部ファイルに展開する
        /// </summary>
        public static void Extract(ScriptedImporter importer, GltfParser parser)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }

            // meta
            {
                var path = GetAndCreateFolder(importer.assetPath, ".vrm1.Meta");
                foreach (var (key, asset) in importer.GetSubAssets<VRM10MetaObject>(importer.assetPath))
                {
                    asset.ExtractSubAsset($"{path}/{asset.name}.asset", false);
                }
            }

            {
                // expressions
                var path = GetAndCreateFolder(importer.assetPath, ".vrm1.Expressions");
                foreach (var (key, asset) in importer.GetSubAssets<VRM10Expression>(importer.assetPath))
                {
                    asset.ExtractSubAsset($"{path}/{asset.name}.asset", false);
                }

            }

            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
