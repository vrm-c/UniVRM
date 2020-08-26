using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace VRM.M17N
{
    /// <summary>
    /// 多言語対応
    /// </summary>
    public enum Languages
    {
        ja,
        en,
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class LangMsgAttribute : System.Attribute
    {
        public Languages Language;
        public string Message;

        public LangMsgAttribute(Languages language, string msg)
        {
            Language = language;
            Message = msg;
        }
    }

    public enum VRMExporterWizardMessages
    {
        [LangMsg(Languages.ja, "ExportRootをセットしてください")]
        [LangMsg(Languages.en, "Please set up a ExportRoot for model export")]
        ROOT_EXISTS,

        [LangMsg(Languages.ja, "ExportRootに親はオブジェクトは持てません")]
        [LangMsg(Languages.en, "ExportRoot must be topmost parent")]
        NO_PARENT,

        [LangMsg(Languages.ja, "ExportRootに回転・拡大縮小は持てません。子階層で回転・拡大縮小してください")]
        [LangMsg(Languages.en, "ExportRoot's rotation and scaling are not allowed to change. Please set up rotation and scaling in child node")]
        ROOT_WITHOUT_ROTATION_AND_SCALING_CHANGED,

        [LangMsg(Languages.ja, "シーンに出していない Prefab はエクスポートできません(細かい挙動が違い、想定外の動作をところがあるため)。シーンに展開してからエクスポートしてください")]
        [LangMsg(Languages.en, "Prefab Asset cannot be exported. Prefab Asset has different behaviour with Scene GameObject. Please put the prefab into the scene")]
        PREFAB_CANNOT_EXPORT,

        [LangMsg(Languages.ja, "回転・拡大縮小を持つノードが含まれています。正規化が必用です。Setting の PoseFreeze を有効にしてください")]
        [LangMsg(Languages.en, " Normalization is required. There are nodes (child GameObject) where rotation and scaling are not default. Please enable PoseFreeze")]
        ROTATION_OR_SCALEING_INCLUDED_IN_NODE,

        [LangMsg(Languages.ja, "正規化済みです。Setting の PoseFreeze は不要です")]
        [LangMsg(Languages.en, "Normalization has been done. PoseFreeze is not required")]
        IS_POSE_FREEZE_DONE,

        [LangMsg(Languages.ja, "ExportRootに Animator がありません")]
        [LangMsg(Languages.en, "No Animator in ExportRoot")]
        NO_ANIMATOR,

        [LangMsg(Languages.ja, "Z+ 向きにしてください")]
        [LangMsg(Languages.en, "The model needs to face the positive Z-axis")]
        FACE_Z_POSITIVE_DIRECTION,

        [LangMsg(Languages.ja, "ExportRootの Animator に Avatar がありません")]
        [LangMsg(Languages.en, "No Avatar in ExportRoot's Animator")]
        NO_AVATAR_IN_ANIMATOR,

        [LangMsg(Languages.ja, "ExportRootの Animator.Avatar が不正です")]
        [LangMsg(Languages.en, "Animator.avatar in ExportRoot is not valid")]
        AVATAR_IS_NOT_VALID,

        [LangMsg(Languages.ja, "ExportRootの Animator.Avatar がヒューマノイドではありません。FBX importer の Rig で設定してください")]
        [LangMsg(Languages.en, "Animator.avatar is not humanoid. Please change model's AnimationType to humanoid")]
        AVATAR_IS_NOT_HUMANOID,

        [LangMsg(Languages.ja, "humanoid設定に顎が含まれている。FBX importer の rig 設定に戻って設定を解除することをおすすめします")]
        [LangMsg(Languages.en, "Jaw bone is included. It may not what you intended. Please check the humanoid avatar setting screen")]
        JAW_BONE_IS_INCLUDED,

        [LangMsg(Languages.ja, "ヒエラルキーの中に同じ名前のGameObjectが含まれている。 エクスポートした場合に自動でリネームする")]
        [LangMsg(Languages.en, "There are bones with the same name in the hierarchy. They will be automatically renamed after export")]
        DUPLICATE_BONE_NAME_EXISTS,

        [LangMsg(Languages.ja, "VRMBlendShapeProxyが必要です。先にVRMフォーマットに変換してください")]
        [LangMsg(Languages.en, "VRMBlendShapeProxy is required. Please convert to VRM format first")]
        NEEDS_VRM_BLENDSHAPE_PROXY,

        [LangMsg(Languages.en, "This model contains vertex color")]
        [LangMsg(Languages.ja, "ヒエラルキーに含まれる mesh に頂点カラーが含まれている")]
        VERTEX_COLOR_IS_INCLUDED,

        [LangMsg(Languages.ja, "ヒエラルキーに active なメッシュが含まれていない")]
        [LangMsg(Languages.en, "No active mesh")]
        NO_ACTIVE_MESH,

        [LangMsg(Languages.ja, "Standard, Unlit, MToon 以外のマテリアルは、Standard になります")]
        [LangMsg(Languages.en, "It will export as `Standard` fallback")]
        UNKNOWN_SHADER,

        [LangMsg(Languages.ja, "名前が長すぎる。リネームしてください： ")]
        [LangMsg(Languages.en, "FileName is too long: ")]
        FILENAME_TOO_LONG,
    }

    static class MsgCache<T> where T : Enum
    {
        static Dictionary<Languages, Dictionary<T, string>> s_cache = new Dictionary<Languages, Dictionary<T, string>>();

        static LangMsgAttribute GetAttribute(T value, Languages language)
        {
            var t = typeof(T);
            var memberInfos = t.GetMember(value.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == t);
            var attr = enumValueMemberInfo.GetCustomAttributes(typeof(LangMsgAttribute), false).Select(x => (LangMsgAttribute)x).ToArray();
            if (attr == null || attr.Length == 0)
            {
                return null;
            }
            var match = attr.FirstOrDefault(x => x.Language == language);
            if (match != null)
            {
                return match;
            }
            return attr.First();
        }

        public static string Get(Languages language, T key)
        {
            if (!s_cache.TryGetValue(language, out Dictionary<T, string> map))
            {
                map = new Dictionary<T, string>();

                var t = typeof(T);
                foreach (T value in Enum.GetValues(t))
                {
                    var match = GetAttribute(value, language);
                    // Attribute。無かったら enum の ToString
                    map.Add(value, match != null ? match.Message : key.ToString());
                }

                s_cache.Add(language, map);
            }
            return map[key];
        }
    }
    public static class Getter
    {
        const string LANG_KEY = "VRM_LANG";

        static Languages? m_lang;

        public static Languages Lang
        {
            get
            {
                if (!m_lang.HasValue)
                {
                    m_lang = EnumUtil.TryParseOrDefault<Languages>(EditorPrefs.GetString(LANG_KEY, default(Languages).ToString()));
                }
                return m_lang.Value;
            }
        }

        public static string Msg<T>(T key) where T : Enum
        {
            return M17N.MsgCache<T>.Get(Lang, key);
        }

        public static void OnGuiSelectLang()
        {
            var lang = (M17N.Languages)EditorGUILayout.EnumPopup("lang", Lang);
            if (lang != Lang)
            {
                m_lang = lang;
                EditorPrefs.SetString(LANG_KEY, M17N.Getter.Lang.ToString());
            }
        }
    }
}
