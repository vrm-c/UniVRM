using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabBoneMeshRemover
    {
        const string BONE_MESH_ERASER_NAME = "BoneMeshEraser";
        const string ASSET_SUFFIX = ".mesh.asset";

        public static bool TryExecutable(GameObject root, SkinnedMeshRenderer smr, out string msg)
        {
            if (root == null)
            {
                msg = MeshUtilityMessages.NO_GAMEOBJECT_SELECTED.Msg();
                return false;
            }

            if (smr == null)
            {
                msg = MeshUtilityMessages.SELECT_SKINNED_MESH.Msg();
                return false;
            }

            msg = "";
            return true;
        }

        public static bool OnGUI(GameObject root, SkinnedMeshRenderer smr, List<BoneMeshEraser.EraseBone> eraseBones)
        {
            var _isInvokeSuccess = false;
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                {
                    _isInvokeSuccess = TabBoneMeshRemover.Execute(root, smr, eraseBones);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        private static bool Execute(GameObject root, SkinnedMeshRenderer smr, List<BoneMeshEraser.EraseBone> eraseBones)
        {
            var bones = smr.bones;

            var meshNode = new GameObject(BONE_MESH_ERASER_NAME);
            meshNode.transform.SetParent(root.transform, false);

            var erased = meshNode.AddComponent<SkinnedMeshRenderer>();
            erased.sharedMesh = BoneMeshEraser.CreateErasedMesh(smr.sharedMesh, eraseBones
                .Where(x => x.Erase)
                .Select(x => Array.IndexOf(bones, x.Bone))
                .ToArray());
            erased.sharedMaterials = smr.sharedMaterials;
            erased.bones = smr.bones;


            // save mesh to Assets
            var assetPath = string.Format("{0}{1}", root.name, ASSET_SUFFIX);
            var prefab = GetPrefab(root);
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}{2}",
                    Path.GetDirectoryName(prefabPath),
                    Path.GetFileNameWithoutExtension(prefabPath),
                    ASSET_SUFFIX
                    );
            }
            UniGLTFLogger.Log($"CreateAsset: {assetPath}");
            AssetDatabase.CreateAsset(erased.sharedMesh, assetPath);

            // destroy BoneMeshEraser in the source
            foreach (var skinnedMesh in root.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.gameObject.name == BONE_MESH_ERASER_NAME)
                {
                    GameObject.DestroyImmediate(skinnedMesh.gameObject);
                }
            }

            // destroy the original mesh in the copied GameObject
            var outputObject = GameObject.Instantiate(root);
            outputObject.name = outputObject.name + "_bone_mesh_erase";
            foreach (var skinnedMesh in outputObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.sharedMesh == smr.sharedMesh)
                {
                    GameObject.DestroyImmediate(skinnedMesh);
                }
            }

            return true;
        }

        public static UnityEngine.Object GetPrefab(GameObject instance)
        {
#if UNITY_2018_2_OR_NEWER
            return PrefabUtility.GetCorrespondingObjectFromSource(instance);
#else
            return PrefabUtility.GetPrefabParent(go);
#endif
        }
    }
}