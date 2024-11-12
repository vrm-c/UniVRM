using System;
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

        enum Tab
        {
            VrmInstance,
            LookAt,
            SpringBone,
        }

        Tab m_tab;

        Vrm10Instance m_instance;

        SerializedProperty m_vrmObject;
        SerializedProperty m_updateType;

        SerializedProperty m_drawLookatGizmo;
        SerializedProperty m_lookatTarget;
        SerializedProperty m_lookatTargetType;

        SerializedProperty m_colliderGroups;
        SerializedProperty m_springs;
        UnityEditorInternal.ReorderableList m_springList;
        // int m_springListIndex = 0;
        SerializedProperty m_springSelected;
        SerializedProperty m_springSelectedJoints;

        void OnEnable()
        {
            m_instance = (Vrm10Instance)target;

            m_vrmObject = serializedObject.FindProperty(nameof(m_instance.Vrm));
            m_updateType = serializedObject.FindProperty(nameof(m_instance.UpdateType));

            m_drawLookatGizmo = serializedObject.FindProperty(nameof(m_instance.DrawLookAtGizmo));
            m_lookatTarget = serializedObject.FindProperty(nameof(m_instance.LookAtTarget));
            m_lookatTargetType = serializedObject.FindProperty(nameof(m_instance.LookAtTargetType));

            m_colliderGroups = serializedObject.FindProperty($"{nameof(m_instance.SpringBone)}.{nameof(m_instance.SpringBone.ColliderGroups)}");
            m_springs = serializedObject.FindProperty($"{nameof(m_instance.SpringBone)}.{nameof(m_instance.SpringBone.Springs)}");
            m_springList = new(serializedObject, m_springs);
            m_springList.drawHeaderCallback += rect =>
                 {
                     EditorGUI.LabelField(rect, m_springs.displayName);
                 };
            Action<UnityEditorInternal.ReorderableList> updateSelected = (list) =>
            {
                var index = list.index;
                if (index < 0)
                {
                    index = 0;
                }
                else if (index >= list.count)
                {
                    index = list.count - 1;
                }
                if (list.index >= 0 && list.index < list.count)
                {
                    m_springSelected = m_springs.GetArrayElementAtIndex(list.index);
                    m_springSelected.FindPropertyRelative("ColliderGroups").isExpanded = true;
                    m_springSelectedJoints = m_springSelected.FindPropertyRelative("Joints");
                    m_springSelectedJoints.isExpanded = true;
                }
                else
                {
                    m_springSelected = null;
                }
            };
            m_springList.onSelectCallback = new(updateSelected);
            m_springList.onChangedCallback = new(updateSelected);
        }

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
                    default: break;
                }
            }

            unityPath.CreateAsset(asset);
            AssetDatabase.Refresh();
            var loaded = unityPath.LoadAsset<VRM10Object>();

            return loaded;
        }

        static bool CheckHumanoid(GameObject go)
        {
            if (go.TryGetComponent<Animator>(out var animator))
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

            var humanoid = go.GetComponentOrNull<UniHumanoid.Humanoid>();
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

            if (PathObject.TryGetFromAsset(instance, out var asset))
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


        static readonly string[] Tabs = ((Tab[])Enum.GetValues(typeof(Tab))).Select(x => x.ToString()).ToArray();

        public override void OnInspectorGUI()
        {
            if (m_instance.Vrm == null)
            {
                SetupVRM10Object(m_instance);
            }

            var backup = GUI.enabled;
            try
            {
                GUI.enabled = true;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    m_tab = (Tab)GUILayout.Toolbar((int)m_tab, Tabs, new GUIStyle(EditorStyles.toolbarButton), GUI.ToolbarButtonSize.FitToContents);
                }
            }
            finally
            {
                GUI.enabled = backup;
            }

            switch (m_tab)
            {
                case Tab.VrmInstance:
                    GUIVrmInstance();
                    break;

                case Tab.LookAt:
                    GUILookAt();
                    break;

                case Tab.SpringBone:
                    GUISpringBone();
                    break;

                default:
                    throw new Exception();
            }
            // base.OnInspectorGUI();
        }

        void GUIVrmInstance()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_vrmObject);
            EditorGUILayout.PropertyField(m_updateType);
            serializedObject.ApplyModifiedProperties();
        }

        void GUILookAt()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_drawLookatGizmo);
            EditorGUILayout.PropertyField(m_lookatTarget);
            EditorGUILayout.PropertyField(m_lookatTargetType);
            serializedObject.ApplyModifiedProperties();
        }

        void GUISpringBone()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_colliderGroups);
            // EditorGUILayout.PropertyField(m_springs);
            m_springList.DoLayoutList();

            if (m_springSelected != null)
            {
                EditorGUILayout.PropertyField(m_springSelected);

                if (m_springSelectedJoints.arraySize > 0 && GUILayout.Button("create joints to children"))
                {
                    if (EditorUtility.DisplayDialog("auto joints",
                        "先頭の joint の子孫をリストに追加します。\n既存のリストは上書きされます。",
                        "ok",
                        "cancel"))
                    {
                        var joints = m_springSelectedJoints;
                        var root = (VRM10SpringBoneJoint)joints.GetArrayElementAtIndex(0).objectReferenceValue;
                        joints.ClearArray();
                        int i = 0;
                        // 0
                        joints.InsertArrayElementAtIndex(i);
                        joints.GetArrayElementAtIndex(i).objectReferenceValue = root;
                        ++i;
                        // 1...
                        foreach (var joint in MakeJointsRecursive(root))
                        {
                            joints.InsertArrayElementAtIndex(i);
                            joints.GetArrayElementAtIndex(i).objectReferenceValue = joint;
                            ++i;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        static IEnumerable<VRM10SpringBoneJoint> MakeJointsRecursive(VRM10SpringBoneJoint parent)
        {
            if (parent.transform.childCount > 0)
            {
                var child = parent.transform.GetChild(0);
                var joint = child.gameObject.GetOrAddComponent<VRM10SpringBoneJoint>();
                // set params
                joint.m_dragForce = parent.m_dragForce;
                joint.m_gravityDir = parent.m_gravityDir;
                joint.m_gravityPower = parent.m_gravityPower;
                joint.m_jointRadius = parent.m_jointRadius;
                joint.m_stiffnessForce = parent.m_stiffnessForce;

                yield return joint;
                foreach (var x in MakeJointsRecursive(joint))
                {
                    yield return x;
                }
            }
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
