using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    /// <summary>
    /// 複数のメッシュをまとめる
    /// </summary>
    [DisallowMultipleComponent]
    public static class MeshIntegratorEditor
    {
        const string MENU_KEY = SkinnedMeshUtility.MENU_KEY + "MeshIntegrator";
        const string ASSET_SUFFIX = ".mesh.asset";

        [MenuItem(MENU_KEY, true, SkinnedMeshUtility.MENU_PRIORITY)]
        private static bool ExportValidate()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        [MenuItem(MENU_KEY, false, SkinnedMeshUtility.MENU_PRIORITY)]
        private static void ExportFromMenu()
        {
            var go = Selection.activeObject as GameObject;

            Integrate(go);
        }


        public static List<MeshIntegratorUtility.MeshIntegrationResult> Integrate(GameObject go)
        {
            Undo.RecordObject(go, "Mesh Integration");

            var clips = new List<BlendShapeClip>();
            var proxy = go.GetComponent<VRMBlendShapeProxy>();
            if (proxy != null && proxy.BlendShapeAvatar != null)
            {
                clips = proxy.BlendShapeAvatar.Clips;
            }
            foreach (var clip in clips)
            {
                Undo.RecordObject(clip, "Mesh Integration");
            }

            var results = MeshIntegratorUtility.Integrate(go, clips);

            foreach (var res in results)
            {
                if (res.IntegratedRenderer == null) continue;
                
                SaveMeshAsset(res.IntegratedRenderer.sharedMesh, go, res.IntegratedRenderer.gameObject.name);
                Undo.RegisterCreatedObjectUndo(res.IntegratedRenderer.gameObject, "Integrate Renderers");
            }
            
            // deactivate source renderers
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
            
            PrefabUtility.ReplacePrefab(go, PrefabUtility.GetCorrespondingObjectFromSource(go), ReplacePrefabOptions.ConnectToPrefab);
            
            return results;
        }

        private static void SaveMeshAsset(Mesh mesh, GameObject go, string name)
        {
#if UNITY_2018_2_OR_NEWER
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
            var prefab = PrefabUtility.GetPrefabParent(go);
#endif

            var assetPath = "";
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}_{2}{3}",
                    Path.GetDirectoryName(prefabPath),
                    Path.GetFileNameWithoutExtension(prefabPath),
                    name,
                    ASSET_SUFFIX
                    );
            }
            else
            {
                assetPath = string.Format("Assets/{0}{1}", name, ASSET_SUFFIX);
            }

            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(mesh, assetPath);
        }

    }
}
