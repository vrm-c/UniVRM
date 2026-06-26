using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniVRM10
{
    public class Vrm10ColliderGroupsWindow : EditorWindow
    {
        const string WINDOW_TITLE = "VRM10CollierGroups";
        const string ListPath = "SpringBone.ColliderGroups";
        ObjectField m_target;
        SerializedObject serializedObject;
        ListView list;
        Vrm10Instance vrm
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

        public static Vrm10ColliderGroupsWindow Show(Vrm10Instance vrm)
        {
            var wnd = GetWindow<Vrm10ColliderGroupsWindow>();
            wnd.titleContent = new GUIContent(WINDOW_TITLE);
            wnd.vrm = vrm;
            return wnd;
        }

        public void CreateGUI()
        {
            m_target = new ObjectField { };
            m_target.SetEnabled(false);
            rootVisualElement.Add(m_target);

            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(splitView);

            list = new ListView
            {
                bindingPath = ListPath,
                makeItem = () =>
                {
                    return new ObjectField();
                },
                bindItem = (v, i) =>
                {
                    var prop = serializedObject.FindProperty($"{ListPath}.Array.data[{i}]");
                    (v as ObjectField).BindProperty(prop);
                },
            };
            list.headerTitle = "Springs";
            list.showFoldoutHeader = true;
            list.showAddRemoveFooter = true;
            list.style.marginLeft = 12;
            splitView.Add(list);

            var selected = new InspectorElement();
#if UNITY_2022_3_OR_NEWER
            list.selectedIndicesChanged += (e) =>
#else
            list.onSelectedIndicesChange += (e) =>
#endif
            {
                var values = e.ToArray();
                if (values.Length > 0)
                {
                    var path = $"{ListPath}.Array.data[{values[0]}]";
                    var prop = serializedObject.FindProperty(path);
                    selected.Bind(new SerializedObject(prop.objectReferenceValue));
                    // var joint = vrm.SpringBone.Springs[values[0]].Joints.FirstOrDefault();
                    // if (joint)
                    // {
                    //     EditorGUIUtility.PingObject(joint);
                    // }
                }
            };
            selected.style.marginLeft = 12;
            splitView.Add(selected);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            VRM10SpringBoneCollider.ColliderGroupsWindowActive = true;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            VRM10SpringBoneCollider.ColliderGroupsWindowActive = false;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            HandleUtility.Repaint();

            if (list != null && list.selectedIndex >= 0
                && list.selectedIndex < vrm.SpringBone.Springs.Count)
            {
                Handles.color = Color.red;
                var selected = vrm.SpringBone.ColliderGroups[list.selectedIndex];
                foreach (var collider in selected.Colliders)
                {
                    if (collider)
                    {
                        // collider.DrawGizmos();
                        Handles.matrix = collider.transform.localToWorldMatrix;
                        Handles.DrawWireDisc(collider.Offset, Vector3.right, collider.Radius);
                        Handles.DrawWireDisc(collider.Offset, Vector3.up, collider.Radius);
                        Handles.DrawWireDisc(collider.Offset, Vector3.forward, collider.Radius);
                    }
                }
            }
        }
    }
}