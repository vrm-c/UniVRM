using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniVRM10
{
    public class Vrm10ColliderGroupsWindow : EditorWindow
    {
        const string WINDOW_TITLE = "VRM10CollierGroups";
        const string CollidersPath = "SpringBone.ColliderGroups";
        ObjectField m_target;
        SerializedObject serializedObject;
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

            var prop = new PropertyField { bindingPath = CollidersPath };
            rootVisualElement.Add(prop);
        }
    }
}