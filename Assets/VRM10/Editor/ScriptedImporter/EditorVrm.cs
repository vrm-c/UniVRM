using UniGLTF;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor;
using VRMShaders;
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
            importer.DrawRemapGUI<VRM10MetaObject>(new string[] { VRM10MetaObject.ExtractKey });

            // expression avatar
            importer.DrawRemapGUI<VRM10ExpressionAvatar>(new string[] { VRM10ExpressionAvatar.ExtractKey });

            // expressions
            importer.DrawRemapGUI<VRM10Expression>(vrm.Expressions.Select(x => CreateKey(x).ExtractKey));

            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects<VRM10MetaObject>();
                importer.ClearExternalObjects<VRM10ExpressionAvatar>();
                importer.ClearExternalObjects<VRM10Expression>();
            }
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

            var path = $"{Path.GetDirectoryName(importer.assetPath)}/{Path.GetFileNameWithoutExtension(importer.assetPath)}.Extracted";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // meta
            {
                foreach (var asset in importer.GetSubAssets<VRM10MetaObject>(importer.assetPath))
                {
                    asset.ExtractSubAsset($"{path}/{asset.name}.asset", false);
                }
            }

            {
                // expressions
                foreach (var asset in importer.GetSubAssets<VRM10Expression>(importer.assetPath))
                {
                    asset.ExtractSubAsset($"{path}/{asset.name}.asset", false);
                }

                // expressions
                foreach (var asset in importer.GetSubAssets<VRM10ExpressionAvatar>(importer.assetPath))
                {
                    asset.ExtractSubAsset($"{path}/{asset.name}.asset", false);
                }

                // external な expressionAvatar.Clips に 再代入する
                var expressionAvatar = importer.GetExternalObjectMap().Select(x => x.Value as VRM10ExpressionAvatar).FirstOrDefault(x => x != null);
                var expressions = importer.GetExternalObjectMap().Select(x => x.Value as VRM10Expression).Where(x => x != null).ToList();
                expressionAvatar.Clips = expressions;
                var avatarPath = AssetDatabase.GetAssetPath(expressionAvatar);
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    EditorUtility.SetDirty(expressionAvatar);
                    AssetDatabase.WriteImportSettingsIfDirty(avatarPath);
                }
            }

            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
