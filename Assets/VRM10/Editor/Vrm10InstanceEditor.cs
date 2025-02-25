using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace UniVRM10
{
    [CustomEditor(typeof(Vrm10Instance))]
    public class Vrm10InstanceEditor : Editor
    {
        const string SaveTitle = "New folder for vrm-1.0 assets...";
        const string SpringsPath = "SpringBone.Springs";
        const string CollidersPath = "SpringBone.ColliderGroups";

        enum Tab
        {
            VrmInstance,
            LookAt,
            SpringBone,
        }
        static Tab s_selected = default;
        static bool s_foldRuntimeLookAt = false;

        Vrm10Instance m_instance;
        private Dictionary<string, Material> m_materials = new();

        SerializedProperty m_script;
        SerializedProperty m_vrmObject;
        SerializedProperty m_updateType;

        SerializedProperty m_drawLookatGizmo;
        SerializedProperty m_lookatTarget;
        SerializedProperty m_lookatTargetType;

        ListView m_springs;

        void OnEnable()
        {
            m_instance = (Vrm10Instance)target;
            m_materials.Clear();
            foreach (var r in m_instance.GetComponentsInChildren<Renderer>())
            {
                foreach (var m in r.sharedMaterials)
                {
                    m_materials.TryAdd(m.name, m);
                }
            }
            m_script = serializedObject.FindProperty("m_Script");

            m_vrmObject = serializedObject.FindProperty(nameof(m_instance.Vrm));
            m_updateType = serializedObject.FindProperty(nameof(m_instance.UpdateType));

            m_drawLookatGizmo = serializedObject.FindProperty(nameof(m_instance.DrawLookAtGizmo));
            m_lookatTarget = serializedObject.FindProperty(nameof(m_instance.LookAtTarget));
            m_lookatTargetType = serializedObject.FindProperty(nameof(m_instance.LookAtTargetType));
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
                if (UniGltfEditorDialog.TryGetDir(SaveTitle, System.IO.Path.GetDirectoryName(saveName), out var dir))
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

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Bind(serializedObject);
            {
                var s = new PropertyField { bindingPath = "m_Script" };
                s.SetEnabled(false);
                root.Add(s);
            }

            var tabs = new EnumField("select UI", s_selected);
            root.Add(tabs);

            var body = new VisualElement();
            List<(Tab, VisualElement)> contents = new()
            {
                (Tab.VrmInstance, new IMGUIContainer(GUIVrmInstance)),
                (Tab.LookAt, new IMGUIContainer(GUILookAt)),
                (Tab.SpringBone, GUISpringBone()),
            };
            foreach (var (tab, content) in contents)
            {
                // content.visible = tab == Tab.VrmInstance;
                content.style.display = tab == s_selected
                    ? DisplayStyle.Flex
                    : DisplayStyle.None
                    ;
                body.Add(content);
            }
            root.Add(body);

            tabs.RegisterValueChangedCallback(e =>
            {
                s_selected = (Tab)e.newValue;
                foreach (var (tab, content) in contents)
                {
                    // content.visible = tab == selected;
                    content.style.display = tab == s_selected
                        ? DisplayStyle.Flex
                        : DisplayStyle.None
                        ;
                }
            });

            return root;
        }

        void GUIVrmInstance()
        {
            if (m_instance.Vrm == null)
            {
                SetupVRM10Object(m_instance);
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_vrmObject);
            EditorGUILayout.PropertyField(m_updateType);
            serializedObject.ApplyModifiedProperties();
        }

        void showuv(ExpressionPreset preset)
        {
            EditorGUI.indentLevel++;
            try
            {
                var (_, clip) = m_instance.Vrm.Expression.Clips.FirstOrDefault(x => x.Preset == preset);
                if (clip != null)
                {
                    foreach (var b in clip.MaterialUVBindings)
                    {
                        if (m_materials.TryGetValue(b.MaterialName, out var m))
                        {
                            EditorGUILayout.TextField(b.MaterialName, $"{b.Offset},${b.Scaling} => {m.mainTextureOffset},{m.mainTextureScale}");
                        }
                        else
                        {
                            EditorGUILayout.TextField(b.MaterialName, "not found");
                        }
                    }
                }
            }
            finally
            {
                EditorGUI.indentLevel--;
            }
        }

        void GUILookAt()
        {
            if (!target)
            {
                return;
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_drawLookatGizmo);
            EditorGUILayout.PropertyField(m_lookatTarget);
            EditorGUILayout.PropertyField(m_lookatTargetType);
            serializedObject.ApplyModifiedProperties();

            // lookat info
            {
                EditorGUILayout.Space();
                s_foldRuntimeLookAt = EditorGUILayout.Foldout(s_foldRuntimeLookAt, "RuntimeInfo");
                if (s_foldRuntimeLookAt)
                {
                    var enabled = GUI.enabled;
                    GUI.enabled = false;
                    EditorGUILayout.Slider("yaw(-180 ~ +180)", m_instance.Runtime.LookAt.Yaw, -180, 180);
                    EditorGUILayout.Slider("pitch(-90 ~ +90)", m_instance.Runtime.LookAt.Pitch, -90, 90);
                    if (m_instance.Runtime.LookAt.EyeDirectionApplicable is LookAtEyeDirectionApplicableToBone)
                    {
                        EditorGUILayout.LabelField("BoneTYpe");
                    }
                    else if (m_instance.Runtime.LookAt.EyeDirectionApplicable is LookAtEyeDirectionApplicableToExpression)
                    {
                        EditorGUILayout.LabelField("ExpressionType");
                        var w = m_instance.Runtime.Expression.ActualWeights;
                        // left
                        if (w.TryGetValue(ExpressionKey.LookLeft, out var left))
                        {
                            EditorGUILayout.Slider("left", left, 0, 1);
                            showuv(ExpressionPreset.lookLeft);
                        }
                        else
                        {
                            EditorGUILayout.TextField("left");
                        }
                        // right
                        if (w.TryGetValue(ExpressionKey.LookRight, out var right))
                        {
                            EditorGUILayout.Slider("right", right, 0, 1);
                            showuv(ExpressionPreset.lookRight);
                        }
                        else
                        {
                            EditorGUILayout.TextField("right");
                        }
                        // up
                        if (w.TryGetValue(ExpressionKey.LookUp, out var up))
                        {
                            EditorGUILayout.Slider("up", up, 0, 1);
                            showuv(ExpressionPreset.lookUp);
                        }
                        else
                        {
                            EditorGUILayout.TextField("up");
                        }
                        // down
                        if (w.TryGetValue(ExpressionKey.LookDown, out var down))
                        {
                            EditorGUILayout.Slider("down", down, 0, 1);
                            showuv(ExpressionPreset.lookDown);
                        }
                        else
                        {
                            EditorGUILayout.TextField("down");
                        }


                    }
                    else
                    {
                        EditorGUILayout.LabelField($"UnknownTYpe: {m_instance.Runtime.LookAt.EyeDirectionApplicable}");
                    }
                    GUI.enabled = enabled;
                    Repaint();
                }
            }
        }

        VisualElement GUISpringBone()
        {
            var root = new VisualElement();

            root.Add(new PropertyField { bindingPath = CollidersPath });

            m_springs = new ListView
            {
                bindingPath = SpringsPath,
                makeItem = () =>
                {
                    return new Label();
                },
                bindItem = (v, i) =>
                {
                    var prop = serializedObject.FindProperty($"{SpringsPath}.Array.data[{i}].Name");
                    (v as Label).BindProperty(prop);
                },
            };
            m_springs.headerTitle = "Springs";
            m_springs.showFoldoutHeader = true;
            m_springs.showAddRemoveFooter = true;
            root.Add(m_springs);

            var selected = new PropertyField();
#if UNITY_2022_3_OR_NEWER
            m_springs.selectedIndicesChanged += (e) =>
#else
            m_springs.onSelectedIndicesChange += (e) =>
#endif
            {
                var values = e.ToArray();
                if (values.Length > 0)
                {
                    var path = $"{SpringsPath}.Array.data[{values[0]}]";
                    var prop = serializedObject.FindProperty(path);
                    selected.BindProperty(prop);
                }
            };
            root.Add(selected);

            return root;
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
            HandleUtility.Repaint();

            // 親指のガイド          
            DrawThumbGuide(target as Vrm10Instance);

            // 選択中の SpringBone
            if (m_springs != null && m_springs.selectedIndex >= 0 && m_springs.selectedIndex < m_instance.SpringBone.Springs.Count)
            {
                Handles.color = Color.red;
                var selected = m_instance.SpringBone.Springs[m_springs.selectedIndex];
                for (int i = 1; i < selected.Joints.Count; ++i)
                {
                    var head = selected.Joints[i - 1];
                    var tail = selected.Joints[i];
                    if (head != null && tail != null)
                    {
                        Handles.DrawLine(head.transform.position, tail.transform.position);
                    }
                }
            }
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
