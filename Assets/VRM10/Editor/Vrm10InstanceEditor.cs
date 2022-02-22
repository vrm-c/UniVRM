using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(Vrm10Instance))]
    public class Vrm10InstanceEditor : Editor
    {
        const string SaveTitle = "Save VRM10Object to...";
        static string[] SaveExtensions = new string[] { "asset" };

        static VRM10Object CreateAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            var unityPath = UnityPath.FromFullpath(path);
            if (!unityPath.IsUnderAssetsFolder)
            {
                EditorUtility.DisplayDialog("error", "The specified path is not inside of Assets/", "OK");
                return null;
            }

            var asset = ScriptableObject.CreateInstance<VRM10Object>();
            unityPath.CreateAsset(asset);

            var loaded = unityPath.LoadAsset<VRM10Object>();

            return loaded;
        }

        static bool CheckHumanoid(GameObject go)
        {
            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                if (animator.avatar == null)
                {
                    EditorGUILayout.HelpBox("animator.avatar is null", MessageType.Error);
                    return false;
                }
                if (!animator.avatar.isValid)
                {
                    EditorGUILayout.HelpBox("animator.avatar is not valid", MessageType.Error);
                    return false;

                }
                if (!animator.avatar.isHuman)
                {
                    EditorGUILayout.HelpBox("animator.avatar is not human", MessageType.Error);
                    return false;
                }

                return true;
            }

            var humanoid = go.GetComponent<UniHumanoid.Humanoid>();
            if (humanoid == null)
            {
                EditorGUILayout.HelpBox("vrm-1.0 require Animator or UniHumanoid.Humanoid", MessageType.Error);
                return false;
            }

            if (humanoid != null)
            {
                if (humanoid.Validate().Any())
                {
                    // 不正
                    EditorGUILayout.HelpBox("Please create humanoid avatar", MessageType.Error);
                    return false;
                }
            }

            return true;
        }

        void Setup(Vrm10Instance instance)
        {
            if (instance.Vrm != null)
            {
                // OK
                return;
            }

            if (!CheckHumanoid(instance.gameObject))
            {
                // can not
                return;
            }

            EditorGUILayout.HelpBox("Humanoid OK.", MessageType.Info);
            if (GUILayout.Button("Create new VRM10Object"))
            {
                var saveName = (instance.name ?? "VRMObject") + ".asset";
                var path = SaveFileDialog.GetPath(SaveTitle, saveName, SaveExtensions);
                var asset = CreateAsset(path);
                if (asset != null)
                {
                    // update editor
                    serializedObject.Update();
                    var prop = serializedObject.FindProperty(nameof(Vrm10Instance.Vrm));
                    prop.objectReferenceValue = asset;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (target is Vrm10Instance instance)
            {
                Setup(instance);
            }

            base.OnInspectorGUI();
        }
    }
}
