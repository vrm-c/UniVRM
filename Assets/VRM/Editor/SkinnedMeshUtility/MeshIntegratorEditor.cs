using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace VRM
{
    /// <summary>
    /// 複数のメッシュをまとめて、BlendShapeClipに変更を反映する
    /// </summary>
    [DisallowMultipleComponent]
    public static class MeshIntegratorEditor
    {
        const string MENU_KEY = VRMVersion.MENU + "/MeshIntegrator";
        const string ASSET_SUFFIX = ".mesh.asset";

        [MenuItem(MENU_KEY, true)]
        private static bool ExportValidate()
        {
            return Selection.activeObject != null &&
                   Selection.activeObject is GameObject &&
                   SkinnedMeshUtility.IsPrefab(Selection.activeObject);
        }

        [MenuItem(MENU_KEY, priority = 1)]
        private static void ExportFromMenu()
        {
            var go = Selection.activeObject as GameObject;

            Integrate(go);
        }

        //[MenuItem("Assets/fooa", false)]
        //private static void Foo()
        //{
        //    var go = Selection.activeObject as GameObject;

        //    Debug.Log(SkinnedMeshUtility.IsPrefab(go));
        //}


        public static List<MeshUtility.MeshIntegrationResult> Integrate(GameObject prefab)
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
            var results = VRMMeshIntegratorUtility.Integrate(instance, clips);

            foreach (var res in results)
            {
                if (res.IntegratedRenderer == null) continue;

                SaveMeshAsset(res.IntegratedRenderer.sharedMesh, instance, res.IntegratedRenderer.gameObject.name);
                Undo.RegisterCreatedObjectUndo(res.IntegratedRenderer.gameObject, "Integrate Renderers");
            }

            // destroy source renderers
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

            // Apply to Prefab
            SkinnedMeshUtility.ApplyChangesToPrefab(instance);
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

        private static void SaveMeshAsset(Mesh mesh, GameObject go, string name)
        {
            var assetPath = Path.Combine(GetRootPrefabPath(go), string.Format("{0}{1}",
                name,
                ASSET_SUFFIX
            ));

            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(mesh, assetPath);
        }

    }
}
