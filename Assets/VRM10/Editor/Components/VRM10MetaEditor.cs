using UniGLTF;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Editor for VRM10ObjectMeta
    /// </summary>
    public class VRM10MetaEditor : SerializedPropertyEditor
    {
        SerializedProperty m_exporterVersion;
        SerializedProperty m_thumbnail;
        VRM10MetaProperty m_name;
        VRM10MetaProperty m_version;
        VRM10MetaProperty m_copyright;
        VRM10MetaProperty m_author;
        VRM10MetaProperty m_contact;
        VRM10MetaProperty m_references;

        SerializedProperty m_AllowedUser;
        SerializedProperty m_ViolentUssage;
        SerializedProperty m_SexualUssage;
        SerializedProperty m_CommercialUssage;
        SerializedProperty m_PoliticalOrReligiousUsage;
        SerializedProperty m_OtherPermissionUrl;

        SerializedProperty m_LicenseType;
        SerializedProperty m_OtherLicenseUrl;

        static string RequiredMessage(string name)
        {
            switch (LanguageGetter.Lang)
            {
                case Languages.ja:
                    return $"必須項目。{name} を入力してください";

                case Languages.en:
                    return $"{name} is required";

                default:
                    throw new System.NotImplementedException();
            }
        }

        enum MessageKeys
        {
            [LangMsg(Languages.ja, "アバターの人格に関する許諾範囲")]
            [LangMsg(Languages.en, "Personation / Characterization Permission")]
            PERSONATION,

            [LangMsg(Languages.ja, "アバターに人格を与えることの許諾範囲")]
            [LangMsg(Languages.en, "A person who can perform with this avatar")]
            ALLOWED_USER,

            [LangMsg(Languages.ja, "このアバターを用いて暴力表現を演じることの許可")]
            [LangMsg(Languages.en, "Violent acts using this avatar")]
            VIOLENT_USAGE,

            [LangMsg(Languages.ja, "このアバターを用いて性的表現を演じることの許可")]
            [LangMsg(Languages.en, "Sexuality acts using this avatar")]
            SEXUAL_USAGE,

            [LangMsg(Languages.ja, "商用利用の許可")]
            [LangMsg(Languages.en, "For commercial use")]
            COMMERCIAL_USAGE,

            [LangMsg(Languages.ja, "再配布・改変に関する許諾範囲")]
            [LangMsg(Languages.en, "Redistribution / Modifications License")]
            REDISTRIBUTION_MODIFICATIONS,

            // [LangMsg(Languages.ja, "")]
            // [LangMsg(Languages.en, "")]
        }

        static string Msg(MessageKeys key)
        {
            return LanguageGetter.Msg(key);
        }

        bool m_foldoutInfo = true;
        bool m_foldoutPermission = true;
        bool m_foldoutDistribution = true;

        static (Rect, Rect) FixedRight(Rect r, int width)
        {
            if (width > r.width)
            {
                width = (int)r.width;
            }
            return (
                new Rect(r.x, r.y, r.width - width, r.height),
                new Rect(r.x + r.width - width, r.y, width, r.height)
            );
        }

        static void RightFixedPropField(SerializedProperty prop, string label)
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            var (left, right) = FixedRight(r, 64);
            // Debug.Log($"{left}, {right}");
            EditorGUI.LabelField(left, label);
            EditorGUI.PropertyField(right, prop, new GUIContent(""), false);
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

        VRM10MetaProperty CreateValidation(string name, VRM10MetaPropertyValidator validator = null)
        {
            return new VRM10MetaProperty(m_rootProperty.FindPropertyRelative(name), validator);
        }

        public VRM10MetaEditor(SerializedObject serializedObject, SerializedProperty property) : base(serializedObject, property)
        {
            m_exporterVersion = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.ExporterVersion));
            m_thumbnail = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.Thumbnail));

            m_name = CreateValidation(nameof(VRM10ObjectMeta.Name), prop =>
                        {
                            if (string.IsNullOrEmpty(prop.stringValue))
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_version = CreateValidation(nameof(VRM10ObjectMeta.Version));
            m_copyright = CreateValidation(nameof(VRM10ObjectMeta.CopyrightInformation));
            m_author = CreateValidation(nameof(VRM10ObjectMeta.Authors), prop =>
                        {
                            if (prop.arraySize == 0)
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_contact = CreateValidation(nameof(VRM10ObjectMeta.ContactInformation));
            m_references = CreateValidation(nameof(VRM10ObjectMeta.References));

            // AvatarPermission
            m_AllowedUser = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.AllowedUser));
            m_ViolentUssage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.ViolentUsage));
            m_SexualUssage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.SexualUsage));
            m_CommercialUssage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.CommercialUsage));
            m_PoliticalOrReligiousUsage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.PoliticalOrReligiousUsage));
            m_OtherPermissionUrl = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.OtherLicenseUrl));

            // m_LicenseType = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.));

            m_OtherLicenseUrl = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.OtherLicenseUrl));
        }

        public static VRM10MetaEditor Create(SerializedObject serializedObject)
        {
            return new VRM10MetaEditor(serializedObject, serializedObject.FindProperty(nameof(VRM10Object.Meta)));
        }

        protected override void RecursiveProperty(SerializedProperty root)
        {
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
                m_name.OnGUI();
                m_version.OnGUI();
                m_author.OnGUI();
                m_contact.OnGUI();
                m_references.OnGUI();
            }
            // EditorGUILayout.LabelField("License ", EditorStyles.boldLabel);
            m_foldoutPermission = EditorGUILayout.Foldout(m_foldoutPermission, Msg(MessageKeys.PERSONATION));
            if (m_foldoutPermission)
            {
                var backup = EditorGUIUtility.labelWidth;
                RightFixedPropField(m_AllowedUser, Msg(MessageKeys.ALLOWED_USER));
                RightFixedPropField(m_ViolentUssage, Msg(MessageKeys.VIOLENT_USAGE));
                RightFixedPropField(m_SexualUssage, Msg(MessageKeys.SEXUAL_USAGE));
                RightFixedPropField(m_CommercialUssage, Msg(MessageKeys.COMMERCIAL_USAGE));
                EditorGUILayout.PropertyField(m_OtherPermissionUrl, new GUIContent("Other License Url"));
                EditorGUIUtility.labelWidth = backup;
            }

            m_foldoutDistribution = EditorGUILayout.Foldout(m_foldoutDistribution, Msg(MessageKeys.REDISTRIBUTION_MODIFICATIONS));
            if (m_foldoutDistribution)
            {
                // var licenseType = m_LicenseType;
                // EditorGUILayout.PropertyField(licenseType);
                // if ((LicenseType)licenseType.intValue == LicenseType.Other)
                // {
                //     EditorGUILayout.PropertyField(m_OtherLicenseUrl);
                // }
            }
        }
    }
}
