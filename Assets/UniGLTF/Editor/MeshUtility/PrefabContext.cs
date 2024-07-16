using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    // Instantiate
    class PrefabContext : IDisposable
    {
        public readonly GameObject Instance;

        readonly UnityPath _assetFolder;

        public bool Keep = false;

        public string AssetFolder => _assetFolder.Value;

        public PrefabContext(GameObject prefab, UnityPath assetFolder)
        {
            this._assetFolder = assetFolder;
            this.Instance = GameObject.Instantiate(prefab);
            if (PrefabUtility.IsOutermostPrefabInstanceRoot(this.Instance))
            {
                // どういう条件でここに来るかはよくわからない
                PrefabUtility.UnpackPrefabInstance(this.Instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        // - Instance を Asset に書き出す
        // - Instance を削除する
        public void Dispose()
        {
            if (Keep)
            {
                // for debug
                return;
            }
            UnityEngine.Object.DestroyImmediate(Instance);
        }

        public static UnityPath GetOutFolder(GameObject _exportTarget)
        {
            // 出力フォルダを決める
            var folder = "Assets";
            var prefab = _exportTarget.GetPrefab();
            if (prefab != null)
            {
                folder = AssetDatabase.GetAssetPath(prefab);
            }

            // 新規で作成されるアセットはすべてこのフォルダの中に作る。上書きチェックはしない
            var assetFolder = EditorUtility.SaveFolderPanel("select asset save folder", Path.GetDirectoryName(folder), "Integrated");
            var unityPath = UniGLTF.UnityPath.FromFullpath(assetFolder);
            if (!unityPath.IsUnderWritableFolder)
            {
                throw new Exception("not in asset folder");
            }
            return unityPath;
        }
    }
}