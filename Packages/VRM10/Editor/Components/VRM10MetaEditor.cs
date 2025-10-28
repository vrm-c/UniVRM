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
        SerializedProperty m_thumbnail;
        VRM10MetaProperty m_name;
        VRM10MetaProperty m_version;
        VRM10MetaProperty m_copyright;
        VRM10MetaProperty m_authors;
        VRM10MetaProperty m_references;
        VRM10MetaProperty m_contact;
        VRM10MetaProperty m_thirdPartyLicenses;
        VRM10MetaProperty m_OtherLicenseUrl;

        SerializedProperty m_AvatarPermission;
        SerializedProperty m_ViolentUssage;
        SerializedProperty m_SexualUssage;
        SerializedProperty m_CommercialUssage;
        SerializedProperty m_PoliticalOrReligiousUsage;
        SerializedProperty m_AntisocialOrHateUsage;
        SerializedProperty m_CreditNotation;
        SerializedProperty m_Redistribution;
        SerializedProperty m_Modification;

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

            [LangMsg(Languages.ja, "このアバターを用いて暴力表現を演じること")]
            [LangMsg(Languages.en, "Violent acts using this avatar")]
            VIOLENT_USAGE,

            [LangMsg(Languages.ja, "このアバターを用いて性的表現を演じること")]
            [LangMsg(Languages.en, "Sexuality acts using this avatar")]
            SEXUAL_USAGE,

            [LangMsg(Languages.ja, "商用利用")]
            [LangMsg(Languages.en, "For commercial use")]
            COMMERCIAL_USAGE,

            [LangMsg(Languages.ja, "政治・宗教用途での利用")]
            [LangMsg(Languages.en, "Permits to use this model in political or religious contents")]
            POLITICAL_USAGE,

            [LangMsg(Languages.ja, "反社会的・憎悪表現を含むコンテンツでの利用")]
            [LangMsg(Languages.en, "Permits to use this model in contents contain anti-social activities or hate speeches")]
            ANTI_USAGE,

            [LangMsg(Languages.ja, "再配布・改変に関する許諾範囲")]
            [LangMsg(Languages.en, "Redistribution / Modifications License")]
            REDISTRIBUTION_MODIFICATIONS,

            [LangMsg(Languages.ja, "クレジット表記")]
            [LangMsg(Languages.en, "Forces or abandons to display the credit")]
            MOD_CREDIT,

            [LangMsg(Languages.ja, "再配布")]
            [LangMsg(Languages.en, "Permits redistribution")]
            MOD_REDISTRIBUTION,

            [LangMsg(Languages.ja, "改変")]
            [LangMsg(Languages.en, "Controls the condition to modify")]
            MOD_MODIFICATION,

            [LangMsg(Languages.ja, "その他のライセンス条件があれば、そのURL")]
            [LangMsg(Languages.en, "The URL links of other license")]
            MOD_OTHER,

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
            m_name = CreateValidation(nameof(VRM10ObjectMeta.Name), prop =>
                        {
                            if (string.IsNullOrEmpty(prop.stringValue))
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_version = CreateValidation(nameof(VRM10ObjectMeta.Version));
            m_authors = CreateValidation(nameof(VRM10ObjectMeta.Authors), prop =>
                        {
                            if (prop.arraySize == 0)
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_copyright = CreateValidation(nameof(VRM10ObjectMeta.CopyrightInformation));
            m_contact = CreateValidation(nameof(VRM10ObjectMeta.ContactInformation));
            m_references = CreateValidation(nameof(VRM10ObjectMeta.References));
            m_thirdPartyLicenses = CreateValidation(nameof(VRM10ObjectMeta.ThirdPartyLicenses));
            m_OtherLicenseUrl = CreateValidation(nameof(VRM10ObjectMeta.OtherLicenseUrl));
            m_thumbnail = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.Thumbnail));
            // AvatarPermission
            m_AvatarPermission = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.AvatarPermission));
            m_ViolentUssage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.ViolentUsage));
            m_SexualUssage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.SexualUsage));
            m_CommercialUssage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.CommercialUsage));
            m_PoliticalOrReligiousUsage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.PoliticalOrReligiousUsage));
            m_AntisocialOrHateUsage = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.AntisocialOrHateUsage));
            // Mod
            m_CreditNotation = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.CreditNotation));
            m_Redistribution = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.Redistribution));
            m_Modification = m_rootProperty.FindPropertyRelative(nameof(VRM10ObjectMeta.Modification));

        }

        public static VRM10MetaEditor Create(SerializedObject serializedObject)
        {
            return new VRM10MetaEditor(serializedObject, serializedObject.FindProperty(nameof(VRM10Object.Meta)));
        }

        protected override void RecursiveProperty(SerializedProperty root)
        {
            // texture
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
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
                m_authors.OnGUI();
                m_copyright.OnGUI();
                m_contact.OnGUI();
                m_references.OnGUI();
                m_thirdPartyLicenses.OnGUI();
                m_OtherLicenseUrl.OnGUI();
            }

            m_foldoutPermission = EditorGUILayout.Foldout(m_foldoutPermission, Msg(MessageKeys.PERSONATION));
            if (m_foldoutPermission)
            {
                var backup = EditorGUIUtility.labelWidth;
                RightFixedPropField(m_AvatarPermission, Msg(MessageKeys.ALLOWED_USER));
                RightFixedPropField(m_ViolentUssage, Msg(MessageKeys.VIOLENT_USAGE));
                RightFixedPropField(m_SexualUssage, Msg(MessageKeys.SEXUAL_USAGE));
                RightFixedPropField(m_CommercialUssage, Msg(MessageKeys.COMMERCIAL_USAGE));
                RightFixedPropField(m_PoliticalOrReligiousUsage, Msg(MessageKeys.POLITICAL_USAGE));
                RightFixedPropField(m_AntisocialOrHateUsage, Msg(MessageKeys.ANTI_USAGE));
                EditorGUIUtility.labelWidth = backup;
            }

            m_foldoutDistribution = EditorGUILayout.Foldout(m_foldoutDistribution, Msg(MessageKeys.REDISTRIBUTION_MODIFICATIONS));
            if (m_foldoutDistribution)
            {
                var backup = EditorGUIUtility.labelWidth;
                RightFixedPropField(m_CreditNotation, Msg(MessageKeys.MOD_CREDIT));
                RightFixedPropField(m_Redistribution, Msg(MessageKeys.MOD_REDISTRIBUTION));
                RightFixedPropField(m_Modification, Msg(MessageKeys.MOD_MODIFICATION));
                EditorGUIUtility.labelWidth = backup;
            }
        }
    }
}
