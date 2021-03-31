using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using VrmLib;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10Controller))]
    public class VRM10ControllerEditor : Editor
    {
        VRM10Controller m_target;
        SkinnedMeshRenderer[] m_renderers;
        Dictionary<ExpressionKey, float> m_expressionKeyWeights = new Dictionary<ExpressionKey, float>();

        public class ExpressionSlider
        {
            Dictionary<ExpressionKey, float> m_expressionKeys;
            ExpressionKey m_key;

            public ExpressionSlider(Dictionary<ExpressionKey, float> expressionKeys, ExpressionKey key)
            {
                m_expressionKeys = expressionKeys;
                m_key = key;
            }

            public KeyValuePair<ExpressionKey, float> Slider()
            {
                var oldValue = m_expressionKeys[m_key];
                var enable = GUI.enabled;
                GUI.enabled = Application.isPlaying;
                var newValue = EditorGUILayout.Slider(m_key.ToString(), oldValue, 0, 1.0f);
                GUI.enabled = enable;
                return new KeyValuePair<ExpressionKey, float>(m_key, newValue);
            }
        }
        List<ExpressionSlider> m_sliders;

        void OnEnable()
        {
            m_target = (VRM10Controller)target;
            if (m_target.Expression.ExpressionAvatar != null && m_target.Expression.ExpressionAvatar.Clips != null)
            {
                m_expressionKeyWeights = m_target.Expression.ExpressionAvatar.Clips.ToDictionary(x => ExpressionKey.CreateFromClip(x), x => 0.0f);
                m_sliders = m_target.Expression.ExpressionAvatar.Clips
                    .Where(x => x != null)
                    .Select(x => new ExpressionSlider(m_expressionKeyWeights, ExpressionKey.CreateFromClip(x)))
                    .ToList()
                    ;
            }

            if (m_target.Meta)
            {
                m_metaEditor = Editor.CreateEditor(m_target.Meta);
            }
            m_controller = PropGui.FromObject(serializedObject, nameof(m_target.Controller));
            m_expression = PropGui.FromObject(serializedObject, nameof(m_target.Expression));
            m_lookAt = PropGui.FromObject(serializedObject, nameof(m_target.LookAt));
            m_firstPerson = PropGui.FromObject(serializedObject, nameof(m_target.FirstPerson));
            m_springBone = PropGui.FromObject(serializedObject, nameof(m_target.SpringBone));
        }

        void OnDisable()
        {
            if (m_metaEditor)
            {
                UnityEditor.Editor.DestroyImmediate(m_metaEditor);
                m_metaEditor = null;
            }
        }

        enum Tabs
        {
            Meta,
            Controller,
            Expression,
            LookAt,
            FirstPerson,
            SpringBone,
        }
        Tabs _tab;

        Editor m_metaEditor;

        class PropGui
        {
            SerializedProperty m_prop;

            PropGui(SerializedProperty property)
            {
                m_prop = property;
            }

            public static PropGui FromObject(SerializedObject serializedObject, string name)
            {
                var prop = serializedObject.FindProperty(name);
                return new PropGui(prop);
            }

            public void RecursiveProperty()
            {
                var depth = m_prop.depth;
                var iterator = m_prop.Copy();
                for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                {
                    if (iterator.depth < depth)
                        return;

                    depth = iterator.depth;

                    using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                        EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }

        PropGui m_controller;
        PropGui m_expression;
        PropGui m_lookAt;
        PropGui m_firstPerson;
        PropGui m_springBone;
        PropGui m_asset;

        public override void OnInspectorGUI()
        {
            {
                var backup = GUI.enabled;
                GUI.enabled = true;

                _tab = (Tabs)EditorGUILayout.EnumPopup("Select GUI", _tab);
                EditorGUILayout.Separator();

                GUI.enabled = backup;
            }

            serializedObject.Update();

            // Setup runtime function.
            m_target.Setup();

            // base.OnInspectorGUI();
            switch (_tab)
            {
                case Tabs.Meta:
                    m_metaEditor?.OnInspectorGUI();
                    break;

                case Tabs.Controller:
                    m_controller.RecursiveProperty();
                    break;

                case Tabs.Expression:
                    ExpressionGUI();
                    m_expression.RecursiveProperty();
                    break;

                case Tabs.LookAt:
                    m_lookAt.RecursiveProperty();
                    break;

                case Tabs.FirstPerson:
                    m_firstPerson.RecursiveProperty();
                    break;

                case Tabs.SpringBone:
                    m_springBone.RecursiveProperty();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        void ExpressionGUI()
        {
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enable when playing", MessageType.Info);
            }

            if (m_target.Expression.ExpressionAvatar == null)
            {
                return;
            }

            if (m_sliders != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Expression Weights", EditorStyles.boldLabel);

                var sliders = m_sliders.Select(x => x.Slider());
                foreach (var slider in sliders)
                {
                    m_expressionKeyWeights[slider.Key] = slider.Value;
                }
                m_target.Expression.SetWeights(m_expressionKeyWeights);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Override rates", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUILayout.Slider("Blink override rate", m_target.Expression.BlinkOverrideRate, 0f, 1f);
                EditorGUILayout.Slider("LookAt override rate", m_target.Expression.LookAtOverrideRate, 0f, 1f);
                EditorGUILayout.Slider("Mouth override rate", m_target.Expression.MouthOverrideRate, 0f, 1f);
            }
            EditorGUI.EndDisabledGroup();
        }

        void OnSceneGUI()
        {
            OnSceneGUIOffset();
            if (!Application.isPlaying)
            {
                // offset
                var p = m_target.LookAt.OffsetFromHead;
                Handles.Label(m_target.Head.position, $"fromHead: [{p.x:0.00}, {p.y:0.00}, {p.z:0.00}]");
            }
            else
            {
                m_target.LookAt.OnSceneGUILookAt(m_target.Head);
            }
        }

        void OnSceneGUIOffset()
        {
            var component = target as VRM10Controller;
            if (!component.LookAt.DrawGizmo)
            {
                return;
            }

            var head = component.Head;
            if (head == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var worldOffset = head.localToWorldMatrix.MultiplyPoint(component.LookAt.OffsetFromHead);
            worldOffset = Handles.PositionHandle(worldOffset, head.rotation);

            Handles.DrawDottedLine(head.position, worldOffset, 5);
            Handles.SphereHandleCap(0, head.position, Quaternion.identity, 0.01f, Event.current.type);
            Handles.SphereHandleCap(0, worldOffset, Quaternion.identity, 0.01f, Event.current.type);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(component, "Changed FirstPerson");

                component.LookAt.OffsetFromHead = head.worldToLocalMatrix.MultiplyPoint(worldOffset);
            }
        }
    }
}
