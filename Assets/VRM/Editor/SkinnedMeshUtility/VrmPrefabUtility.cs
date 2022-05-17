using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using UniGLTF.MeshUtility;

namespace VRM
{
    public static class VrmPrefabUtility
    {
        const string BACKUP_DIR = "MeshIntegratorBackup";

        public static GameObject InstantiatePrefab(GameObject prefab)
        {
            if (!IsPrefab(prefab)) return null;

            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        static bool IsPrefab(Object instance)
        {
            if (instance == null)
            {
                return false;
            }
            if (PrefabUtility.GetPrefabAssetType(instance) != PrefabAssetType.Regular)
            {
                return false;
            }
            return true;
        }

        public static void ApplyChangesToPrefab(GameObject instance)
        {
            var prefab = instance.GetPrefab();
            if (prefab == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
        }

        /// <summary>
        /// VRM prefab を ${prefab_dir}/MeshIntegratorBackup/ に複製する。
        /// 
        /// * prefab
        /// * BlendShapeAvatar
        /// * BlendShapeClip
        /// 
        /// が複製される。
        /// </summary>
        /// <param name="go"></param>
        public static void BackupVrmPrefab(GameObject go)
        {
            var prefab = go.GetGameObjectType() == GameObjectType.AssetPrefab ? go : PrefabUtility.GetCorrespondingObjectFromSource(go);
            var prefabPath = UnityPath.FromAsset(prefab);

            var proxy = go.GetComponent<VRMBlendShapeProxy>();

            var srcAvatar = proxy.BlendShapeAvatar;
            var dstAvatar = (BlendShapeAvatar)BackupAsset(srcAvatar, prefabPath);

            var clipMapper = srcAvatar.Clips.ToDictionary(x => x, x => (BlendShapeClip)BackupAsset(x, prefabPath));
            dstAvatar.Clips = clipMapper.Values.ToList();

            var dstPrefab = BackupAsset(prefab, prefabPath);
            var dstInstance = InstantiatePrefab(dstPrefab);
            dstInstance.GetComponent<VRMBlendShapeProxy>().BlendShapeAvatar = dstAvatar;
            ApplyChangesToPrefab(dstInstance);
            Object.DestroyImmediate(dstInstance);
        }

        /// <summary>
        /// asset を ${prefab_dir}/MeshIntegratorBackup/ にコピーし、コピーしたアセットをロードして返す
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="rootPrefab"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T BackupAsset<T>(T asset, UnityPath prefabPath) where T : UnityEngine.Object
        {
            var srcAssetPath = UnityPath.FromAsset(asset);
            var assetName = srcAssetPath.FileName;

            var backupPath = prefabPath.Parent.Child(BACKUP_DIR);
            backupPath.EnsureFolder();
            var dstAssetPath = backupPath.Child(assetName);

            AssetDatabase.CopyAsset(srcAssetPath.Value, dstAssetPath.Value);
            return dstAssetPath.LoadAsset<T>();
        }
    }
}
