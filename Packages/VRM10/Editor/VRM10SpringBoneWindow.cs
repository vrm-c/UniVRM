using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniVRM10
{
    public class VRM10SpringBoneWindow : EditorWindow
    {
        const string SpringsPath = "SpringBone.Springs";
        ObjectField m_target;
        SerializedObject serializedObject;
        ListView m_springs;
        Vrm10Instance Vrm
        {
            get
            {
                return (Vrm10Instance)m_target.value;
            }
            set
            {
                m_target.value = value;
                if (value)
                {
                    serializedObject = new SerializedObject(value);
                    rootVisualElement.Bind(serializedObject);
                }
                else
                {
                    serializedObject = null;
                }
            }
        }

        public static VRM10SpringBoneWindow Show(Vrm10Instance vrm)
        {
            var wnd = GetWindow<VRM10SpringBoneWindow>();
            wnd.titleContent = new GUIContent("VRM10SpringBone");
            wnd.Vrm = vrm;
            return wnd;
        }

        public void CreateGUI()
        {
            m_target = new ObjectField { };
            m_target.SetEnabled(false);
            rootVisualElement.Add(m_target);

            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(splitView);

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
            m_springs.style.marginLeft = 12;
            splitView.Add(m_springs);

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
                    var joint = Vrm.SpringBone.Springs[values[0]].Joints.FirstOrDefault();
                    if (joint)
                    {
                        EditorGUIUtility.PingObject(joint);
                    }
                }
            };
            selected.style.marginLeft = 12;
            splitView.Add(selected);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            HandleUtility.Repaint();

            // 選択中の SpringBone
            if (m_springs != null && m_springs.selectedIndex >= 0
                && m_springs.selectedIndex < Vrm.SpringBone.Springs.Count)
            {
                Handles.color = Color.red;
                var selected = Vrm.SpringBone.Springs[m_springs.selectedIndex];
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
    }
}