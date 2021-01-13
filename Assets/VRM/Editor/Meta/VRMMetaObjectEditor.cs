using UnityEditor;
using UnityEngine;
using MeshUtility.M17N;
using System.IO;

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
            switch (MeshUtility.M17N.Getter.Lang)
            {
                case MeshUtility.M17N.Languages.ja:
                    return $"必須項目。{name} を入力してください";

                case MeshUtility.M17N.Languages.en:
                    return $"{name} is required";

                default:
                    throw new System.NotImplementedException();
            }
        }

        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }
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

            [LangMsg(Languages.ja, "Camera.main で画像を Render します")]
            [LangMsg(Languages.en, "Create a thumbnail image by Camera.main")]
            SCREENSHOT,

            [LangMsg(Languages.ja, "スクリーンショット")]
            [LangMsg(Languages.en, "Screenshot")]
            SCREENSHOT_BUTTON,

            // [LangMsg(Languages.ja, "")]
            // [LangMsg(Languages.en, "")]
        }

        static string Msg(MessageKeys key)
        {
            return MeshUtility.M17N.Getter.Msg(key);
        }

        bool m_foldoutInfo = true;
        bool m_foldoutPermission = true;
        bool m_foldoutDistribution = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (VRMVersion.IsNewer(m_exporterVersion.stringValue))
            {
                EditorGUILayout.HelpBox("Check UniVRM new version. https://github.com/dwango/UniVRM/releases", MessageType.Warning);
            }

            // texture
            EditorGUILayout.BeginHorizontal();
            {
                // 左側
                EditorGUILayout.BeginVertical();
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(m_exporterVersion);
                    GUI.enabled = true;
                    EditorGUILayout.PropertyField(m_thumbnail);

                    if (Camera.main)
                    {
                        EditorGUILayout.HelpBox(MessageKeys.SCREENSHOT.Msg(), MessageType.Info);
                        if (GUILayout.Button(MessageKeys.SCREENSHOT_BUTTON.Msg()))
                        {
                            TakeScreenShot();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                // 右側
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
                var licenseType = m_LicenseType;
                EditorGUILayout.PropertyField(licenseType);
                if ((LicenseType)licenseType.intValue == LicenseType.Other)
                {
                    EditorGUILayout.PropertyField(m_OtherLicenseUrl);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void TakeScreenShot()
        {
            var dst = SaveDialog(m_target, "png");
            if (string.IsNullOrEmpty(dst))
            {
                return;
            }

            var backup = RenderTexture.active;
            var backup2 = Camera.main.targetTexture;
            var rt = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = rt;
                Camera.main.targetTexture = rt;
                Camera.main.Render();
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();

                var ext = Path.GetExtension(dst).ToLower();
                switch (ext)
                {
                    case ".png":
                        File.WriteAllBytes(dst, tex.EncodeToPNG());
                        break;

                    case ".jpg":
                        File.WriteAllBytes(dst, tex.EncodeToJPG());
                        break;
                }

                var assetPath = MeshUtility.UnityPath.FromFullpath(dst);
                EditorApplication.delayCall += () =>
                {
                    assetPath.ImportAsset();
                    m_target.Thumbnail = assetPath.LoadAsset<Texture2D>();
                };
            }
            finally
            {
                RenderTexture.active = backup;
                Camera.main.targetTexture = backup2;
                Texture2D.DestroyImmediate(tex);
                RenderTexture.DestroyImmediate(rt);
            }
        }

        static string SaveDialog(VRMMetaObject meta, string ext)
        {
            var directory = Application.dataPath;
            var assetPath = AssetDatabase.GetAssetPath(meta);
            if (!string.IsNullOrEmpty(assetPath))
            {
                directory = Path.Combine(directory + "/..", Path.GetDirectoryName(assetPath));
            }
            return EditorUtility.SaveFilePanel(
                    "Save thumbnail",
                    directory,
                    $"thumbnail.{ext}",
                    ext);
        }

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
    }
}
