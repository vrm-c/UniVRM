using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UniGLTF.MeshUtility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace VRM
{
    /// <summary>
    /// 複数のメッシュを統合する。
    /// VRM の場合、BlendShapeClip の調整が必用なのでこれも実行する。
    /// </summary>
    [DisallowMultipleComponent]
    public static class MeshIntegratorEditor
    {
        const string ASSET_SUFFIX = ".mesh.asset";

        public static bool IntegrateValidation()
        {
            return Selection.activeObject != null &&
                   Selection.activeObject is GameObject &&
                   SkinnedMeshUtility.IsPrefab(Selection.activeObject);
        }

        public static List<MeshIntegrationResult> Integrate(GameObject prefab, UniGLTF.UnityPath writeAssetPath, IEnumerable<Mesh> excludes)
        {
            Undo.RecordObject(prefab, "Mesh Integration");
            var instance = SkinnedMeshUtility.InstantiatePrefab(prefab);

            var clips = new List<BlendShapeClip>();
            var proxy = instance.GetComponent<VRMBlendShapeProxy>();
            if (proxy != null && proxy.BlendShapeAvatar != null)
            {
                clips = proxy.BlendShapeAvatar.Clips;
            }
            foreach (var clip in clips)
            {
                Undo.RecordObject(clip, "Mesh Integration");
            }

            // Backup Exists
            BackupVrmPrefab(prefab);

            // Execute
            var results = VRMMeshIntegratorUtility.Integrate(instance, clips, excludes);

            // disable source renderer
            foreach (var res in results)
            {
                foreach (var renderer in res.SourceSkinnedMeshRenderers)
                {
                    Undo.RecordObject(renderer.gameObject, "Deactivate old renderer");
                    renderer.gameObject.SetActive(false);
                }

                foreach (var renderer in res.SourceMeshRenderers)
                {
                    Undo.RecordObject(renderer.gameObject, "Deactivate old renderer");
                    renderer.gameObject.SetActive(false);
                }
            }

            foreach (var result in results)
            {
                if (result.IntegratedRenderer == null) continue;

                var assetPath = writeAssetPath.Parent.Child($"{result.IntegratedRenderer.gameObject.name}{ASSET_SUFFIX}");
                Debug.LogFormat("CreateAsset: {0}", assetPath);
                assetPath.CreateAsset(result.IntegratedRenderer.sharedMesh);
                Undo.RegisterCreatedObjectUndo(result.IntegratedRenderer.gameObject, "Integrate Renderers");
            }

            // Apply to Prefab
            var prefabPath = UniGLTF.UnityPath.FromUnityPath(AssetDatabase.GetAssetPath(prefab));
            if (prefabPath.Equals(writeAssetPath))
            {
                SkinnedMeshUtility.ApplyChangesToPrefab(instance);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(instance, writeAssetPath.Value, out bool success);
                if (!success)
                {
                    throw new System.Exception($"PrefabUtility.SaveAsPrefabAsset: {writeAssetPath}");
                }
            }

            // destroy source renderers
            Object.DestroyImmediate(instance);

            return results;
        }

        private static void BackupVrmPrefab(GameObject rootPrefab)
        {
            var proxy = rootPrefab.GetComponent<VRMBlendShapeProxy>();

            var srcAvatar = proxy.BlendShapeAvatar;
            var dstAvatar = (BlendShapeAvatar)BackupAsset(srcAvatar, rootPrefab);

            var clipMapper = srcAvatar.Clips.ToDictionary(x => x, x => (BlendShapeClip)BackupAsset(x, rootPrefab));
            dstAvatar.Clips = clipMapper.Values.ToList();

            var dstPrefab = BackupAsset(rootPrefab, rootPrefab);
            var dstInstance = SkinnedMeshUtility.InstantiatePrefab(dstPrefab);
            dstInstance.GetComponent<VRMBlendShapeProxy>().BlendShapeAvatar = dstAvatar;
            SkinnedMeshUtility.ApplyChangesToPrefab(dstInstance);
            Object.DestroyImmediate(dstInstance);
        }

        private static T BackupAsset<T>(T asset, GameObject rootPrefab) where T : UnityEngine.Object
        {
            var srcAssetPath = UnityPath.FromAsset(asset);
            var assetName = srcAssetPath.FileName;

            var backupDir = "MeshIntegratorBackup";
            var backupPath = UnityPath.FromAsset(rootPrefab).Parent.Child(backupDir);
            backupPath.EnsureFolder();
            var dstAssetPath = backupPath.Child(assetName);

            AssetDatabase.CopyAsset(srcAssetPath.Value, dstAssetPath.Value);
            return dstAssetPath.LoadAsset<T>();
        }

        private static string GetRootPrefabPath(GameObject go)
        {
            var prefab = SkinnedMeshUtility.IsPrefab(go) ? go : SkinnedMeshUtility.GetPrefab(go);
            var assetPath = "";
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/", Path.GetDirectoryName(prefabPath));
            }
            else
            {
                assetPath = string.Format("Assets/");
            }
            assetPath = assetPath.Replace(@"\", @"/");
            return assetPath;
        }
    }
}
