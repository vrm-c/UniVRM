using UnityEditor;
using UnityEngine;

namespace VRM
{
    [CustomEditor(typeof(VRMMetaObject))]
    public class VRMMetaObjectEditor : Editor
    {
        class ValidateProperty
        {
            public SerializedProperty m_prop;

            public delegate (string, MessageType) Validator(SerializedProperty prop);
            Validator m_validator;

            public ValidateProperty(SerializedProperty prop, Validator validator)
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

        VRMMetaObject m_target;
        SerializedProperty m_Script;
        SerializedProperty m_exporterVersion;
        SerializedProperty m_thumbnail;
        ValidateProperty m_title;
        ValidateProperty m_version;
        ValidateProperty m_author;
        ValidateProperty m_contact;
        ValidateProperty m_reference;

        SerializedProperty m_AllowedUser;
        SerializedProperty m_ViolentUssage;
        SerializedProperty m_SexualUssage;
        SerializedProperty m_CommercialUssage;
        SerializedProperty m_OtherPermissionUrl;

        SerializedProperty m_LicenseType;
        SerializedProperty m_OtherLicenseUrl;

        static string RequiredMessage(string name)
        {
            switch (M17N.Getter.Lang)
            {
                case M17N.Languages.ja:
                    return $"必須項目。{name} を入力してください";

                case M17N.Languages.en:
                    return $"{name} is required";

                default:
                    throw new System.NotImplementedException();
            }
        }

        private void OnEnable()
        {
            m_target = (VRMMetaObject)target;

            m_Script = serializedObject.FindProperty("m_Script");
            m_exporterVersion = serializedObject.FindProperty(nameof(m_target.ExporterVersion));
            m_thumbnail = serializedObject.FindProperty(nameof(m_target.Thumbnail));
            m_title = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Title)), prop =>
                        {
                            if (string.IsNullOrEmpty(prop.stringValue))
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_version = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Version)), prop =>
                        {
                            if (string.IsNullOrEmpty(prop.stringValue))
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_author = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Author)), prop =>
                        {
                            if (string.IsNullOrEmpty(prop.stringValue))
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_contact = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.ContactInformation)), prop =>
                        {
                            return ("", MessageType.None);
                        });
            m_reference = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Reference)), prop =>
                        {
                            return ("", MessageType.None);
                        });

            m_AllowedUser = serializedObject.FindProperty(nameof(m_target.AllowedUser));
            m_ViolentUssage = serializedObject.FindProperty(nameof(m_target.ViolentUssage));
            m_SexualUssage = serializedObject.FindProperty(nameof(m_target.SexualUssage));
            m_CommercialUssage = serializedObject.FindProperty(nameof(m_target.CommercialUssage));
            m_OtherPermissionUrl = serializedObject.FindProperty(nameof(m_target.OtherLicenseUrl));

            m_LicenseType = serializedObject.FindProperty(nameof(m_target.LicenseType));
            m_OtherLicenseUrl = serializedObject.FindProperty(nameof(m_target.OtherLicenseUrl));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script, true);
            GUI.enabled = true;

            EditorGUILayout.Space();
            VRMMetaObjectGUI(serializedObject);
        }

        bool m_foldoutInfo = true;
        bool m_foldoutPermission = true;
        bool m_foldoutDistribution = true;
        void VRMMetaObjectGUI(SerializedObject so)
        {
            M17N.Getter.OnGuiSelectLang();

            so.Update();

            if (VRMVersion.IsNewer(m_exporterVersion.stringValue))
            {
                EditorGUILayout.HelpBox("Check UniVRM new version. https://github.com/dwango/UniVRM/releases", MessageType.Warning);
            }

            // texture
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_exporterVersion);
                GUI.enabled = true;
                EditorGUILayout.PropertyField(m_thumbnail);
                EditorGUILayout.EndVertical();
                m_thumbnail.objectReferenceValue = TextureField("", (Texture2D)m_thumbnail.objectReferenceValue, 100);
            }
            EditorGUILayout.EndHorizontal();

            m_foldoutInfo = EditorGUILayout.Foldout(m_foldoutInfo, "Information");
            if (m_foldoutInfo)
            {
                m_title.OnGUI();
                m_version.OnGUI();
                m_author.OnGUI();
                m_contact.OnGUI();
                m_reference.OnGUI();
            }
            // EditorGUILayout.LabelField("License ", EditorStyles.boldLabel);
            m_foldoutPermission = EditorGUILayout.Foldout(m_foldoutPermission, "Personation / Characterization Permission");
            if (m_foldoutPermission)
            {
                EditorGUILayout.PropertyField(m_AllowedUser, new GUIContent("A person who can perform with this avatar"), false);
                EditorGUILayout.PropertyField(m_ViolentUssage, new GUIContent("Violent acts using this avatar"));
                EditorGUILayout.PropertyField(m_SexualUssage, new GUIContent("Sexuality acts using this avatar"));
                EditorGUILayout.PropertyField(m_CommercialUssage, new GUIContent("For commercial use"));
                EditorGUILayout.PropertyField(m_OtherPermissionUrl, new GUIContent("Other License Url"));
            }

            m_foldoutDistribution = EditorGUILayout.Foldout(m_foldoutDistribution, "Redistribution / Modifications License");
            if (m_foldoutDistribution)
            {
                var licenseType = m_LicenseType;
                EditorGUILayout.PropertyField(licenseType);
                if ((LicenseType)licenseType.intValue == LicenseType.Other)
                {
                    EditorGUILayout.PropertyField(m_OtherLicenseUrl);
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
