using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(Vrm10Instance))]
    public class Vrm10InstanceEditor : Editor
    {
        const string SaveTitle = "New folder for vrm-1.0 assets...";
        static string[] SaveExtensions = new string[] { "asset" };

        static VRM10Object CreateAsset(string path, Dictionary<ExpressionPreset, VRM10Expression> expressions)
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
            foreach (var kv in expressions)
            {
                switch (kv.Key)
                {
                    case ExpressionPreset.aa: asset.Expression.Aa = kv.Value; break;
                    case ExpressionPreset.ih: asset.Expression.Ih = kv.Value; break;
                    case ExpressionPreset.ou: asset.Expression.Ou = kv.Value; break;
                    case ExpressionPreset.ee: asset.Expression.Ee = kv.Value; break;
                    case ExpressionPreset.oh: asset.Expression.Oh = kv.Value; break;
                    case ExpressionPreset.happy: asset.Expression.Happy = kv.Value; break;
                    case ExpressionPreset.angry: asset.Expression.Angry = kv.Value; break;
                    case ExpressionPreset.sad: asset.Expression.Sad = kv.Value; break;
                    case ExpressionPreset.relaxed: asset.Expression.Relaxed = kv.Value; break;
                    case ExpressionPreset.surprised: asset.Expression.Surprised = kv.Value; break;
                    case ExpressionPreset.blink: asset.Expression.Blink = kv.Value; break;
                    case ExpressionPreset.blinkLeft: asset.Expression.BlinkLeft = kv.Value; break;
                    case ExpressionPreset.blinkRight: asset.Expression.BlinkRight = kv.Value; break;
                    case ExpressionPreset.lookUp: asset.Expression.LookUp = kv.Value; break;
                    case ExpressionPreset.lookDown: asset.Expression.LookDown = kv.Value; break;
                    case ExpressionPreset.lookLeft: asset.Expression.LookLeft = kv.Value; break;
                    case ExpressionPreset.lookRight: asset.Expression.LookRight = kv.Value; break;
                    case ExpressionPreset.neutral: asset.Expression.Neutral = kv.Value; break;
                }
            }

            unityPath.CreateAsset(asset);
            AssetDatabase.Refresh();
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

        static VRM10Expression CreateAndSaveExpression(ExpressionPreset preset, string dir)
        {
            var clip = ScriptableObject.CreateInstance<VRM10Expression>();
            clip.name = preset.ToString();
            var path = System.IO.Path.Combine(dir, $"{preset}.asset");
            var unityPath = UnityPath.FromFullpath(path);
            unityPath.CreateAsset(clip);
            var loaded = unityPath.LoadAsset<VRM10Expression>();
            return loaded;
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
                var saveName = (instance.name ?? "vrm-1.0");
                var dir = SaveFileDialog.GetDir(SaveTitle, saveName);
                if (!string.IsNullOrEmpty(dir))
                {
                    var expressions = new Dictionary<ExpressionPreset, VRM10Expression>();
                    foreach (ExpressionPreset expression in CachedEnum.GetValues<ExpressionPreset>())
                    {
                        if (expression == ExpressionPreset.custom)
                        {
                            continue;
                        }
                        expressions[expression] = CreateAndSaveExpression(expression, dir);
                    }

                    var path = System.IO.Path.Combine(dir, (instance.name ?? "VRMObject") + ".asset");
                    var asset = CreateAsset(path, expressions);
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
