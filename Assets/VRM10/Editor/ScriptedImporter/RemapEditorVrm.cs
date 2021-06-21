using UniGLTF;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor;
using VRMShaders;
using System.Collections.Generic;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    public class RemapEditorVrm : RemapEditorBase
    {
        public RemapEditorVrm(IEnumerable<SubAssetKey> keys, EditorMapGetterFunc getter, EditorMapSetterFunc setter) : base(keys, getter, setter)
        { }

        public void OnGUI(ScriptedImporter importer, GltfParser parser, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm)
        {
            var hasExternal = importer.GetExternalObjectMap().Any(x => x.Value is VRM10Object || x.Value is VRM10Expression);
            using (new EditorGUI.DisabledScope(hasExternal))
            {
                if (GUILayout.Button("Extract Meta And Expressions ..."))
                {
                    Extract(importer, parser);
                }
            }

            // meta
            DrawRemapGUI<VRM10Object>(importer.GetExternalObjectMap());

            // expressions
            DrawRemapGUI<VRM10Expression>(importer.GetExternalObjectMap());

            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects(
                    typeof(VRM10Object),
                    typeof(VRM10Expression));
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
        /// 
        /// * VRM10Object
        /// * VRM10Expression[]
        /// 
        /// が Extract 対象となる
        /// 
        /// </summary>
        public static void Extract(ScriptedImporter importer, GltfParser parser)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }

            var path = GetAndCreateFolder(importer.assetPath, ".vrm1.Assets");
            {
                var map = new Dictionary<VRM10Expression, VRM10Expression>();
                foreach (var (key, asset) in importer.GetSubAssets<VRM10Expression>(importer.assetPath))
                {
                    var clone = asset.ExtractSubAsset($"{path}/{asset.name}.asset", false);
                    map.Add(asset, clone as VRM10Expression);
                }

                var (_, vrmObject) = importer.GetSubAssets<VRM10Object>(importer.assetPath).First();

                vrmObject.Expression.Replace(map);

                {
                    vrmObject.ExtractSubAsset($"{path}/{vrmObject.name}.asset", false);
                }
            }

            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
