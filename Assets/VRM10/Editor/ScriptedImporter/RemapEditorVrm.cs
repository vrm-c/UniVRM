using UniGLTF;
using UnityEngine;
using System.Linq;
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

        public void OnGUI(ScriptedImporter importer, GltfData data, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm)
        {
            if (CanExtract(importer))
            {
                if (GUILayout.Button("Extract Meta And Expressions ..."))
                {
                    Extract(importer, data);
                }
                EditorGUILayout.HelpBox("Extract subasset to external object and overwrite remap", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Clear extraction"))
                {
                    ClearExternalObjects(importer, typeof(VRM10Object), typeof(VRM10Expression));
                }
                EditorGUILayout.HelpBox("Clear remap. All remap use subAsset", MessageType.Info);
            }

            DrawRemapGUI<VRM10Object>(importer.GetExternalObjectMap());
            DrawRemapGUI<VRM10Expression>(importer.GetExternalObjectMap());
        }

        /// <summary>
        /// 
        /// * VRM10Object
        /// * VRM10Expression[]
        /// 
        /// が Extract 対象となる
        /// 
        /// </summary>
        public static void Extract(ScriptedImporter importer, GltfData data)
        {
            if (string.IsNullOrEmpty(importer.assetPath))
            {
                return;
            }

            var path = GetAndCreateFolder(importer.assetPath, ".vrm1.Assets");

            var assets = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);
            var prefab = assets.First(x => x is GameObject) as GameObject;

            // expression を extract し置き換え map を作る
            var map = new Dictionary<VRM10Expression, VRM10Expression>();
            foreach (var asset in assets)
            {
                if (asset is VRM10Expression expression)
                {
                    // preview用のprefab
                    expression.Prefab = prefab;

                    var clone = ExtractSubAsset(asset, $"{path}/{asset.name}.asset", false);
                    map.Add(expression, clone as VRM10Expression);
                }
            }

            // vrmObject の expression を置き換える
            var vrmObject = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).First(x => x is VRM10Object) as VRM10Object;
            vrmObject.Expression.Replace(map);
            vrmObject.Prefab = prefab; // for FirstPerson Editor

            // extract
            ExtractSubAsset(vrmObject, $"{path}/{vrmObject.name}.asset", false);

            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
