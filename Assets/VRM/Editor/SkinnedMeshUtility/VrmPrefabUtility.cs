using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;

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
            var prefab = GetPrefab(instance);
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

        static Object GetPrefab(GameObject instance)
        {
#if UNITY_2018_2_OR_NEWER
            return PrefabUtility.GetCorrespondingObjectFromSource(instance);
#else
            return PrefabUtility.GetPrefabParent(go);
#endif
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
        /// <param name="rootPrefab"></param>
        public static void BackupVrmPrefab(GameObject rootPrefab)
        {
            var proxy = rootPrefab.GetComponent<VRMBlendShapeProxy>();

            var srcAvatar = proxy.BlendShapeAvatar;
            var dstAvatar = (BlendShapeAvatar)BackupAsset(srcAvatar, rootPrefab);

            var clipMapper = srcAvatar.Clips.ToDictionary(x => x, x => (BlendShapeClip)BackupAsset(x, rootPrefab));
            dstAvatar.Clips = clipMapper.Values.ToList();

            var dstPrefab = BackupAsset(rootPrefab, rootPrefab);
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
        private static T BackupAsset<T>(T asset, GameObject rootPrefab) where T : UnityEngine.Object
        {
            var srcAssetPath = UnityPath.FromAsset(asset);
            var assetName = srcAssetPath.FileName;

            var backupPath = UnityPath.FromAsset(rootPrefab).Parent.Child(BACKUP_DIR);
            backupPath.EnsureFolder();
            var dstAssetPath = backupPath.Child(assetName);

            AssetDatabase.CopyAsset(srcAssetPath.Value, dstAssetPath.Value);
            return dstAssetPath.LoadAsset<T>();
        }
    }
}
