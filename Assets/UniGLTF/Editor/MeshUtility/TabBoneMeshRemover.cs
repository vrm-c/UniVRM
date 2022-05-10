using System;
using System.IO;
using System.Linq;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabBoneMeshRemover
    {
        public static bool OnGUI(GameObject _exportTarget, SkinnedMeshRenderer _cSkinnedMesh, Transform _cEraseRoot, BoneMeshEraser.EraseBone[] _eraseBones)
        {
            var _isInvokeSuccess = false;
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                {
                    _isInvokeSuccess = TabBoneMeshRemover.Execute(_exportTarget, _cSkinnedMesh, _cEraseRoot, _eraseBones);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        public static bool Execute(GameObject _exportTarget, SkinnedMeshRenderer _cSkinnedMesh, Transform _cEraseRoot, BoneMeshEraser.EraseBone[] _eraseBones)
        {
            if (_exportTarget == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg(), "ok");
                return false;
            }
            var go = _exportTarget;

            if (_cSkinnedMesh == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.SELECT_SKINNED_MESH.Msg(), "ok");
                return false;
            }
            else if (_cEraseRoot == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.SELECT_ERASE_ROOT.Msg(), "ok");
                return false;
            }
            BoneMeshRemove(go, _cSkinnedMesh, _eraseBones);

            return true;
        }

        private static void BoneMeshRemove(GameObject go, SkinnedMeshRenderer _cSkinnedMesh, BoneMeshEraser.EraseBone[] _eraseBones)
        {
            var renderer = Remove(go, _cSkinnedMesh, _eraseBones);
            var outputObject = GameObject.Instantiate(go);
            outputObject.name = outputObject.name + "_bone_mesh_erase";
            if (renderer == null)
            {
                return;
            }

            // save mesh to Assets
            var assetPath = string.Format("{0}{1}", go.name, MeshUtility.ASSET_SUFFIX);
            var prefab = MeshUtility.GetPrefab(go);
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}{2}",
                    Path.GetDirectoryName(prefabPath),
                    Path.GetFileNameWithoutExtension(prefabPath),
                    MeshUtility.ASSET_SUFFIX
                    );
            }

            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(renderer.sharedMesh, assetPath);

            // destroy BoneMeshEraser in the source
            foreach (var skinnedMesh in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.gameObject.name == BoneMeshEraserWizard.BONE_MESH_ERASER_NAME)
                {
                    GameObject.DestroyImmediate(skinnedMesh.gameObject);
                }
            }
            // destroy the original mesh in the copied GameObject
            foreach (var skinnedMesh in outputObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.sharedMesh == _cSkinnedMesh.sharedMesh)
                {
                    GameObject.DestroyImmediate(skinnedMesh);
                }
            }
        }

        private static SkinnedMeshRenderer Remove(GameObject go, SkinnedMeshRenderer _cSkinnedMesh, BoneMeshEraser.EraseBone[] _eraseBones)
        {
            var bones = _cSkinnedMesh.bones;
            var eraseBones = _eraseBones
                .Where(x => x.Erase)
                .Select(x => Array.IndexOf(bones, x.Bone))
                .ToArray();

            var meshNode = new GameObject(BoneMeshEraserWizard.BONE_MESH_ERASER_NAME);
            meshNode.transform.SetParent(go.transform, false);

            var erased = meshNode.AddComponent<SkinnedMeshRenderer>();
            erased.sharedMesh = BoneMeshEraser.CreateErasedMesh(_cSkinnedMesh.sharedMesh, eraseBones);
            erased.sharedMaterials = _cSkinnedMesh.sharedMaterials;
            erased.bones = _cSkinnedMesh.bones;

            return erased;
        }
    }
}