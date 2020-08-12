using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    [CustomEditor(typeof(VRMMetaObject))]
    public class VRMMetaObjectEditor : Editor
    {
        // SerializedProperty m_ScriptProp;

        class CustomProperty
        {
            public SerializedProperty m_prop;

            public delegate (string, MessageType) Validator(SerializedProperty prop);
            Validator m_validator;

            public CustomProperty(SerializedProperty prop, Validator validator)
            {
                m_prop = prop;
                m_validator = validator;
            }

            public void OnGUI()
            {
                // var old = m_prop.stringValue;
                EditorGUILayout.PropertyField(m_prop);
                var (msg, msgType) = m_validator(m_prop);
                if (!string.IsNullOrEmpty(msg))
                {
                    EditorGUILayout.HelpBox(msg, msgType);
                }
                // return old != m_prop.stringValue;
            }
        }
        List<KeyValuePair<string, CustomProperty>> m_customPropMap = new List<KeyValuePair<string, CustomProperty>>();

        Dictionary<string, SerializedProperty> m_propMap = new Dictionary<string, SerializedProperty>();
        void InitMap(SerializedObject so)
        {
            m_propMap.Clear();
            m_customPropMap.Clear();
            if (so == null)
            {
                return;
            }

            for (var it = so.GetIterator(); it.NextVisible(true);)
            {
                switch (it.name)
                {
                    case "m_Script":
                        break;

                    case "Title":
                    case "Version":
                    case "Author":
                        m_customPropMap.Add(new KeyValuePair<string, CustomProperty>(it.name, new CustomProperty(so.FindProperty(it.name), prop =>
                        {
                            if (string.IsNullOrEmpty(prop.stringValue))
                            {
                                return ($"必須項目。{prop.name} を入力してください", MessageType.Error);
                            }
                            return ("", MessageType.None);
                        })));
                        break;

                    case "ContactInformation":
                    case "Reference":
                        m_customPropMap.Add(new KeyValuePair<string, CustomProperty>(it.name,
                        new CustomProperty(so.FindProperty(it.name), prop =>
                        {
                            return ("", MessageType.None);
                        })));
                        break;

                    default:
                        m_propMap.Add(it.name, so.FindProperty(it.name));
                        break;
                }
                //Debug.LogFormat("{0}", it.name);
            }
        }

        private void OnEnable()
        {
            // m_ScriptProp = serializedObject.FindProperty("m_Script");
            InitMap(serializedObject);
        }        

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // GUI.enabled = false;
            // EditorGUILayout.PropertyField(m_ScriptProp, true);
            // GUI.enabled = true;

            EditorGUILayout.Space();
            VRMMetaObjectGUI(serializedObject);
        }

        bool m_foldoutInfo = true;
        bool m_foldoutPermission = true;
        bool m_foldoutDistribution = true;
        void VRMMetaObjectGUI(SerializedObject so)
        {
            InitMap(so);
            if (m_propMap == null || m_propMap.Count == 0) return;

            so.Update();

            GUI.enabled = false;

            EditorGUILayout.PropertyField(m_propMap["ExporterVersion"]);
            if (VRMVersion.IsNewer(m_propMap["ExporterVersion"].stringValue))
            {
                EditorGUILayout.HelpBox("Check UniVRM new version. https://github.com/dwango/UniVRM/releases", MessageType.Warning);
            }
            GUI.enabled = true;

            m_foldoutInfo = EditorGUILayout.Foldout(m_foldoutInfo, "Information");
            if (m_foldoutInfo)
            {
                // texture
                var thumbnail = m_propMap["Thumbnail"];
                EditorGUILayout.PropertyField(thumbnail);
                thumbnail.objectReferenceValue = TextureField("", (Texture2D)thumbnail.objectReferenceValue, 100);

                foreach (var kv in m_customPropMap)
                {
                    kv.Value.OnGUI();
                }
            }

            EditorGUILayout.LabelField("License ", EditorStyles.boldLabel);

            m_foldoutPermission = EditorGUILayout.Foldout(m_foldoutPermission, "Personation / Characterization Permission");
            if (m_foldoutPermission)
            {
                EditorGUILayout.PropertyField(m_propMap["AllowedUser"], new GUIContent("A person who can perform with this avatar"), false);
                EditorGUILayout.PropertyField(m_propMap["ViolentUssage"], new GUIContent("Violent acts using this avatar"));
                EditorGUILayout.PropertyField(m_propMap["SexualUssage"], new GUIContent("Sexuality acts using this avatar"));
                EditorGUILayout.PropertyField(m_propMap["CommercialUssage"], new GUIContent("For commercial use"));
                EditorGUILayout.PropertyField(m_propMap["OtherPermissionUrl"], new GUIContent("Other License Url"));
            }

            m_foldoutDistribution = EditorGUILayout.Foldout(m_foldoutDistribution, "Redistribution / Modifications License");
            if (m_foldoutDistribution)
            {
                var licenseType = m_propMap["LicenseType"];
                EditorGUILayout.PropertyField(licenseType);
                if ((LicenseType)licenseType.intValue == LicenseType.Other)
                {
                    EditorGUILayout.PropertyField(m_propMap["OtherLicenseUrl"]);
                }
            }

            so.ApplyModifiedProperties();
        }

        private static Texture2D TextureField(string name, Texture2D texture, int size)
        {
            GUILayout.BeginHorizontal();
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            //style.fixedWidth = size;
            GUILayout.Label(name, style);
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(size), GUILayout.Height(size));
            GUILayout.EndVertical();
            return result;
        }
    }
}
