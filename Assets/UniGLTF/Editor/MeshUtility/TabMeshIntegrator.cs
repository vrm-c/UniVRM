using System.Collections.Generic;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabMeshIntegrator
    {
        public static bool TryExecutable(GameObject root, out string msg)
        {
            // check
            if (root == null)
            {
                msg = MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg();
                return false;
            }

            if (HasVrm(root))
            {
                msg = MeshProcessingMessages.VRM_DETECTED.Msg();
                return false;
            }

            if (root.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0 && root.GetComponentsInChildren<MeshFilter>().Length == 0)
            {
                msg = MeshProcessingMessages.NO_MESH.Msg();
                return false;
            }

            msg = "";
            return true;
        }

        const string VRM_META = "VRMMeta";
        static bool HasVrm(GameObject root)
        {
            var allComponents = root.GetComponents(typeof(Component));
            foreach (var component in allComponents)
            {
                if (component == null) continue;
                var sourceString = component.ToString();
                if (sourceString.Contains(VRM_META))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool OnGUI(GameObject root, bool onlyBlendShapeRenderers)
        {
            var _isInvokeSuccess = false;
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                {
                    _isInvokeSuccess = TabMeshIntegrator.Execute(root, onlyBlendShapeRenderers);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        static bool Execute(GameObject root, bool onlyBlendShapeRenderers)
        {
            var results = new List<MeshIntegrationResult>();
            if (onlyBlendShapeRenderers)
            {
                results.Add(MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: MeshEnumerateOption.OnlyWithBlendShape));
                results.Add(MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: MeshEnumerateOption.OnlyWithoutBlendShape));
            }
            else
            {
                results.Add(MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: MeshEnumerateOption.All));
            }

            // 統合結果を適用した新しいヒエラルキーをコピーから作成する
            MeshIntegratorUtility.CopyAndReplaceWithResults(root, results);
            return true;
        }
    }
}
