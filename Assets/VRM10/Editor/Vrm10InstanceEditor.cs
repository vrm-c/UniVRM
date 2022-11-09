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

        static VRM10Object CreateAsset(string path, Dictionary<ExpressionPreset, VRM10Expression> expressions, Vrm10Instance instance)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            var unityPath = UnityPath.FromFullpath(path);
            if (!unityPath.IsUnderWritableFolder)
            {
                EditorUtility.DisplayDialog("error", "The specified path is not inside of Assets or writable Packages", "OK");
                return null;
            }

            var asset = ScriptableObject.CreateInstance<VRM10Object>();

            asset.Prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance?.gameObject);
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

        static VRM10Expression CreateAndSaveExpression(ExpressionPreset preset, string dir, Vrm10Instance instance)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance.gameObject);
            var clip = ScriptableObject.CreateInstance<VRM10Expression>();
            clip.name = preset.ToString();
            clip.Prefab = prefab;
            var path = System.IO.Path.Combine(dir, $"{preset}.asset");
            var unityPath = UnityPath.FromFullpath(path);
            unityPath.CreateAsset(clip);
            var loaded = unityPath.LoadAsset<VRM10Expression>();
            return loaded;
        }

        static string GetSaveName(Vrm10Instance instance)
        {
            if (instance == null)
            {
                return "Assets/vrm-1.0.assets";
            }

            if (VRMShaders.PathObject.TryGetFromAsset(instance, out var asset))
            {
                return (asset.Parent.Child(instance.name + ".asset")).UnityAssetPath;
            }

            return $"Assets/{instance.name}.assets";
        }

        void SetupVRM10Object(Vrm10Instance instance)
        {
            if (!CheckHumanoid(instance.gameObject))
            {
                // can not
                return;
            }

            EditorGUILayout.HelpBox("Humanoid OK.", MessageType.Info);

            // VRM10Object
            var prop = serializedObject.FindProperty(nameof(Vrm10Instance.Vrm));
            if (prop.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No VRM10Object.", MessageType.Error);
            }
            if (GUILayout.Button("Create new VRM10Object and default Expressions. select target folder"))
            {
                var saveName = GetSaveName(instance);
                var dir = SaveFileDialog.GetDir(SaveTitle, System.IO.Path.GetDirectoryName(saveName));
                if (!string.IsNullOrEmpty(dir))
                {
                    var expressions = new Dictionary<ExpressionPreset, VRM10Expression>();
                    foreach (ExpressionPreset expression in CachedEnum.GetValues<ExpressionPreset>())
                    {
                        if (expression == ExpressionPreset.custom)
                        {
                            continue;
                        }
                        expressions[expression] = CreateAndSaveExpression(expression, dir, instance);
                    }

                    var path = System.IO.Path.Combine(dir, (instance.name ?? "VRMObject") + ".asset");
                    var asset = CreateAsset(path, expressions, instance);
                    if (asset != null)
                    {
                        // update editor
                        serializedObject.Update();
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
                if (instance.Vrm == null)
                {
                    SetupVRM10Object(instance);
                }

                if (instance.Vrm != null)
                {
                    EditorGUILayout.HelpBox("SpringBone gizmo etc...", MessageType.Info);
                    if (GUILayout.Button("Open " + VRM10Window.WINDOW_TITLE))
                    {
                        VRM10Window.Open();
                    }
                }
            }

            base.OnInspectorGUI();
        }

        public void OnSceneGUI()
        {
            // 親指のガイド          
            DrawThumbGuide(target as Vrm10Instance);
        }

        static void DrawThumbGuide(Vrm10Instance instance)
        {
            if (instance == null)
            {
                return;
            }
            if (instance.TryGetBoneTransform(HumanBodyBones.LeftThumbProximal, out var l0))
            {
                if (instance.TryGetBoneTransform(HumanBodyBones.LeftThumbIntermediate, out var l1))
                {
                    if (instance.TryGetBoneTransform(HumanBodyBones.LeftThumbDistal, out var l2))
                    {
                        var color = new Color(0.5f, 1.0f, 0.0f, 1.0f);
                        var thumbDir = (Vector3.forward + Vector3.left).normalized;
                        var nailNormal = (Vector3.forward + Vector3.right).normalized;
                        DrawThumbGuide(l0.position, l2.position, thumbDir, nailNormal, color);
                    }
                }
            }
            if (instance.TryGetBoneTransform(HumanBodyBones.RightThumbProximal, out var r0))
            {
                if (instance.TryGetBoneTransform(HumanBodyBones.RightThumbIntermediate, out var r1))
                {
                    if (instance.TryGetBoneTransform(HumanBodyBones.RightThumbDistal, out var r2))
                    {
                        var color = new Color(0.5f, 1.0f, 0.0f, 1.0f);
                        var thumbDir = (Vector3.forward + Vector3.right).normalized;
                        var nailNormal = (Vector3.forward + Vector3.left).normalized;
                        DrawThumbGuide(r0.position, r2.position, thumbDir, nailNormal, color);
                    }
                }
            }
        }
        static void DrawThumbGuide(Vector3 metacarpalPos, Vector3 distalPos, Vector3 thumbDir, Vector3 nailNormal, Color color)
        {
            Handles.color = color;
            Handles.matrix = Matrix4x4.identity;

            var thumbVector = distalPos - metacarpalPos;
            var thumbLength = thumbVector.magnitude * 1.5f;
            var thickness = thumbLength * 0.1f;
            var tipCenter = metacarpalPos + thumbDir * (thumbLength - thickness);
            var crossVector = Vector3.Cross(thumbDir, nailNormal);

            // 指の形を描く
            Handles.DrawLine(metacarpalPos + crossVector * thickness, tipCenter + crossVector * thickness);
            Handles.DrawLine(metacarpalPos - crossVector * thickness, tipCenter - crossVector * thickness);
            Handles.DrawWireArc(tipCenter, nailNormal, crossVector, 180f, thickness);

            Handles.DrawLine(metacarpalPos + nailNormal * thickness, tipCenter + nailNormal * thickness);
            Handles.DrawLine(metacarpalPos - nailNormal * thickness, tipCenter - nailNormal * thickness);
            Handles.DrawWireArc(tipCenter, crossVector, -nailNormal, 180f, thickness);

            Handles.DrawWireDisc(metacarpalPos, thumbDir, thickness);
            Handles.DrawWireDisc(tipCenter, thumbDir, thickness);

            // 爪の方向に伸びる線を描く
            Handles.DrawDottedLine(tipCenter, tipCenter + nailNormal * thickness * 8.0f, 1.0f);

            // 爪を描く
            Vector2[] points2 = {
                new Vector2(-0.2f, -0.5f),
                new Vector2(0.2f, -0.5f),
                new Vector2(0.5f, -0.3f),
                new Vector2(0.5f, 0.3f),
                new Vector2(0.2f, 0.5f),
                new Vector2(-0.2f, 0.5f),
                new Vector2(-0.5f, 0.3f),
                new Vector2(-0.5f, -0.3f),
                new Vector2(-0.2f, -0.5f),
            };
            Vector3[] points = points2
                .Select(v => tipCenter + (nailNormal + crossVector * v.x + thumbDir * v.y) * thickness)
                .ToArray();

            Handles.DrawAAPolyLine(points);
            Handles.color = color * new Color(1.0f, 1.0f, 1.0f, 0.1f);
            Handles.DrawAAConvexPolygon(points);

            // 文字ラベルを描く
            Handles.Label(tipCenter + nailNormal * thickness * 6.0f, "Thumb nail direction");
        }
    }
}
